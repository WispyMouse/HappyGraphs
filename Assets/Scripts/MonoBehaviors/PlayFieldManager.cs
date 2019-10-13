using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayFieldManager : MonoBehaviour
{
    public PlayingCard PlayingCardPF;
    public PlayableSpot PlayableSpotPF;

    public Transform HandLocation;

    Stack<CardData> Deck;
    HashSet<PlayableSpot> PlayableSpots { get; set; } = new HashSet<PlayableSpot>();
    HashSet<PlayingCard> PlayedCards { get; set; } = new HashSet<PlayingCard>();

    private void Awake()
    {

    }

    private void Start()
    {
        Deck = InstantiateDeck();

        PlayingCard thisCard = GeneratePlayingCard(DrawCard());
        thisCard.SetCoordinate(new Coordinate(0, 0));
        PlayedCards.Add(thisCard);
        SetPlayableSpaces();

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

    public void PlayerPlaysCard(PlayingCard card, Coordinate toCoordinate)
    {
        card.SetCoordinate(toCoordinate);
        PlayedCards.Add(card);
        SetPlayableSpaces();
        DealToPlayer();
    }

    CardData DrawCard()
    {
        return Deck.Pop();
    }

    void DealToPlayer()
    {
        PlayingCard newPlayingCard = GeneratePlayingCard(DrawCard());
        newPlayingCard.transform.position = HandLocation.position;
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
            foreach (Coordinate curNeighbor in currentCard.OnCoordinate.GetNeighbors())
            {
                consideredCoordinates.Add(curNeighbor);
            }
        }

        // For each considered Coordinate, create a PlayableSpot for it if there isn't a PlayingCard on that Coordinate already

        foreach (Coordinate consideredCoordinate in consideredCoordinates)
        {
            if (!PlayedCards.Any(card => card.OnCoordinate == consideredCoordinate))
            {
                GeneratePlayableSpot(consideredCoordinate);
            }
        }
    }

    PlayableSpot GeneratePlayableSpot(Coordinate onCoordinate)
    {
        PlayableSpot newSpot = Instantiate(PlayableSpotPF);
        newSpot.SetCoordinate(onCoordinate);
        PlayableSpots.Add(newSpot);
        return newSpot;
    }
}
