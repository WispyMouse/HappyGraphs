using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class SolutionEngine
{
    static List<GameAction> solution { get; set; }
    static System.Diagnostics.Stopwatch SolutionTimeStopwatch { get; set; }

    public List<GameAction> FindSolution(PlayFieldData activePlayField, Stack<CardData> deck, List<CardData> hand)
    {
        solution = null;

        if (activePlayField.AreAnyCoordinatesAreIncompleteable())
        {
            Debug.Log("The current playing field already has incompleteable cards.");
            return null;
        }

        SolutionTimeStopwatch = System.Diagnostics.Stopwatch.StartNew();

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
        return solution;
    }

    void SolutionIteration(PlayFieldData activePlayField, Stack<CardData> deck, List<CardData> hand, List<GameAction> gameActionsTaken)
    {
        if (solution != null)
        {
            return;
        }

        List<GameAction> consideredActions = AllPossibleActions(activePlayField, hand);

        foreach (GameAction validAction in consideredActions)
        {
            PlayFieldData resultedPlayField;
            Stack<CardData> resultedDeck;
            List<CardData> resultedHand;
            bool resultsInIncompleteness;

            TakeAction(validAction, activePlayField, deck, hand, out resultedPlayField, out resultedDeck, out resultedHand, out resultsInIncompleteness);

            if (resultsInIncompleteness)
            {
                continue;
            }

            List<GameAction> resultActions = new List<GameAction>();
            resultActions.AddRange(gameActionsTaken);
            resultActions.Add(validAction);

            if (resultedDeck.Count == 0 && resultedHand.Count == 0)
            {
                if (resultedPlayField.CountOfCardsThatAreNotHappy() == 0)
                {
                    solution = resultActions;
                    return;
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

    void TakeAction(GameAction toTake, PlayFieldData startingPlayField, Stack<CardData> startingDeck, List<CardData> startingHand,
        out PlayFieldData resultPlayField, out Stack<CardData> resultDeck, out List<CardData> resultHand, out bool resultsInIncompleteness)
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

        if (resultPlayField.AreAnyCoordinatesAreIncompleteable())
        {
            resultsInIncompleteness = true;
        }
        else
        {
            resultsInIncompleteness = false;
        }

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
            return false;
        }

        return true;
    }
}
