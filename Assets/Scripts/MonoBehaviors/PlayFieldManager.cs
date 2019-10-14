using System.Collections;
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

    Stack<CardData> Deck;
    HashSet<PlayingCard> CardsInHand { get; set; } = new HashSet<PlayingCard>();

    // The running total of previous play field's incomplete cards
    int PreviousPlayfieldIncompleteCards { get; set; } = 0;
    public PlayFieldRuntime ActivePlayField { get; set; }

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

    public bool TryPlayerPlaysCard(PlayingCard playedCard, Coordinate toCoordinate)
    {
        if (!ActivePlayField.IsSpotValidForCard(toCoordinate, playedCard))
        {
            return false;
        }

        CardsInHand.Remove(playedCard);
        ActivePlayField.PlayCard(playedCard, toCoordinate);

        CameraManagerInstance.NewPlacement(toCoordinate.WorldspaceCoordinate);

        CheckForHappyCards();
        ActivePlayField.SetPlayableSpaces();

        if (Deck.Count > 0)
        {
            DealToPlayer();

            if (ActivePlayField.NoMovesArePossible(CardsInHand))
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

    PlayingCard GeneratePlayingCard(CardData forCard)
    {
        PlayingCard newCard = Instantiate(PlayingCardPF, transform);
        newCard.SetCardData(forCard);
        return newCard;
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
        HashSet<PlayingCard> newlyHappyCards = ActivePlayField.GetNewlyHappyCards();
    
        foreach (PlayingCard currentCard in newlyHappyCards)
        {
            currentCard.BecomeHappy();
        }

        IncompleteCardsValue.text = (PreviousPlayfieldIncompleteCards + ActivePlayField.GetIncompleteCards().Count).ToString();
    }

    void NewPlayingField()
    {
        if (Deck.Count == 0)
        {
            Debug.Log("Trying to make a new playing field, but the deck is empty");
            return;
        }

        if (ActivePlayField != null)
        {
            PreviousPlayfieldIncompleteCards += ActivePlayField.GetIncompleteCards().Count;

            // For now, we're going to just... jettison the PlayedCards to space.
            // Need to figure out something more, ah, elegant than that
            Debug.Log("Moving the existing playing field far out of the way");
            ActivePlayField.transform.position = (Vector3.right * 1000 + Vector3.up * 500);
            CameraManagerInstance.ResetCamera();
        }

        ActivePlayField = ObjectPooler.GetObject<PlayFieldRuntime>(PlayFieldRuntimePF, transform);
        ActivePlayField.SeedInitialCard(DrawCard());
        
        CheckForHappyCards();
        ActivePlayField.SetPlayableSpaces();
    }

    public void UpdateValidityOfPlayableSpots(PlayingCard forCard)
    {
        ActivePlayField.UpdateValidityOfPlayableSpots(forCard);
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
