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

        return PerfectSolveableShuffleDeck(newDeck, rules, seed);
    }

    public static Stack<CardData> PerfectSolveableShuffleDeck(Stack<CardData> toShuffle, GameRules rules, int seed)
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
}
