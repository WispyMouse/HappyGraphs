using fastJSON;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SaveDataManager : MonoBehaviour
{
    const string ruleSetDirectoryName = "SAVEDRULESETSDIRECTORY";

    static HashSet<GameRules> savedRules { get; set; } = new HashSet<GameRules>();

    public static HashSet<GameRules> GetDefaultGameRules()
    {
        HashSet<GameRules> defaultRules = new HashSet<GameRules>();

        // Basic 4s
        GameRules basicFours = new GameRules()
        {
            RuleSetName = "Basic 4s",
            HandSizeRule = 1,
            GridTypeRule = GridType.FourWay,
            StackDeck = true
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
            GridTypeRule = GridType.SixWay,
            StackDeck = true
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
            GridTypeRule = GridType.EightWay,
            StackDeck = true
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
        ruleSet.GenerateNewID();
        savedRules.Add(ruleSet);

        string rulesJson = JSON.ToJSON(ruleSet);
        PlayerPrefs.SetString(ruleSet.RuleSetGUID.ToString(), rulesJson);

        string directoryJson = JSON.ToJSON(savedRules.Select(rule => rule.RuleSetGUID).ToList());
        PlayerPrefs.SetString(ruleSetDirectoryName, directoryJson);
    }

    public static HashSet<GameRules> GetSavedRuleSets()
    {
        if (savedRules.Count == 0)
        {
            string directoryString = PlayerPrefs.GetString(ruleSetDirectoryName);

            if (!string.IsNullOrWhiteSpace(directoryString))
            {
                List<string> directoryJson = JSON.ToObject<List<string>>(directoryString);

                foreach (string ruleGuid in directoryJson)
                {
                    string ruleString = PlayerPrefs.GetString(ruleGuid);

                    if (string.IsNullOrWhiteSpace(ruleString))
                    {
                        Debug.LogError($"Saved rule set at {ruleGuid} was empty, disregarding");
                    }
                    else
                    {
                        GameRules deserializedRules = JSON.ToObject<GameRules>(ruleString);
                        savedRules.Add(deserializedRules);
                    }
                }
            }
        }

        return savedRules;
    }
}
