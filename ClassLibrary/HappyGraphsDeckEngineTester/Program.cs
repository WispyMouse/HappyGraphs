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
            DeckCreationEngine.GenerateDeck(rules, new Random().Next(1000, 9999));
        }
    }
}
