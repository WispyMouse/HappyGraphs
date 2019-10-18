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

    public bool NoMovesArePossible(IEnumerable<PlayingCard> hand)
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
            if (!currentCard.IsHappy && !currentCard.CannotBeCompleted)
            {
                if (ShouldCardBeHappy(currentCard))
                {
                    newCards.Add(currentCard);
                }
            }
        }

        return newCards;
    }

    bool ShouldCardBeHappy(PlayingCard consideredCard)
    {
        int neighbors = consideredCard.OnCoordinate.GetNeighbors().Count(neighbor => PlayedCards.Any(card => card.OnCoordinate == neighbor));

        if (consideredCard.RepresentingCard.FaceValue == neighbors)
        {
            return true;
        }

        return false;
    }

    // This function depends on an up to date SetPlayableSpaces having already been called
    public HashSet<PlayingCard> GetNewlyNotCompleteableCards()
    {
        HashSet<PlayingCard> newCards = new HashSet<PlayingCard>();

        foreach (PlayingCard currentCard in PlayedCards)
        {
            if (!currentCard.IsHappy && !currentCard.CannotBeCompleted)
            {
                if (ShouldCardBeIncompleteable(currentCard))
                {
                    newCards.Add(currentCard);
                }
            }
        }

        return newCards;
    }

    bool ShouldCardBeIncompleteable(PlayingCard consideredCard)
    {
        int requiredNeighbors = consideredCard.RepresentingCard.FaceValue;
        int occuppiedNeighbors = consideredCard.OnCoordinate.GetNeighbors().Count(neighbor => PlayedCards.Any(card => card.OnCoordinate == neighbor));
        int playableSpotNeighbors = consideredCard.OnCoordinate.GetNeighbors().Where(neighbor => PlayableSpots.Any(spot => spot.OnCoordinate == neighbor)).Count();

        if (occuppiedNeighbors + playableSpotNeighbors < requiredNeighbors)
        {
            return true;
        }

        return false;
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

    public bool TryRemoveCardAtCoordinate(Coordinate toRemove, out PlayingCard foundCard)
    {
        foundCard = PlayedCards.FirstOrDefault(card => card.OnCoordinate == toRemove);

        if (foundCard == null)
        {
            return false;
        }

        PlayedCards.Remove(foundCard);

        foreach (PlayingCard neighbor in PlayedCards.Where(card => toRemove.GetNeighbors().Contains(card.OnCoordinate)))
        {
            neighbor.SetHappiness(ShouldCardBeHappy(neighbor));
        }

        SetPlayableSpaces();

        foreach (PlayingCard neighbor in PlayedCards.Where(card => toRemove.GetNeighbors().Contains(card.OnCoordinate)))
        {
            neighbor.SetIncompleteness(ShouldCardBeIncompleteable(neighbor));
        }

        return true;
    }

    public Rect GetDimensions()
    {
        Coordinate leftMost = PlayedCards.Select(x => x.OnCoordinate).OrderBy(x => x.X).First();
        Coordinate rightMost = PlayedCards.Select(x => x.OnCoordinate).OrderByDescending(x => x.X).First();
        Coordinate bottomMost = PlayedCards.Select(x => x.OnCoordinate).OrderBy(x => x.Y).First();
        Coordinate topMost = PlayedCards.Select(x => x.OnCoordinate).OrderByDescending(x => x.Y).First();

        return new Rect(leftMost.WorldspaceCoordinate.x, bottomMost.WorldspaceCoordinate.y,
            rightMost.WorldspaceCoordinate.x - leftMost.WorldspaceCoordinate.x, topMost.WorldspaceCoordinate.y - bottomMost.WorldspaceCoordinate.y);
    }
}
