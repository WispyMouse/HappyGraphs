using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public enum GridType { FourWay, SixWay, EightWay }
public class GameRules
{
    // This is a string rather than a System.Guid, because FastJSON (the library used for serialization) seems to serialize Guids oddly
    public string RuleSetGUID;

    public string RuleSetName = "New Rule Set";
    public Dictionary<int, int> CardsPerRankRules = new Dictionary<int, int>();
    public int HandSizeRule = 1;
    public GridType GridTypeRule  = GridType.FourWay;
    public bool StackDeck = true;

    public bool IsDefaultRule { get; set; }

    public GameRules()
    {
    }

    public void GenerateNewID()
    {
        RuleSetGUID = System.Guid.NewGuid().ToString();
    }

    public GameRules CloneRules()
    {
        GameRules newRules = new GameRules();

        newRules.RuleSetGUID = RuleSetGUID;
        newRules.RuleSetName = RuleSetName;
        newRules.CardsPerRankRules = new Dictionary<int, int>(CardsPerRankRules);
        newRules.HandSizeRule = HandSizeRule;
        newRules.GridTypeRule = GridTypeRule;
        newRules.StackDeck = StackDeck;
        newRules.IsDefaultRule = IsDefaultRule;

        return newRules;
    }

    public void AdjustHandSizeRule(int deckSize)
    {
        HandSizeRule = Mathf.Min(HandSizeRule, deckSize - 1);
    }

    public int GetCardsPerRank(int rank)
    {
        int value;

        if (CardsPerRankRules.TryGetValue(rank, out value))
        {
            return value;
        }

        CardsPerRankRules.Add(rank, 4);
        return 4;
    }

    public void SetCardsPerRank(int rank, int toValue)
    {
        toValue = Mathf.Max(0, toValue);

        if (CardsPerRankRules.ContainsKey(rank))
        {
            CardsPerRankRules[rank] = toValue;
        }
        else
        {
            CardsPerRankRules.Add(rank, toValue);
        }
    }
}
