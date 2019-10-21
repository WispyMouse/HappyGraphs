using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveDataManager : MonoBehaviour
{
    static HashSet<GameRules> savedRules { get; set; } = new HashSet<GameRules>();

    public static HashSet<GameRules> GetDefaultGameRules()
    {
        HashSet<GameRules> defaultRules = new HashSet<GameRules>();

        // Basic 4s
        GameRules basicFours = new GameRules()
        {
            RuleSetName = "Basic 4s",
            HandSizeRule = 1,
            GridTypeRule = GridType.FourWay
        };
        basicFours.SetCardsPerRank(1, 4);
        basicFours.SetCardsPerRank(2, 4);
        basicFours.SetCardsPerRank(3, 4);
        basicFours.SetCardsPerRank(4, 4);
        basicFours.SetCardsPerRank(5, 0);
        basicFours.SetCardsPerRank(6, 0);
        basicFours.SetCardsPerRank(7, 0);
        basicFours.SetCardsPerRank(8, 0);
        defaultRules.Add(basicFours);

        // Basic 6s
        GameRules basicSixes = new GameRules()
        {
            RuleSetName = "Basic 6s",
            HandSizeRule = 1,
            GridTypeRule = GridType.FourWay
        };
        basicSixes.SetCardsPerRank(1, 4);
        basicSixes.SetCardsPerRank(2, 4);
        basicSixes.SetCardsPerRank(3, 4);
        basicSixes.SetCardsPerRank(4, 4);
        basicSixes.SetCardsPerRank(5, 4);
        basicSixes.SetCardsPerRank(6, 4);
        basicSixes.SetCardsPerRank(7, 0);
        basicSixes.SetCardsPerRank(8, 0);
        defaultRules.Add(basicSixes);

        // Basic 8s
        GameRules basicEights = new GameRules()
        {
            RuleSetName = "Basic 8s",
            HandSizeRule = 1,
            GridTypeRule = GridType.FourWay
        };
        basicEights.SetCardsPerRank(1, 4);
        basicEights.SetCardsPerRank(2, 4);
        basicEights.SetCardsPerRank(3, 4);
        basicEights.SetCardsPerRank(4, 4);
        basicEights.SetCardsPerRank(5, 4);
        basicEights.SetCardsPerRank(6, 4);
        basicEights.SetCardsPerRank(7, 4);
        basicEights.SetCardsPerRank(8, 4);
        defaultRules.Add(basicEights);

        return defaultRules;
    }

    public static void SaveNewRuleSet(GameRules ruleSet)
    {
        savedRules.Add(ruleSet);
    }

    public static HashSet<GameRules> GetSavedRuleSets()
    {
        return savedRules;
    }
}
