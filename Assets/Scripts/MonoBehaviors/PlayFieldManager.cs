using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayFieldManager : MonoBehaviour
{
    public PlayingCard PlayingCardPF;
    public PlayableSpot PlayableSpotPF;

    public CameraManager CameraManagerInstance;

    public Text DeckCountLabel;
    public Text IncompleteCardsValue;

    Stack<CardData> Deck;
    HashSet<PlayableSpot> PlayableSpots { get; set; } = new HashSet<PlayableSpot>();
    HashSet<PlayingCard> PlayedCards { get; set; } = new HashSet<PlayingCard>();
    HashSet<PlayingCard> CardsInHand { get; set; } = new HashSet<PlayingCard>();

    private void Start()
    {
        Deck = InstantiateDeck();

        IncompleteCardsValue.text = "0";
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

        if (Deck.Count > 0)
        {
            DealToPlayer();

            if (NoMovesArePossible())
            {
                NewPlayingField();
            }

            ResetCardsInHandPosition();
        }

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

    void ResetCardsInHandPosition()
    {
        foreach (PlayingCard cardInHand in CardsInHand)
        {
            cardInHand.transform.position = CameraManagerInstance.HandLocation;
        }
    }

    void CheckForHappyCards()
    {
        int incompleteCards = 0;

        foreach (PlayingCard currentCard in PlayedCards)
        {
            if (!currentCard.IsHappy)
            {
                int neighbors = currentCard.OnCoordinate.GetNeighbors().Count(neighbor => PlayedCards.Any(card => card.OnCoordinate == neighbor));

                if (currentCard.RepresentingCard.FaceValue == neighbors)
                {
                    currentCard.BecomeHappy();
                }
                else
                {
                    incompleteCards++;
                }
            }
        }

        IncompleteCardsValue.text = incompleteCards.ToString();
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
        if (PlayableSpots.Count == 0)
        {
            Debug.Log("There are no valid playable spaces, so no moves are possible");
            return true;
        }

        foreach (PlayableSpot spot in PlayableSpots)
        {
            foreach (PlayingCard card in CardsInHand)
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

    void NewPlayingField()
    {
        if (Deck.Count == 0)
        {
            Debug.Log("Trying to make a new playing field, but the deck is empty");
            return;
        }

        if (PlayedCards.Count > 0)
        {
            // For now, we're going to just... jettison the PlayedCards to space.
            // Need to figure out something more, ah, elegant than that
            Debug.Log("Moving the existing playing field far out of the way");
            foreach (PlayingCard card in PlayedCards)
            {
                card.SetCoordinate(new Coordinate(card.OnCoordinate.X + 1000, card.OnCoordinate.Y + 500), DegreesOfSpeed.Quickly);
            }

            CameraManagerInstance.ResetCamera();
        }

        PlayingCard thisCard = GeneratePlayingCard(DrawCard());
        thisCard.SetCoordinate(new Coordinate(0, 0), DegreesOfSpeed.Instantly);
        PlayedCards.Add(thisCard);
        CheckForHappyCards();
        SetPlayableSpaces();
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
