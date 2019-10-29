using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class SolutionEngine : MonoBehaviour
{
    static List<GameAction> solution { get; set; }
    static System.Diagnostics.Stopwatch SolutionTimeStopwatch { get; set; }
    static System.Diagnostics.Stopwatch TimeSpentFindingPossibleActions { get; set; }
    static System.Diagnostics.Stopwatch TimeSpentTakingActions { get; set; }

    public static System.Diagnostics.Stopwatch RoamingCheck { get; set; } = new System.Diagnostics.Stopwatch();
    static int DeadEndedActions { get; set; } = 0;
    static int TimesTooIncompleteCheckFires { get; set; } = 0;
    static int ImmediateImperfectCheckFires { get; set; } = 0;

    public void FindSolution(PlayFieldData activePlayField, Stack<CardData> deck, HashSet<CardData> hand)
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
        DeadEndedActions = 0;
        TimesTooIncompleteCheckFires = 0;
        ImmediateImperfectCheckFires = 0;

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
    }

    void SolutionIteration(PlayFieldData activePlayField, Stack<CardData> deck, HashSet<CardData> hand, List<GameAction> gameActionsTaken)
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
            HashSet<CardData> resultedHand;

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

    List<GameAction> AllPossibleActions(PlayFieldData activePlayField, HashSet<CardData> hand)
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

    void TakeAction(GameAction toTake, PlayFieldData startingPlayField, Stack<CardData> startingDeck, HashSet<CardData> startingHand,
        out PlayFieldData resultPlayField, out Stack<CardData> resultDeck, out HashSet<CardData> resultHand)
    {
        resultPlayField = startingPlayField.CloneData();

        // Frustratingly, creating a new stack creates that stack upsidedown! Reverse it, again
        resultDeck = new Stack<CardData>(new Stack<CardData>(startingDeck));

        resultHand = new HashSet<CardData>(startingHand);

        resultHand.Remove(toTake.CardPlayed.Value);

        if (resultDeck.Count > 0)
        {
            resultHand.Add(resultDeck.Pop());
        }

        resultPlayField.SetCard(toTake.CardPlayed.Value, toTake.CoordinatePlayedOn.Value);

        if (!resultPlayField.AreAnyMovesPossible(resultHand) && resultDeck.Count > 0)
        {
            resultPlayField = new PlayFieldData();
            resultPlayField.SetCard(resultDeck.Pop(), new Coordinate(0, 0));
        }
    }

    bool CanPossiblyPerfectClear(PlayFieldData activePlayField, Stack<CardData> deck, HashSet<CardData> hand)
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
