using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class DeckCreationEngine
{
    public static Stack<CardData> GenerateDeck(GameRules rules, int seed)
    {
        Stack<CardData> newDeck = new Stack<CardData>();

        int maxCardValue;

        switch (rules.GridTypeRule)
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
            for (int cardCount = 0; cardCount < rules.GetCardsPerRank(rank); cardCount++)
            {
                newDeck.Push(new CardData(rank));
            }
        }

        return SimpleShuffle(newDeck, rules, seed);

        Stack<CardData> deck = PerfectSolveableShuffle(newDeck, rules, seed);

        if (deck == null)
        {
            Debug.Log("Unable to create a perfect solveable deck using this rule set.");
            deck = SimpleShuffle(newDeck, rules, seed);
        }

        return deck;
    }

    public static Stack<CardData> SimpleShuffle(Stack<CardData> toShuffle, GameRules rules, int seed)
    {
        List<CardData> originalDeck = toShuffle.ToList();
        toShuffle.Clear();

        System.Random seededRandom = new System.Random(seed);

        // If we stack the deck, it means the last card in the deck should be the lowest value possible, preferably a 1
        // This is so that you don't get stuck in a situation where the last card needs more connections than you could have reasonably prepared for
        // The next card is either a 1 or a 2, the next is either a 1 or a 2 or a 3...
        if (rules.StackDeck)
        {
            int maxCardRank = originalDeck.Max(value => value.FaceValue);
            int lastCardRank = 1;

            while (lastCardRank < maxCardRank)
            {
                IEnumerable<CardData> matchingCards = originalDeck.Where(card => card.FaceValue <= lastCardRank).OrderBy(card => seededRandom.Next(0, 10000));

                if (matchingCards.Any())
                {
                    CardData usedCard = matchingCards.First();
                    originalDeck.Remove(usedCard);
                    toShuffle.Push(usedCard);
                }

                lastCardRank++;
            }
        }

        foreach (CardData card in originalDeck.OrderBy(card => seededRandom.Next(0, 10000)))
        {
            toShuffle.Push(card);
        }

        return toShuffle;
    }

    public static Stack<CardData> PerfectSolveableShuffle(Stack<CardData> toShuffle, GameRules rules, int seed)
    {
        List<CardData> originalDeck = toShuffle.ToList();
        Queue<CardData> placedCards = new Queue<CardData>();

        System.Random seededRandom = new System.Random(seed);

        PlayFieldData activePlayField = new PlayFieldData();

        Queue<CardData> cardOrder = PerfectSolveableShuffleIteration(originalDeck, placedCards, rules, seededRandom, activePlayField);

        if (cardOrder != null)
        {
            return new Stack<CardData>(new Stack<CardData>(cardOrder));
        }
        else
        {
            return null;
        }
    }

    static Queue<CardData> PerfectSolveableShuffleIteration(List<CardData> remainingCards, Queue<CardData> placedCards, GameRules rules, System.Random seededRandom, PlayFieldData activePlayField)
    {
        foreach (CardData nextCard in RandomizeOptions(remainingCards, seededRandom))
        {
            HashSet<Coordinate> validSpots = ValidCoordinatesToPlaceCard(nextCard, activePlayField);

            foreach (Coordinate spot in RandomizeOptions(validSpots, seededRandom))
            {
                PlayFieldData clonedField = activePlayField.CloneData();
                clonedField.SetCard(nextCard, spot);

                if (!clonedField.AreAnyCoordinatesAreIncompleteable())
                {
                    List<CardData> newRemaining = new List<CardData>(remainingCards);
                    newRemaining.Remove(nextCard);

                    // Frustratingly, creating a new stack creates that stack upsidedown! Reverse it, again
                    Queue<CardData> newPlacedCards = new Queue<CardData>(new Queue<CardData>(placedCards));
                    newPlacedCards.Enqueue(nextCard);

                    if (newRemaining.Count == 0)
                    {
                        PrintBoardState(clonedField);
                        return newPlacedCards;
                    }
                    else if (CanPossiblyPerfectClear(clonedField, newRemaining))
                    {
                        if (clonedField.CountOfCardsThatAreNotHappy() == 0)
                        {
                            Debug.Log("**CREATES A NEW FIELD**");
                            clonedField = new PlayFieldData();
                        }

                        Queue<CardData> stackResult = PerfectSolveableShuffleIteration(newRemaining, newPlacedCards, rules, seededRandom, clonedField);

                        if (stackResult != null)
                        {
                            return stackResult;
                        }
                    }
                }
            }
        }

        return null;
    }

    static HashSet<Coordinate> ValidCoordinatesToPlaceCard(CardData toPlace, PlayFieldData activePlayField)
    {
        HashSet<Coordinate> validCoordinates = new HashSet<Coordinate>();

        foreach (Coordinate playableSpace in activePlayField.GetValidPlayableSpaces())
        {
            if (activePlayField.IsSpotValidForCard(toPlace, playableSpace))
            {
                validCoordinates.Add(playableSpace);
            }
        }

        return validCoordinates;
    }

    static Queue<T> RandomizeOptions<T>(IEnumerable<T> options, System.Random seededRandom)
    {
        List<T> remainingOptions = new List<T>(options);
        Queue<T> orderedOptions = new Queue<T>();

        while (remainingOptions.Count > 0)
        {
            T selectedOption = remainingOptions.OrderBy(option => seededRandom.Next(0, 10000)).First();
            remainingOptions.RemoveAll(option => option.Equals(selectedOption));
            orderedOptions.Enqueue(selectedOption);
        }

        return orderedOptions;
    }

    static bool CanPossiblyPerfectClear(PlayFieldData activePlayField, IEnumerable<CardData> deck)
    {
        int totalPlaybleSum = deck.Sum(card => card.FaceValue);
        int remainingNeededNeighbors = activePlayField.NeededNeighbors();

        if (totalPlaybleSum < remainingNeededNeighbors)
        {
            return false;
        }

        return true;
    }

    static void PrintBoardState(PlayFieldData activePlayField)
    {
        foreach (Coordinate coordinate in activePlayField.PlayedCards.Keys)
        {
            Debug.Log($"{coordinate} - {activePlayField.PlayedCards[coordinate].FaceValue}");
        }
    }
}
