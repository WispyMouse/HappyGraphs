using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class SolutionEngine
{
    static List<GameAction> solution { get; set; }
    static System.Diagnostics.Stopwatch SolutionTimeStopwatch { get; set; }
    static System.Diagnostics.Stopwatch TimeSpentFindingPossibleActions { get; set; }
    static System.Diagnostics.Stopwatch TimeSpentTakingActions { get; set; }

    public static System.Diagnostics.Stopwatch RoamingCheck { get; set; } = new System.Diagnostics.Stopwatch();
    public static System.Diagnostics.Stopwatch TimeSpentCalculatingValidPlayableSpace { get; set; } = new System.Diagnostics.Stopwatch();
    public static System.Diagnostics.Stopwatch TimeSpentCalculatingHappyCoordinates { get; set; } = new System.Diagnostics.Stopwatch();
    static int DeadEndedActions { get; set; } = 0;
    static int TimesTooIncompleteCheckFires { get; set; } = 0;
    static int ImmediateImperfectCheckFires { get; set; } = 0;
    public static int CardWouldInvalidateSelf { get; set; } = 0;

    public static int HappyCoordinatesCacheUsed { get; set; } = 0;
    public static int ValidPlayableSpaceCacheUsed { get; set; } = 0;
    public static int TimesFieldIsCloned { get; set; } = 0;
    public static int TimesOccuppiedNeighborCountIsAskedFor { get; set; } = 0;

    public static int IsSpotValidForCardCheck { get; set; } = 0;
    public static int ShouldCoordinateBeHappyCheck { get; set; } = 0;
    public static int ShouldCoordinateBeIncompletableCheck { get; set; } = 0;
    public static int NeededNeighborsCheck { get; set; } = 0;

    public void FindSolution(PlayFieldData activePlayField, Stack<CardData> deck, List<CardData> hand)
    {
        if (activePlayField.GetIncompleteableCoordinates().Any())
        {
            Debug.Log("The current playing field already has incompleteable cards.");
            return;
        }

        solution = null;
        SolutionTimeStopwatch = System.Diagnostics.Stopwatch.StartNew();
        TimeSpentFindingPossibleActions = new System.Diagnostics.Stopwatch();
        TimeSpentTakingActions = new System.Diagnostics.Stopwatch();
        RoamingCheck = new System.Diagnostics.Stopwatch();
        TimeSpentCalculatingValidPlayableSpace = new System.Diagnostics.Stopwatch();
        TimeSpentCalculatingHappyCoordinates = new System.Diagnostics.Stopwatch();
        DeadEndedActions = 0;
        TimesTooIncompleteCheckFires = 0;
        ImmediateImperfectCheckFires = 0;
        CardWouldInvalidateSelf = 0;
        HappyCoordinatesCacheUsed = 0;
        TimesFieldIsCloned = 0;
        TimesOccuppiedNeighborCountIsAskedFor = 0;
        IsSpotValidForCardCheck = 0;
        ShouldCoordinateBeHappyCheck = 0;
        ShouldCoordinateBeIncompletableCheck = 0;
        NeededNeighborsCheck = 0;

        StringBuilder deckString = new StringBuilder();
        Debug.Log("The deck is:");

        foreach (CardData cards in deck.ToList())
        {
            deckString.Append(cards.FaceValue);
            deckString.Append(", ");
        }

        Debug.Log(deckString.ToString().TrimEnd(' ').TrimEnd(','));

        SolutionIteration(activePlayField, deck, hand, new List<GameAction>());

        if (solution == null)
        {
            Debug.Log("No solutions found.");
        }
        else
        {
            Debug.Log($"Solution found in {solution.Count} moves");

            foreach (GameAction actionsTaken in solution)
            {
                Debug.Log(actionsTaken.GetActionText());
            }
        }
        SolutionTimeStopwatch.Stop();
        Debug.Log($"Solution took {SolutionTimeStopwatch.ElapsedMilliseconds} milliseconds to find.");
        Debug.Log($"Spent {TimeSpentFindingPossibleActions.ElapsedMilliseconds} milliseconds finding possible actions.");
        Debug.Log($"Spent {TimeSpentTakingActions.ElapsedMilliseconds} milliseconds taking actions.");
        Debug.Log($"Spent {RoamingCheck.ElapsedMilliseconds} milliseconds on the roaming check.");
        Debug.Log($"There were {DeadEndedActions} dead ended actions.");
        Debug.Log($"The shortcircuit fired {TimesTooIncompleteCheckFires} times.");
        Debug.Log($"There have been {ImmediateImperfectCheckFires} removed actions for being immediately imperfect.");
        Debug.Log($"There have been {CardWouldInvalidateSelf} actions that would invalidate their own card.");
        Debug.Log($"The valid playable coordinates cache was used {ValidPlayableSpaceCacheUsed} times.");
        Debug.Log($"The happy coordinates cache was used {HappyCoordinatesCacheUsed} times.");
        Debug.Log($"Spent {TimeSpentCalculatingValidPlayableSpace.ElapsedMilliseconds} miliseconds searching for valid playable spaces.");
        Debug.Log($"Spent {TimeSpentCalculatingHappyCoordinates.ElapsedMilliseconds} miliseconds calculating happy coordinates.");
        Debug.Log($"There were {TimesFieldIsCloned} PlayFieldData clones made.");
        Debug.Log($"Asked for OccuppiedNeighborsAtCoordinate {TimesOccuppiedNeighborCountIsAskedFor} times.");
        Debug.Log($"Asked for IsSpotValidForCoordinate {IsSpotValidForCardCheck} times.");
        Debug.Log($"Asked for ShouldCoordinateBeHappy {ShouldCoordinateBeHappyCheck} times.");
        Debug.Log($"Asked for ShouldCoordinateBeIncompletable {ShouldCoordinateBeIncompletableCheck} times.");
        Debug.Log($"Asked for NeededNeighbors {NeededNeighborsCheck} times.");
    }

    void SolutionIteration(PlayFieldData activePlayField, Stack<CardData> deck, List<CardData> hand, List<GameAction> gameActionsTaken)
    {
        if (solution != null)
        {
            return;
        }

        TimeSpentFindingPossibleActions.Start();
        List<GameAction> consideredActions = AllPossibleActions(activePlayField, hand);
        List<GameAction> validActions = ReduceToNotImmediatelyImperfectActions(activePlayField, consideredActions);
        TimeSpentFindingPossibleActions.Stop();

        foreach (GameAction validAction in validActions)
        {
            TimeSpentTakingActions.Start();

            PlayFieldData resultedPlayField;
            Stack<CardData> resultedDeck;
            List<CardData> resultedHand;

            TakeAction(validAction, activePlayField, deck, hand, out resultedPlayField, out resultedDeck, out resultedHand);

            List<GameAction> resultActions = new List<GameAction>();
            resultActions.AddRange(gameActionsTaken);
            resultActions.Add(validAction);

            TimeSpentTakingActions.Stop();

            if (resultedDeck.Count == 0 && resultedHand.Count == 0)
            {
                if (resultedPlayField.CountOfCardsThatAreNotHappy() == 0)
                {
                    solution = resultActions;
                    return;
                }
                else
                {
                    DeadEndedActions++;
                }
            }
            else if (CanPossiblyPerfectClear(resultedPlayField, resultedDeck, resultedHand))
            {
                SolutionIteration(resultedPlayField, resultedDeck, resultedHand, resultActions);
            }
        }
    }

    List<GameAction> AllPossibleActions(PlayFieldData activePlayField, List<CardData> hand)
    {
        List<GameAction> possibleActions = new List<GameAction>();

        HashSet<Coordinate> possibleCoordinates = activePlayField.GetValidPlayableSpaces();

        foreach (CardData consideredCard in hand)
        {
            foreach (Coordinate consideredCoordinate in possibleCoordinates)
            {
                if (activePlayField.IsSpotValidForCard(consideredCard, consideredCoordinate))
                {
                    possibleActions.Add(GameAction.FromCardPlayed(consideredCard, consideredCoordinate));
                }
            }
        }

        return possibleActions;
    }

    List<GameAction> ReduceToNotImmediatelyImperfectActions(PlayFieldData activePlayField, List<GameAction> potentialActions)
    {
        List<GameAction> resultedActions = new List<GameAction>();

        foreach (GameAction consideredAction in potentialActions)
        {
            PlayFieldData clonedPlayField = activePlayField.CloneData();
            clonedPlayField.SetCard(consideredAction.CardPlayed.Value, consideredAction.CoordinatePlayedOn.Value);

            if (clonedPlayField.ShouldCoordinateBeIncompletable(consideredAction.CoordinatePlayedOn.Value))
            {
                CardWouldInvalidateSelf++;
                continue;
            }

            if (clonedPlayField.GetIncompleteableCoordinates().Count == 0)
            {
                resultedActions.Add(consideredAction);
            }
            else
            {
                ImmediateImperfectCheckFires++;
            }
        }

        return resultedActions;
    }

    void TakeAction(GameAction toTake, PlayFieldData startingPlayField, Stack<CardData> startingDeck, List<CardData> startingHand,
        out PlayFieldData resultPlayField, out Stack<CardData> resultDeck, out List<CardData> resultHand)
    {
        resultPlayField = startingPlayField.CloneData();

        // Frustratingly, creating a new stack creates that stack upsidedown! Reverse it, again
        resultDeck = new Stack<CardData>(new Stack<CardData>(startingDeck));

        resultHand = new List<CardData>(startingHand);

        resultHand.Remove(toTake.CardPlayed.Value);

        if (resultDeck.Count > 0)
        {
            resultHand.Add(resultDeck.Pop());
        }

        resultPlayField.SetCard(toTake.CardPlayed.Value, toTake.CoordinatePlayedOn.Value);

        if (resultDeck.Count > 0 && resultHand.Count > 0 && !resultPlayField.AreAnyMovesPossible(resultHand))
        {
            resultPlayField = new PlayFieldData();
            resultPlayField.SetCard(resultDeck.Pop(), new Coordinate(0, 0));
        }
    }

    bool CanPossiblyPerfectClear(PlayFieldData activePlayField, Stack<CardData> deck, List<CardData> hand)
    {
        int totalPlaybleSum = deck.Sum(card => card.FaceValue) + hand.Sum(card => card.FaceValue);
        int remainingNeededNeighbors = activePlayField.NeededNeighbors();

        if (totalPlaybleSum < remainingNeededNeighbors)
        {
            TimesTooIncompleteCheckFires++;
            return false;
        }

        return true;
    }
}
