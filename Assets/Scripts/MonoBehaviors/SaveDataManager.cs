using fastJSON;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SaveDataManager : MonoBehaviour
{
    const string ruleSetDirectoryName = "SAVEDRULESETSDIRECTORY";
    const string lastUsedRuleSetName = "LASTUSEDRULESET";

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
            StackDeck = true,
            RuleSetGUID = "cc05f7cf-2b02-4117-bcae-d8a481da4070"
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
            StackDeck = true,
            RuleSetGUID = "03d8e8b1-0a9d-4f79-a538-993ab08ef1ef"
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
            StackDeck = true,
            RuleSetGUID = "a4da6df5-5f40-4da0-ac9b-7e89fa208813"
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

    public static void UpdateRuleSet(GameRules ruleSet)
    {
        if (string.IsNullOrWhiteSpace(ruleSet.RuleSetGUID))
        {
            SaveNewRuleSet(ruleSet);
            return;
        }
        GameRules matchingRules = savedRules.FirstOrDefault(rules => rules.RuleSetGUID == ruleSet.RuleSetGUID);

        if (matchingRules == null)
        {
            SaveNewRuleSet(ruleSet);
            return;
        }

        savedRules.Remove(matchingRules);
        savedRules.Add(ruleSet);

        string rulesJson = JSON.ToJSON(ruleSet);
        PlayerPrefs.SetString(ruleSet.RuleSetGUID.ToString(), rulesJson);
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

    public static GameRules GetInitialGameRule()
    {
        string lastUsedRuleSetGuid = PlayerPrefs.GetString(lastUsedRuleSetName, "");

        if (!string.IsNullOrWhiteSpace(lastUsedRuleSetGuid))
        {
            GameRules defaultMatchingRule = GetDefaultGameRules().FirstOrDefault(rules => rules.RuleSetGUID == lastUsedRuleSetGuid);

            if (defaultMatchingRule != null)
            {
                return defaultMatchingRule;
            }

            GameRules matchingRules = savedRules.FirstOrDefault(rules => rules.RuleSetGUID == lastUsedRuleSetGuid);

            if (matchingRules != null)
            {
                return matchingRules;
            }
            else
            {
                Debug.LogError($"Saved rule set at {lastUsedRuleSetGuid} was empty, disregarding");
            }
        }

        return GetDefaultGameRules().First();
    }

    public static void SetLastUsedGameRule(GameRules toValue)
    {
        if (!string.IsNullOrEmpty(toValue.RuleSetGUID))
        {
            PlayerPrefs.SetString(lastUsedRuleSetName, toValue.RuleSetGUID);
        }
        else
        {
            Debug.LogError("Attempted to set a last used game rule to a set without a GUID");
        }
    }
}
