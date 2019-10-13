﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PlayFieldManager : MonoBehaviour
{
    public PlayingCard PlayingCardPF;
    public PlayableSpot PlayableSpotPF;

    public CameraManager CameraManagerInstance;

    public Text DeckCountLabel;

    Stack<CardData> Deck;
    HashSet<PlayableSpot> PlayableSpots { get; set; } = new HashSet<PlayableSpot>();
    HashSet<PlayingCard> PlayedCards { get; set; } = new HashSet<PlayingCard>();
    HashSet<PlayingCard> CardsInHand { get; set; } = new HashSet<PlayingCard>();

    private void Start()
    {
        Deck = InstantiateDeck();

        NewPlayingField();

        DealToPlayer();
    }

    Stack<CardData> InstantiateDeck()
    {
        Stack<CardData> newDeck = new Stack<CardData>();
        
        for (int value = 1; value <= 4; value++)
        {
            foreach (Suit suitType in GetAllSuits())
            {
                newDeck.Push(new CardData(suitType, value));
            }
        }

        ShuffleDeck(newDeck);

        return newDeck;
    }

    void ShuffleDeck(Stack<CardData> toShuffle)
    {
        CardData[] originalDeck = toShuffle.ToArray();
        toShuffle.Clear();
        
        foreach(CardData card in originalDeck.OrderBy(card => Random.Range(0f, 1f)))
        {
            toShuffle.Push(card);
        }
    }

    static IEnumerable<Suit> GetAllSuits()
    {
        return new Suit[] { Suit.Club, Suit.Diamond, Suit.Heart, Suit.Spade };
    }

    PlayingCard GeneratePlayingCard(CardData forCard)
    {
        PlayingCard newCard = Instantiate(PlayingCardPF);
        newCard.SetCardData(forCard);
        return newCard;
    }

    public bool TryPlayerPlaysCard(PlayingCard playedCard, Coordinate toCoordinate)
    {
        if (!IsSpotValidForCard(toCoordinate, playedCard))
        {
            return false;
        }

        CardsInHand.Remove(playedCard);
        playedCard.SetCoordinate(toCoordinate);
        PlayedCards.Add(playedCard);

        CameraManagerInstance.NewPlacement(toCoordinate.WorldspaceCoordinate);

        CheckForHappyCards();
        SetPlayableSpaces();
        DealToPlayer();
        return true;
    }

    CardData DrawCard()
    {
        CardData toReturn = Deck.Pop();
        DeckCountLabel.text = $"x{Deck.Count}";
        return toReturn;
    }

    void DealToPlayer()
    {
        if (Deck.Count > 0)
        {
            PlayingCard newPlayingCard = GeneratePlayingCard(DrawCard());
            newPlayingCard.transform.position = CameraManagerInstance.HandLocation;
            CardsInHand.Add(newPlayingCard);
        }
        else
        {
            Debug.Log("The deck is empty!");
        }        
    }

    void CheckForHappyCards()
    {
        foreach (PlayingCard currentCard in PlayedCards)
        {
            if (!currentCard.IsHappy)
            {
                int neighbors = currentCard.OnCoordinate.GetNeighbors().Count(neighbor => PlayedCards.Any(card => card.OnCoordinate == neighbor));

                if (currentCard.RepresentingCard.FaceValue == neighbors)
                {
                    currentCard.BecomeHappy();
                }
            }
        }
    }

    void SetPlayableSpaces()
    {
        // Clear out the old collection of spaces

        foreach (PlayableSpot spot in PlayableSpots)
        {
            Destroy(spot.gameObject); // TODO: Object pool these
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
        PlayableSpot newSpot = Instantiate(PlayableSpotPF);
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

    bool IsSpotValidForCard(Coordinate forCoordinate, PlayingCard forCard)
    {
        int neighborsAtPosition = forCoordinate.GetNeighbors().Count(neighbor => PlayedCards.Any(card => card.OnCoordinate == neighbor));
        return forCard.RepresentingCard.FaceValue >= neighborsAtPosition;
    }

    bool NoMovesArePossible()
    {
        return false;
    }

    void NewPlayingField()
    {
        if (Deck.Count == 0)
        {
            Debug.Log("Trying to make a new playing field, but the deck is empty");
            return;
        }

        PlayingCard thisCard = GeneratePlayingCard(DrawCard());
        thisCard.SetCoordinate(new Coordinate(0, 0));
        PlayedCards.Add(thisCard);
        SetPlayableSpaces();
    }
}
