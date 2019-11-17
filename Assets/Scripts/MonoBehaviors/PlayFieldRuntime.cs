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
    public PlayFieldData CurrentPlayField { get; set; } = new PlayFieldData();

    bool showingActualValues { get; set; } = false;

    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            if (!showingActualValues)
            {
                foreach (PlayingCard card in PlayedCards)
                {
                    card.ActualValueToggle(true);
                }
                showingActualValues = true;
            }
        }
        else
        {
            if (showingActualValues)
            {
                foreach (PlayingCard card in PlayedCards)
                {
                    card.ActualValueToggle(false);
                }
                showingActualValues = false;
            }
        }
    }

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
        CurrentPlayField.SetCard(forCard, new Coordinate(0, 0));
    }

    public bool IsSpotValidForCard(PlayingCard forCard, Coordinate onCoordinate)
    {
        return CurrentPlayField.IsSpotValidForCard(forCard.RepresentingCard, onCoordinate);
    }

    public bool NoMovesArePossible(List<PlayingCard> hand)
    {
        if (PlayableSpots.Count == 0)
        {
            Debug.Log("There are no valid playable spaces, so no moves are possible");
            return true;
        }

        return !CurrentPlayField.AreAnyMovesPossible(hand.Select(card => card.RepresentingCard).ToList());
    }

    public void PlayCard(PlayingCard toPlay, Coordinate onCoordinate)
    {
        toPlay.SetCoordinate(onCoordinate);
        toPlay.transform.SetParent(transform, true);
        PlayedCards.Add(toPlay);
        CurrentPlayField.SetCard(toPlay.RepresentingCard, onCoordinate);
    }

    public HashSet<PlayingCard> GetNewlyHappyCards()
    {
        HashSet<PlayingCard> newCards = new HashSet<PlayingCard>();
        HashSet<Coordinate> happyCoordinates = CurrentPlayField.GetHappyCoordinates();

        foreach (PlayingCard currentCard in PlayedCards.Where(card => happyCoordinates.Contains(card.OnCoordinate)))
        {
            if (!currentCard.IsHappy)
            {
                newCards.Add(currentCard);
            }
        }

        return newCards;
    }

    public HashSet<PlayingCard> GetNewlyNotCompleteableCards()
    {
        HashSet<PlayingCard> newCards = new HashSet<PlayingCard>();
        HashSet<Coordinate> incompleteableCoordinates = CurrentPlayField.GetIncompleteableCoordinates();

        foreach (PlayingCard currentCard in PlayedCards.Where(card => incompleteableCoordinates.Contains(card.OnCoordinate)))
        {
            if (!currentCard.CannotBeCompleted)
            {
                newCards.Add(currentCard);
            }
        }

        return newCards;
    }

    public HashSet<PlayingCard> GetHappyCards()
    {
        return new HashSet<PlayingCard>(PlayedCards.Where(card => card.IsHappy));
    }

    public HashSet<PlayingCard> GetIncompleteCards()
    {
        return new HashSet<PlayingCard>(PlayedCards.Where(card => !card.IsHappy));
    }

    public void SetPlayableSpaces()
    {
        foreach (PlayableSpot spot in PlayableSpots)
        {
            ObjectPooler.ReturnObject<PlayableSpot>(spot);
        }

        PlayableSpots.Clear();

        HashSet<Coordinate> validPlayableSpaces = CurrentPlayField.GetValidPlayableSpaces();
        foreach (Coordinate curCoordinate in validPlayableSpaces)
        {
            GeneratePlayableSpot(curCoordinate);
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
                spot.ClearWouldEffects();
            }
        }
        else
        {
            foreach (PlayableSpot spot in PlayableSpots)
            {
                if (!IsSpotValidForCard(forCard, spot.OnCoordinate))
                {
                    spot.SetValidity(SpotValidity.Invalid);
                    spot.ClearWouldEffects();
                }
                else
                {
                    Dictionary<PlayingCard, int> possibleIncompleteableCards, possibleHappyCards, allNeighbors;
                    GetNewHypotheticalPlacementEffects(forCard, spot.OnCoordinate, out possibleIncompleteableCards, out possibleHappyCards, out allNeighbors);

                    if (possibleIncompleteableCards.ContainsKey(forCard))
                    {
                        spot.SetValidity(SpotValidity.PossibleUnsolveable);
                    }
                    else
                    {
                        spot.SetValidity(SpotValidity.Valid);
                    }

                    spot.SetWouldEffects(possibleHappyCards, possibleIncompleteableCards, allNeighbors);
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
        CurrentPlayField.RemoveCard(toRemove);

        HashSet<Coordinate> playableSpaces = CurrentPlayField.GetValidPlayableSpaces();

        foreach (PlayingCard card in PlayedCards)
        {
            card.SetHappiness(CurrentPlayField.ShouldCoordinateBeHappy(card.OnCoordinate));
            card.SetIncompleteness(CurrentPlayField.ShouldCoordinateBeIncompletable(card.OnCoordinate));
        }

        SetPlayableSpaces();

        return true;
    }

    public Rect GetDimensions()
    {
        if (PlayedCards.Count == 0)
        {
            return new Rect();
        }

        Coordinate leftMost = PlayedCards.Select(x => x.OnCoordinate).OrderBy(x => x.X).First();
        Coordinate rightMost = PlayedCards.Select(x => x.OnCoordinate).OrderByDescending(x => x.X).First();
        Coordinate bottomMost = PlayedCards.Select(x => x.OnCoordinate).OrderBy(x => x.Y).First();
        Coordinate topMost = PlayedCards.Select(x => x.OnCoordinate).OrderByDescending(x => x.Y).First();

        return new Rect(leftMost.GetWorldspacePosition().x, bottomMost.GetWorldspacePosition().y,
            rightMost.GetWorldspacePosition().x - leftMost.GetWorldspacePosition().x, topMost.GetWorldspacePosition().y - bottomMost.GetWorldspacePosition().y);
    }

    public void GetNewHypotheticalPlacementEffects(PlayingCard forCard, Coordinate onCoordinate, 
        out Dictionary<PlayingCard, int> newHypotheticalIncompleteableCards, out Dictionary<PlayingCard, int> newHypotheticalHappyCards, out Dictionary<PlayingCard, int> allNeighbors)
    {
        newHypotheticalIncompleteableCards = new Dictionary<PlayingCard, int>();
        newHypotheticalHappyCards = new Dictionary<PlayingCard, int>();
        allNeighbors = new Dictionary<PlayingCard, int>();

        PlayFieldData hypotheticalPlayField = CurrentPlayField.CloneData();
        hypotheticalPlayField.SetCard(forCard.RepresentingCard, onCoordinate);

        IEnumerable<Coordinate> newHypotheticalIncompleteableCoordinates = hypotheticalPlayField.GetIncompleteableCoordinates().Except(CurrentPlayField.GetIncompleteableCoordinates());
        IEnumerable<Coordinate> newHypotheticalHappyCoordinates = hypotheticalPlayField.GetHappyCoordinates().Except(CurrentPlayField.GetHappyCoordinates());

        newHypotheticalIncompleteableCards = PlayedCards.Where(card => newHypotheticalIncompleteableCoordinates.Contains(card.OnCoordinate))
                .ToDictionary(card => card, card => hypotheticalPlayField.OccuppiedNeighborsAtCoordinate(card.OnCoordinate));

        newHypotheticalHappyCards = PlayedCards.Where(card => newHypotheticalHappyCoordinates.Contains(card.OnCoordinate))
            .ToDictionary(card => card, card => hypotheticalPlayField.OccuppiedNeighborsAtCoordinate(card.OnCoordinate));

        allNeighbors = PlayedCards.Where(card => onCoordinate.GetNeighbors().Contains(card.OnCoordinate))
            .ToDictionary(card => card, card => hypotheticalPlayField.OccuppiedNeighborsAtCoordinate(card.OnCoordinate));

        allNeighbors.Add(forCard, hypotheticalPlayField.OccuppiedNeighborsAtCoordinate(onCoordinate));

        if (newHypotheticalIncompleteableCoordinates.Contains(onCoordinate))
        {
            newHypotheticalIncompleteableCards.Add(forCard, hypotheticalPlayField.OccuppiedNeighborsAtCoordinate(onCoordinate));
        }

        if (newHypotheticalHappyCoordinates.Contains(onCoordinate))
        {
            newHypotheticalHappyCards.Add(forCard, hypotheticalPlayField.OccuppiedNeighborsAtCoordinate(onCoordinate));
        }
    }

    public void UpdateCardVisuals()
    {
        foreach (PlayingCard currentCard in GetNewlyHappyCards())
        {
            currentCard.SetHappiness(true);
        }

        foreach (PlayingCard currentCard in GetNewlyNotCompleteableCards())
        {
            currentCard.SetIncompleteness(true);
        }

        foreach (PlayingCard currentCard in GetIncompleteCards())
        {
            currentCard.SetNeighborCount(CurrentPlayField.OccuppiedNeighborsAtCoordinate(currentCard.OnCoordinate));
        }
    }
}
