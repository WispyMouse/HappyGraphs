﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayFieldManager : MonoBehaviour
{
    public PlayFieldRuntime PlayFieldRuntimePF;
    public PlayingCard PlayingCardPF;

    public CameraManager CameraManagerInstance;

    public Text DeckCountLabel;
    public Text IncompleteCardsValue;

    public InputField CardsPerRankField;
    public static int CardsPerRankRule = 4;

    public InputField HandSizeField;
    public static int HandSizeRule = 1;
    private int handSize { get; set; }

    Stack<CardData> deck;
    List<PlayingCard> cardsInHand { get; set; } = new List<PlayingCard>();

    // The running total of previous play field's incomplete cards
    int previousPlayfieldIncompleteCards { get; set; } = 0;
    public PlayFieldRuntime ActivePlayField { get; set; }

    private void Start()
    {
        CardsPerRankField.text = CardsPerRankRule.ToString();
        HandSizeField.text = HandSizeRule.ToString();

        deck = InstantiateDeck();

        handSize = Mathf.Min(HandSizeRule, deck.Count - 1);

        IncompleteCardsValue.text = "0";
        NewPlayingField();

        DealToPlayer();
    }

    Stack<CardData> InstantiateDeck()
    {
        // First, we want to make a list of suits equal to the amount of cards per rank
        List<Suit> suits = new List<Suit>();
        List<Suit> allSuits = new List<Suit>(GetAllSuits());

        for (int cardCount = 0; cardCount < CardsPerRankRule; cardCount++)
        {
            suits.Add(allSuits[cardCount % allSuits.Count]);
        }

        // Then, for each rank and for each suit in the list, add a card
        Stack<CardData> newDeck = new Stack<CardData>();

        for (int value = 1; value <= 4; value++)
        {
            foreach (Suit curSuit in suits)
            {
                newDeck.Push(new CardData(curSuit, value));
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

    public bool TryPlayerPlaysCard(PlayingCard playedCard, Coordinate toCoordinate)
    {
        if (!ActivePlayField.IsSpotValidForCard(toCoordinate, playedCard))
        {
            return false;
        }

        cardsInHand.Remove(playedCard);
        ActivePlayField.PlayCard(playedCard, toCoordinate);

        CameraManagerInstance.NewPlacement(toCoordinate.WorldspaceCoordinate);

        CheckForNewHappyCards();
        ActivePlayField.SetPlayableSpaces();
        CheckForNewCannotBeCompletedCards();

        DealToPlayer();

        if (ActivePlayField.NoMovesArePossible(cardsInHand))
        {
            NewPlayingField();
        }

        ResetCardsInHandPosition();

        return true;
    }

    CardData DrawCard()
    {
        CardData toReturn = deck.Pop();
        DeckCountLabel.text = $"x{deck.Count}";
        return toReturn;
    }

    void DealToPlayer()
    {
        if (deck.Count > 0)
        {
            PlayingCard newPlayingCard = GeneratePlayingCard(DrawCard());
            newPlayingCard.transform.position = CameraManagerInstance.HandLocation;
            cardsInHand.Add(newPlayingCard);

            if (cardsInHand.Count < handSize)
            {
                DealToPlayer();
                return;
            }
        }
        else
        {
            Debug.Log("The deck is empty!");
        }

        ResetCardsInHandPosition();
    }

    PlayingCard GeneratePlayingCard(CardData forCard)
    {
        PlayingCard newCard = Instantiate(PlayingCardPF, transform);
        newCard.SetCardData(forCard);
        return newCard;
    }

    void ResetCardsInHandPosition()
    {
        for (int cardIndex = 0; cardIndex < cardsInHand.Count; cardIndex++)
        {
            cardsInHand[cardIndex].transform.position = CameraManagerInstance.GetHandPosition(cardIndex, handSize);
        }
    }

    void CheckForNewHappyCards()
    {
        foreach (PlayingCard currentCard in ActivePlayField.GetNewlyHappyCards())
        {
            currentCard.BecomeHappy();
        }
    }

    void CheckForNewCannotBeCompletedCards()
    {
        foreach (PlayingCard currentCard in ActivePlayField.GetNewlyNotCompleteableCards())
        {
            currentCard.MarkAsCannotComplete();
        }

        IncompleteCardsValue.text = (previousPlayfieldIncompleteCards + ActivePlayField.GetIncompleteCards().Count).ToString();
    }

    void NewPlayingField()
    {
        if (deck.Count == 0)
        {
            Debug.Log("Trying to make a new playing field, but the deck is empty");
            return;
        }

        if (ActivePlayField != null)
        {
            previousPlayfieldIncompleteCards += ActivePlayField.GetIncompleteCards().Count;

            // For now, we're going to just... jettison the PlayedCards to space.
            // Need to figure out something more, ah, elegant than that
            Debug.Log("Moving the existing playing field far out of the way");
            ActivePlayField.transform.position = (Vector3.right * 1000 + Vector3.up * 500);
            CameraManagerInstance.ResetCamera();
        }

        ActivePlayField = ObjectPooler.GetObject<PlayFieldRuntime>(PlayFieldRuntimePF, transform);
        ActivePlayField.SeedInitialCard(DrawCard());
        
        CheckForNewHappyCards();
        ActivePlayField.SetPlayableSpaces();
        CheckForNewCannotBeCompletedCards();
    }

    public void UpdateValidityOfPlayableSpots(PlayingCard forCard)
    {
        ActivePlayField.UpdateValidityOfPlayableSpots(forCard);
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void CardsPerRankChanged()
    {
        int parsedValue;

        if (int.TryParse(CardsPerRankField.text, out parsedValue))
        {
            CardsPerRankRule = Mathf.Max(1, parsedValue);
            CardsPerRankField.text = CardsPerRankRule.ToString();
        }
        else
        {
            CardsPerRankField.text = CardsPerRankRule.ToString();
        }
    }

    public void HandSizeRuleChanged()
    {
        int parsedValue;

        if (int.TryParse(HandSizeField.text, out parsedValue))
        {
            HandSizeRule = Mathf.Max(1, parsedValue);
            HandSizeField.text = HandSizeRule.ToString();
        }
        else
        {
            HandSizeField.text = HandSizeRule.ToString();
        }
    }
}
