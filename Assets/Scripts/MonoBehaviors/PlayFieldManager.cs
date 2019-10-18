using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum GridType { FourWay, EightWay }
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

    public static GridType GridTypeRule = GridType.FourWay;
    public Dropdown GridTypeDropdown;
    public static GridType ActiveGridType = GridType.FourWay;

    Stack<CardData> deck;
    List<PlayingCard> cardsInHand { get; set; } = new List<PlayingCard>();

    // The running total of previous play field's incomplete cards
    int previousPlayfieldIncompleteCards { get; set; } = 0;
    public PlayFieldRuntime ActivePlayField { get; set; }

    Stack<GameAction> GameActions { get; set; } = new Stack<GameAction>();

    private void Start()
    {
        CardsPerRankField.text = CardsPerRankRule.ToString();
        HandSizeField.text = HandSizeRule.ToString();
        ActiveGridType = GridTypeRule;

        switch (ActiveGridType)
        {
            default:
            case GridType.FourWay:
                GridTypeDropdown.value = 0;
                break;
            case GridType.EightWay:
                GridTypeDropdown.value = 1;
                break;
        }

        deck = InstantiateDeck();

        handSize = Mathf.Min(HandSizeRule, deck.Count - 1);

        IncompleteCardsValue.text = "0";
        NewPlayingField();

        DealToPlayer(false);
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

        int maxCardValue = 4;

        switch (ActiveGridType)
        {
            default:
            case GridType.FourWay:
                maxCardValue = 4;
                break;
            case GridType.EightWay:
                maxCardValue = 8;
                break;
        }

        for (int value = 1; value <= maxCardValue; value++)
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

        GameActions.Push(GameAction.FromCardPlayed(playedCard.RepresentingCard, toCoordinate));

        CameraManagerInstance.UpdateCamera(ActivePlayField);

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
        CardData drawnCard = deck.Pop();
        DeckCountLabel.text = $"x{deck.Count}";
        
        return drawnCard;
    }

    void DealToPlayer(bool logAction = true)
    {
        if (deck.Count > 0)
        {
            PlayingCard newPlayingCard = GeneratePlayingCard(DrawCard());
            newPlayingCard.transform.position = CameraManagerInstance.HandLocation;
            cardsInHand.Add(newPlayingCard);

            if (logAction)
            {
                GameActions.Push(GameAction.FromCardDrawnFromDeck(newPlayingCard.RepresentingCard));
            }

            if (cardsInHand.Count < handSize)
            {
                DealToPlayer(logAction);
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
            currentCard.SetHappiness(true);
        }
    }

    void CheckForNewCannotBeCompletedCards()
    {
        foreach (PlayingCard currentCard in ActivePlayField.GetNewlyNotCompleteableCards())
        {
            currentCard.SetIncompleteness(true);
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

    public void GridTypeRuleChanged()
    {
        switch (GridTypeDropdown.value)
        {
            default:
            case 0:
                GridTypeRule = GridType.FourWay;
                break;
            case 1:
                GridTypeRule = GridType.EightWay;
                break;
        }
    }

    public void UndoButton()
    {
        if (GameActions.Count == 0)
        {
            Debug.Log("Could not undo, there were no actions.");
            return;
        }

        if (GameActions.Peek().ActionUndoType == UndoType.CannotUndo)
        {
            Debug.Log("Cannot undo the next action");
            Debug.Log(GameActions.Peek().GetActionText());
            return;
        }

        GameAction previousAction = GameActions.Pop();
        UndoAction(previousAction);

        if (previousAction.ActionUndoType == UndoType.ContinueAfter)
        {
            UndoButton();
        }
    }

    void UndoAction(GameAction action)
    {
        if (action.CardPlayed.HasValue)
        {
            PlayingCard foundCard;

            if (!ActivePlayField.TryRemoveCardAtCoordinate(action.CoordinatePlayedOn.Value, out foundCard))
            {
                Debug.LogError("Tried to remove a card from the active field, but it doesn't exist");
            }
            else
            {
                cardsInHand.Add(foundCard);
                foundCard.Reset();
                ResetCardsInHandPosition();
                CameraManagerInstance.UpdateCamera(ActivePlayField);
            }
        }

        if (action.CardDrawn.HasValue)
        {
            PlayingCard matchingCardInHand = cardsInHand.FirstOrDefault(card => card.RepresentingCard == action.CardDrawn.Value);

            if (matchingCardInHand == null)
            {
                Debug.LogError("Tried to remove a card from hand, but it doesn't exist");
            }
            else
            {
                cardsInHand.Remove(matchingCardInHand);
                ObjectPooler.ReturnObject(matchingCardInHand);

                deck.Push(action.CardDrawn.Value);
                DeckCountLabel.text = $"x{deck.Count}";
                ResetCardsInHandPosition();
            }
        }
    }
}
