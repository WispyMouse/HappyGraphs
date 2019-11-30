using System;
using System.Collections.Generic;

namespace HappyGraphsDeckEngineTester
{
    class Program
    {
        static void Main(string[] args)
        {
            GameRules rules = new GameRules();
            rules.GridTypeRule = GridType.FourWay;
            rules.StackDeck = true;
            rules.HandSizeRule = 1;
            Deck newDeck = DeckCreationEngine.GenerateDeck(rules, new Random().Next(1000, 9999));

            while (newDeck.DeckSize > 0)
            {
                CardData nextCard = newDeck.PopCard();
                Console.WriteLine(nextCard.FaceValue);
            }
        }
    }
}
