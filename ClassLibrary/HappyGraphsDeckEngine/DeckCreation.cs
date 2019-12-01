using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class DeckCreationEngine
{
    public static Deck LastGeneratedDeck { get; set; }

    public static Deck GenerateDeck(GameRules rules, int seed)
    {
        Deck newDeck = new Deck();

        int maxCardValue;

        switch (rules.GridTypeRule)
        {
            default:
            case GridTypeEnum.FourWay:
                maxCardValue = 4;
                break;
            case GridTypeEnum.SixWay:
                maxCardValue = 6;
                break;
            case GridTypeEnum.EightWay:
                maxCardValue = 8;
                break;
        }

        Random seededRandom = new Random(seed);

        for (int rank = 1; rank <= maxCardValue; rank++)
        {
            for (int cardCount = 0; cardCount < rules.GetCardsPerRank(rank); cardCount++)
            {
                newDeck.PushCard(new CardData(rank, FlavorCodeTranslations.GetRandomColorHexCodeIndex(seededRandom)));
            }
        }

        return SimpleShuffle(newDeck, rules, seededRandom);

        /*
        Stack<CardData> deck = PerfectSolveableShuffle(newDeck, rules, seed);

        if (deck == null)
        {
            deck = SimpleShuffle(newDeck, rules, seed);
        }

        return deck;
        */
    }

    public static Deck SimpleShuffle(Deck toShuffle, GameRules rules, Random randomEngine)
    {
        List<CardData> originalDeckCards = toShuffle.GetAllCards();
        Deck shuffledDeck = new Deck();

        // If we stack the deck, it means the last card in the deck should be the lowest value possible, preferably a 1
        // This is so that you don't get stuck in a situation where the last card needs more connections than you could have reasonably prepared for
        // The next card is either a 1 or a 2, the next is either a 1 or a 2 or a 3...
        if (rules.StackDeck)
        {
            int maxCardRank = originalDeckCards.Max(value => value.FaceValue);
            int lastCardRank = 1;

            while (lastCardRank < maxCardRank)
            {
                IEnumerable<CardData> matchingCards = originalDeckCards.Where(card => card.FaceValue <= lastCardRank).OrderBy(card => randomEngine.Next(0, 10000));

                if (matchingCards.Any())
                {
                    CardData usedCard = matchingCards.First();
                    originalDeckCards.Remove(usedCard);
                    shuffledDeck.PushCard(usedCard);
                }

                lastCardRank++;
            }
        }

        foreach (CardData card in originalDeckCards.OrderBy(card => randomEngine.Next(0, 10000)))
        {
            shuffledDeck.PushCard(card);
        }

        return shuffledDeck;
    }

    public static Deck PerfectSolveableShuffle(Deck toShuffle, GameRules rules, Random randomEngine)
    {
        List<CardData> originalDeckCards = toShuffle.GetAllCards();
        Queue<CardData> placedCards = new Queue<CardData>();

        PlayFieldData activePlayField = new PlayFieldData();

        Queue<CardData> cardOrder = PerfectSolveableShuffleIteration(originalDeckCards, placedCards, rules, randomEngine, activePlayField);

        if (cardOrder != null)
        {
            return new Deck(cardOrder);
        }
        else
        {
            return null;
        }
    }

    static Queue<CardData> PerfectSolveableShuffleIteration(List<CardData> remainingCards, Queue<CardData> placedCards, GameRules rules, Random seededRandom, PlayFieldData activePlayField)
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
                            // Debug.Log("**CREATES A NEW FIELD**");
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

    static Queue<T> RandomizeOptions<T>(IEnumerable<T> options, Random seededRandom)
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
            System.Console.WriteLine($"{coordinate} - {activePlayField.PlayedCards[coordinate].FaceValue}");
        }
    }
}
