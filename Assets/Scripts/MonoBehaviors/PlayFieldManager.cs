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

    Stack<CardData> deck;
    List<PlayingCard> cardsInHand { get; set; } = new List<PlayingCard>();

    // The running total of previous play field's incomplete cards
    int previousPlayfieldIncompleteCards { get; set; } = 0;
    public PlayFieldRuntime ActivePlayField { get; set; }

    Stack<GameAction> GameActions { get; set; } = new Stack<GameAction>();

    private void Start()
    {
        deck = InstantiateDeck();

        GameRulesManager.ActiveGameRules.AdjustHandSizeRule(deck.Count);

        IncompleteCardsValue.text = "0";
        NewPlayingField(false);

        DealToPlayer(false);
    }

    Stack<CardData> InstantiateDeck()
    {
        Stack<CardData> newDeck = new Stack<CardData>();

        int maxCardValue = 4;

        switch (GameRulesManager.ActiveGameRules.GridTypeRule)
        {
            default:
            case GridType.FourWay:
                maxCardValue = 4;
                break;
            case GridType.SixWay:
                maxCardValue = 6;
                break;
            case GridType.EightWay:
                maxCardValue = 8;
                break;
        }

        for (int rank = 1; rank <= maxCardValue; rank++)
        {
            for (int cardCount = 0; cardCount < GameRulesManager.ActiveGameRules.GetCardsPerRank(rank); cardCount++)
            {
                newDeck.Push(new CardData(rank));
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

    public bool TryPlayerPlaysCard(PlayingCard playedCard, Coordinate toCoordinate)
    {
        if (!ActivePlayField.IsSpotValidForCard(playedCard, toCoordinate))
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

            if (cardsInHand.Count < GameRulesManager.ActiveGameRules.HandSizeRule)
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
            cardsInHand[cardIndex].transform.position = CameraManagerInstance.GetHandPosition(cardIndex, GameRulesManager.ActiveGameRules.HandSizeRule);
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

    void NewPlayingField(bool logAction = true)
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

        CardData seedCard = DrawCard();

        if (logAction && ActivePlayField != null)
        {
            GameActions.Push(GameAction.FromNewPlayingField(seedCard, ActivePlayField));
        }

        ActivePlayField = ObjectPooler.GetObject<PlayFieldRuntime>(PlayFieldRuntimePF, transform);
        ActivePlayField.SeedInitialCard(seedCard);
        
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

        CameraManagerInstance.UpdateCamera(ActivePlayField);
        ResetCardsInHandPosition();
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

        if (action.SeedCard.HasValue)
        {
            PlayingCard foundCard;

            if (!ActivePlayField.TryRemoveCardAtCoordinate(action.CoordinatePlayedOn.Value, out foundCard))
            {
                Debug.LogError("Tried to remove a card from the active field, but it doesn't exist");
            }
            else
            {
                ObjectPooler.ReturnObject(foundCard);

                deck.Push(action.SeedCard.Value);
                DeckCountLabel.text = $"x{deck.Count}";
            }
        }

        if (action.PreviousPlayfield != null)
        {
            ObjectPooler.ReturnObject(ActivePlayField);
            ActivePlayField = action.PreviousPlayfield;
            ActivePlayField.transform.position = Vector3.zero;
        }
    }
}
