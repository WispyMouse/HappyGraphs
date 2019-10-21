using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GridType { FourWay, SixWay, EightWay }
public class GameRules
{
    public string RuleSetName { get; set; } = "New Rule Set";
    public Dictionary<int, int> CardsPerRankRules { get; private set; } = new Dictionary<int, int>();
    public int HandSizeRule { get; set; } = 1;
    public GridType GridTypeRule { get; set; } = GridType.FourWay;

    public GameRules()
    {

    }

    public GameRules CloneRules()
    {
        return (GameRules)this.MemberwiseClone();
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
