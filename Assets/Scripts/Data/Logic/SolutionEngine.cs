using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static class SolutionEngine
{
    public static List<GameAction> FindSolution(PlayFieldData activePlayField, Stack<CardData> deck, List<CardData> hand)
    {
        if (activePlayField.AreAnyCoordinatesAreIncompleteable())
        {
            Debug.Log("The current playing field already has incompleteable cards.");
            return null;
        }

        StringBuilder deckString = new StringBuilder();
        Debug.Log("The deck is:");

        foreach (CardData cards in deck.ToList())
        {
            deckString.Append(cards.FaceValue);
            deckString.Append(", ");
        }

        Debug.Log(deckString.ToString().TrimEnd(' ').TrimEnd(','));

        List<GameAction> solution = SolutionIteration(activePlayField, deck, hand, new List<GameAction>());

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

        return solution;
    }

    static List<GameAction> SolutionIteration(PlayFieldData activePlayField, Stack<CardData> deck, List<CardData> hand, List<GameAction> gameActionsTaken)
    {
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
                    return resultActions;
                }
            }
            else if (CanPossiblyPerfectClear(resultedPlayField, resultedDeck, resultedHand))
            {
                List<GameAction> results = SolutionIteration(resultedPlayField, resultedDeck, resultedHand, resultActions);
                if (results != null)
                {
                    return results;
                }
            }
        }

        return null;
    }

    static List<GameAction> AllPossibleActions(PlayFieldData activePlayField, List<CardData> hand)
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

    static void TakeAction(GameAction toTake, PlayFieldData startingPlayField, Stack<CardData> startingDeck, List<CardData> startingHand,
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

    static bool CanPossiblyPerfectClear(PlayFieldData activePlayField, Stack<CardData> deck, List<CardData> hand)
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
