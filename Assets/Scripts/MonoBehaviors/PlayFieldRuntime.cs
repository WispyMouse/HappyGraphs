using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayFieldRuntime : MonoBehaviour
{
    public PlayingCard PlayingCardPF;
    public PlayableSpot PlayableSpotPF;

    HashSet<PlayingCard> PlayedCards { get; set; } = new HashSet<PlayingCard>();
    HashSet<PlayableSpot> PlayableSpots { get; set; } = new HashSet<PlayableSpot>();

    PlayingCard GeneratePlayingCard(CardData forCard)
    {
        PlayingCard newCard = ObjectPooler.GetObject<PlayingCard>(PlayingCardPF, transform);
        newCard.SetCardData(forCard);
        return newCard;
    }

    public void SeedInitialCard(CardData forCard)
    {
        PlayingCard newCard = GeneratePlayingCard(forCard);
        newCard.SetCoordinate(new Coordinate(0, 0), DegreesOfSpeed.Instantly);
        PlayedCards.Add(newCard);
    }

    public bool IsSpotValidForCard(Coordinate forCoordinate, PlayingCard forCard)
    {
        int neighborsAtPosition = forCoordinate.GetNeighbors().Count(neighbor => PlayedCards.Any(card => card.OnCoordinate == neighbor));
        return forCard.RepresentingCard.FaceValue >= neighborsAtPosition;
    }

    public bool NoMovesArePossible(HashSet<PlayingCard> hand)
    {
        if (PlayableSpots.Count == 0)
        {
            Debug.Log("There are no valid playable spaces, so no moves are possible");
            return true;
        }

        foreach (PlayableSpot spot in PlayableSpots)
        {
            foreach (PlayingCard card in hand)
            {
                if (IsSpotValidForCard(spot.OnCoordinate, card))
                {
                    return false;
                }
            }
        }

        Debug.Log("None of the cards in hand can validly be played in any playable space, so no moves are possible");
        return true;
    }

    public void PlayCard(PlayingCard toPlay, Coordinate onCoordinate)
    {
        toPlay.SetCoordinate(onCoordinate);
        toPlay.transform.SetParent(transform, true);
        PlayedCards.Add(toPlay);
    }

    public HashSet<PlayingCard> GetNewlyHappyCards()
    {
        HashSet<PlayingCard> newCards = new HashSet<PlayingCard>();

        foreach (PlayingCard currentCard in PlayedCards)
        {
            if (!currentCard.IsHappy)
            {
                int neighbors = currentCard.OnCoordinate.GetNeighbors().Count(neighbor => PlayedCards.Any(card => card.OnCoordinate == neighbor));

                if (currentCard.RepresentingCard.FaceValue == neighbors)
                {
                    newCards.Add(currentCard);
                }
            }
        }

        return newCards;
    }

    public HashSet<PlayingCard> GetIncompleteCards()
    {
        return new HashSet<PlayingCard>(PlayedCards.Where(card => !card.IsHappy));
    }

    public void SetPlayableSpaces()
    {
        // Clear out the old collection of spaces

        foreach (PlayableSpot spot in PlayableSpots)
        {
            ObjectPooler.ReturnObject<PlayableSpot>(spot);
        }

        PlayableSpots.Clear();

        // For every played card, add every neighboring coordinate to a HashSet (no duplicates)

        HashSet<Coordinate> consideredCoordinates = new HashSet<Coordinate>();

        foreach (PlayingCard currentCard in PlayedCards)
        {
            if (currentCard.IsHappy)
            {
                continue;
            }

            foreach (Coordinate curNeighbor in currentCard.OnCoordinate.GetNeighbors())
            {
                consideredCoordinates.Add(curNeighbor);
            }
        }

        // For each considered Coordinate, if all the following is true, create a Playable Spot:
        // - there are no cards on that spot
        // - there are no happy cards neighboring that spot

        foreach (Coordinate consideredCoordinate in consideredCoordinates)
        {
            if (PlayedCards.Any(card => card.OnCoordinate == consideredCoordinate))
            {
                continue;
            }

            if (PlayedCards.Where(card => card.IsHappy).Any(card => consideredCoordinate.GetNeighbors().Any(neighbor => neighbor == card.OnCoordinate)))
            {
                continue;
            }

            GeneratePlayableSpot(consideredCoordinate);
        }

        UpdateValidityOfPlayableSpots(null);
    }

    PlayableSpot GeneratePlayableSpot(Coordinate onCoordinate)
    {
        PlayableSpot newSpot = ObjectPooler.GetObject<PlayableSpot>(PlayableSpotPF, transform);
        newSpot.SetCoordinate(onCoordinate);
        PlayableSpots.Add(newSpot);
        return newSpot;
    }

    public void UpdateValidityOfPlayableSpots(PlayingCard forCard)
    {
        if (forCard == null)
        {
            foreach (PlayableSpot spot in PlayableSpots)
            {
                spot.SetValidity(SpotValidity.Possible);
            }
        }
        else
        {
            foreach (PlayableSpot spot in PlayableSpots)
            {
                if (IsSpotValidForCard(spot.OnCoordinate, forCard))
                {
                    spot.SetValidity(SpotValidity.Valid);
                }
                else
                {
                    spot.SetValidity(SpotValidity.Invalid);
                }
            }
        }
    }
}
