using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayFieldManager : MonoBehaviour
{
    public PlayingCard PlayingCardPF;
    public Camera GameplayCamera;
    public Transform HandLocation;

    Stack<CardData> Deck;

    private void Awake()
    {

    }

    private void Start()
    {
        Deck = InstantiateDeck();
        CardData CenterCard = DrawCard();
        PlayCard(CenterCard, new Coordinate(0, 0));
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

    public static IEnumerable<Suit> GetAllSuits()
    {
        return new Suit[] { Suit.Club, Suit.Diamond, Suit.Heart, Suit.Spade };
    }

    public void PlayCard(CardData card, Coordinate toCoordinate)
    {
        PlayingCard thisCard = GeneratePlayingCard(card);
        PlayCard(thisCard, toCoordinate);
    }

    public PlayingCard GeneratePlayingCard(CardData forCard)
    {
        PlayingCard newCard = Instantiate(PlayingCardPF);
        newCard.SetCardData(forCard);
        return newCard;
    }

    public void PlayCard(PlayingCard card, Coordinate toCoordinate)
    {
        card.SetCoordinate(toCoordinate);
    }

    public CardData DrawCard()
    {
        return Deck.Pop();
    }

    public void DealToPlayer()
    {
        PlayingCard newPlayingCard = GeneratePlayingCard(DrawCard());
        newPlayingCard.transform.position = HandLocation.position;
    }
}
