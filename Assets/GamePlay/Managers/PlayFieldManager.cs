using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

enum SeedMode { Random, Set }
public class PlayFieldManager : MonoBehaviour
{
    public PlayFieldRuntime PlayFieldRuntimePF;
    public PlayingCard PlayingCardPF;

    public CameraManager CameraManagerInstance;
    public LoadingScreenDialogWindow LoadingScreenDialogWindowInstance;
    public GameRulesManager GameRulesManagerInstance;

    public Text DeckCountLabel;
    public Text TotalCardFaceValue;
    public Text IncompleteCardsValue;
    public Text SatisfiedFaceValue;
    public Text SatisfiedCountValue;

    Deck deck;
    List<PlayingCard> cardsInHand { get; set; } = new List<PlayingCard>();

    // The running total of previous play field's incomplete cards
    int previousPlayfieldIncompleteCards { get; set; } = 0;
    int previousPlayfieldTotalFaceValue { get; set; } = 0;
    int previousPlayfieldSatisfiedCount { get; set; } = 0;
    int previousPlayfieldSatisfiedFaceValue { get; set; } = 0;
    int totalDeckSize { get; set; } = 0;
    int totalDeckFaceValue { get; set; } = 0;

    public PlayFieldRuntime ActivePlayField { get; set; }

    Stack<GameAction> GameActions { get; set; } = new Stack<GameAction>();

    public InputField SeedField;
    public Toggle RandomSeedToggle;
    public Toggle SetSeedToggle;
    static SeedMode ActiveSeedMode { get; set; } = SeedMode.Random;
    static int DeckSeed { get; set; } = -1;

    private void Start()
    {
        Clear();
    }

    public void Clear()
    {
        TotalCardFaceValue.text = "0";
        IncompleteCardsValue.text = "0";
        SatisfiedCountValue.text = "0";
        SatisfiedFaceValue.text = "0";

        for (int ii = cardsInHand.Count - 1; ii >= 0; ii--)
        {
            Destroy(cardsInHand[ii].gameObject);
        }
        cardsInHand.Clear();

        deck = null;
    }

    public void InitiateStartupSequence()
    {
        StartCoroutine(StartupSequence());
    }

    IEnumerator StartupSequence()
    {
        LoadingScreenDialogWindowInstance.OpenDialog();

        if (DeckSeed == -1 || ActiveSeedMode == SeedMode.Random)
        {
            DeckSeed = Random.Range(1000, 9999);
        }

        UpdateSeedPanel();

        GameRulesManagerInstance.StartWithRules(GameRulesManager.FutureGameRules);

        yield return DeckFetchingEngine.GetDeck(GameRulesManager.ActiveGameRules, DeckSeed);
        deck = DeckFetchingEngine.LastGeneratedDeck;

        totalDeckSize = deck.DeckStack.Count;
        totalDeckFaceValue = deck.DeckStack.Sum(card => card.FaceValue);
        NewPlayingField(false);

        DealToPlayer(false);

        LoadingScreenDialogWindowInstance.CloseDialog();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.End))
        {
            Debug.Log($"Deck Seed is {DeckSeed}");
            List<GameAction> actionsToTake = SolutionEngine.FindSolution(ActivePlayField.CurrentPlayField, deck, new List<CardData>(cardsInHand.Select(card => card.RepresentingCard)));

            if (actionsToTake != null)
            {
                foreach (GameAction toTake in actionsToTake)
                {
                    PlayingCard matchingCard = cardsInHand.First(card => card.RepresentingCard == toTake.CardPlayed.Value);
                    TryPlayerPlaysCard(matchingCard, toTake.CoordinatePlayedOn.Value);
                }
            }
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

        ActivePlayField.UpdateCardVisuals();
        ActivePlayField.SetPlayableSpaces();
        UpdateScoreLabels();

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
        CardData drawnCard = deck.PopCard();
        DeckCountLabel.text = $"x{deck.DeckStack.Count}";
        
        return drawnCard;
    }

    void DealToPlayer(bool logAction = true)
    {
        if (deck.DeckStack.Count > 0)
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
        PlayingCard newCard = ObjectPooler.GetObject(PlayingCardPF, transform);
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

    void UpdateScoreLabels()
    {
        TotalCardFaceValue.text = (previousPlayfieldTotalFaceValue + ActivePlayField.GetIncompleteCards().Sum(card => card.RepresentingCard.FaceValue)).ToString();
        IncompleteCardsValue.text = (previousPlayfieldIncompleteCards + ActivePlayField.GetIncompleteCards().Count).ToString();

        SatisfiedCountValue.text = ((float)(previousPlayfieldSatisfiedCount + ActivePlayField.GetHappyCards().Count) / (float)totalDeckSize).ToString("P0");
        SatisfiedFaceValue.text = ((float)(previousPlayfieldSatisfiedFaceValue + ActivePlayField.GetHappyCards().Sum(card => card.RepresentingCard.FaceValue)) / (float)totalDeckFaceValue).ToString("P0");
    }

    void NewPlayingField(bool logAction = true)
    {
        if (deck.DeckStack.Count == 0)
        {
            Debug.Log("Trying to make a new playing field, but the deck is empty");
            return;
        }

        if (ActivePlayField != null)
        {
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
            previousPlayfieldIncompleteCards += ActivePlayField.GetIncompleteCards().Count;
            previousPlayfieldTotalFaceValue += ActivePlayField.GetIncompleteCards().Sum(card => card.RepresentingCard.FaceValue);
            previousPlayfieldSatisfiedCount += ActivePlayField.GetHappyCards().Count;
            previousPlayfieldSatisfiedFaceValue += ActivePlayField.GetHappyCards().Sum(card => card.RepresentingCard.FaceValue);
        }

        ActivePlayField = ObjectPooler.GetObject<PlayFieldRuntime>(PlayFieldRuntimePF, transform);
        ActivePlayField.SeedInitialCard(seedCard);

        ActivePlayField.UpdateCardVisuals();
        ActivePlayField.SetPlayableSpaces();
        UpdateScoreLabels();
    }

    public void UpdateValidityOfPlayableSpots(PlayingCard forCard)
    {
        ActivePlayField.UpdateValidityOfPlayableSpots(forCard);
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

        // If the previous thing on the stack is ContinueAfter, then recursively continue undoing
        if (previousAction.ActionUndoType == UndoType.ContinueAfter)
        {
            UndoButton();
            return;
        }

        CameraManagerInstance.UpdateCamera(ActivePlayField);
        ResetCardsInHandPosition();
        UpdateScoreLabels();
        ActivePlayField.UpdateCardVisuals();
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

                deck.PushCard(action.CardDrawn.Value);
                DeckCountLabel.text = $"x{deck.DeckStack.Count}";
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

                deck.PushCard(action.SeedCard.Value);
                DeckCountLabel.text = $"x{deck.DeckStack.Count}";
            }
        }

        if (action.PreviousPlayfield != null)
        {
            ObjectPooler.ReturnObject(ActivePlayField);
            ActivePlayField = action.PreviousPlayfield;
            ActivePlayField.transform.position = Vector3.zero;
            previousPlayfieldIncompleteCards -= ActivePlayField.GetIncompleteCards().Count;
            previousPlayfieldTotalFaceValue -= ActivePlayField.GetIncompleteCards().Sum(card => card.RepresentingCard.FaceValue);
            previousPlayfieldSatisfiedCount -= ActivePlayField.GetHappyCards().Count;
            previousPlayfieldSatisfiedFaceValue -= ActivePlayField.GetHappyCards().Sum(card => card.RepresentingCard.FaceValue);
        }
    }

    public void UpdateSeedPanel()
    {
        if (ActiveSeedMode == SeedMode.Random)
        {
            SeedField.text = DeckSeed.ToString();

            RandomSeedToggle.isOn = true;
            SeedField.interactable = false;
        }
        else
        {
            SetSeedToggle.isOn = true;
            SeedField.interactable = true;

            int parsedSeed;
            if (int.TryParse(SeedField.text, out parsedSeed))
            {
                DeckSeed = parsedSeed;
            }
            else
            {
                SeedField.text = DeckSeed.ToString();
            }
        }
    }

    public void SeedPanelChanged()
    {
        if (RandomSeedToggle.isOn)
        {
            ActiveSeedMode = SeedMode.Random;
        }
        else
        {
            ActiveSeedMode = SeedMode.Set;
        }

        UpdateSeedPanel();
    }
}
