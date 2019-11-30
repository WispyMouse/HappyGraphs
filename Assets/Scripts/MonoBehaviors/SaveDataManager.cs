using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SaveDataManager : MonoBehaviour
{
    const string ruleSetDirectoryName = "SAVEDRULESETSDIRECTORY";
    const string lastUsedRuleSetName = "LASTUSEDRULESET";

    static List<GameRules> savedRules { get; set; } = new List<GameRules>();

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Delete))
        {
            PlayerPrefs.DeleteAll();
            savedRules.Clear();
        }
    }

    public static HashSet<GameRules> GetDefaultGameRules()
    {
        HashSet<GameRules> defaultRules = new HashSet<GameRules>();

        // Basic 4s
        GameRules basicFours = new GameRules()
        {
            RuleSetName = "Basic 4s",
            HandSizeRule = 1,
            GridTypeRule = GridTypeEnum.FourWay,
            StackDeck = true,
            // These default ruleset guids were generated once, and are effectively arbitrary
            // Having a GUID allows the reloading memory to work
            RuleSetGUID = "cc05f7cf-2b02-4117-bcae-d8a481da4070", 
            IsDefaultRule = true
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
            GridTypeRule = GridTypeEnum.SixWay,
            StackDeck = true,
            RuleSetGUID = "03d8e8b1-0a9d-4f79-a538-993ab08ef1ef",
            IsDefaultRule = true
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
            GridTypeRule = GridTypeEnum.EightWay,
            StackDeck = true,
            RuleSetGUID = "a4da6df5-5f40-4da0-ac9b-7e89fa208813",
            IsDefaultRule = true
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
        if (string.IsNullOrWhiteSpace(ruleSet.RuleSetGUID) || ruleSet.IsDefaultRule)
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

        string rulesJson = JsonConvert.SerializeObject(ruleSet);
        PlayerPrefs.SetString(ruleSet.RuleSetGUID.ToString(), rulesJson);

        SetLastUsedGameRule(ruleSet);
    }

    public static void SaveNewRuleSet(GameRules ruleSet)
    {
        ruleSet.IsDefaultRule = false;
        ruleSet.GenerateNewID();
        savedRules.Add(ruleSet);

        string rulesJson = JsonConvert.SerializeObject(ruleSet);
        PlayerPrefs.SetString(ruleSet.RuleSetGUID.ToString(), rulesJson);

        UpdateDirectory();

        SetLastUsedGameRule(ruleSet);
    }

    public static List<GameRules> GetSavedRuleSets()
    {
        if (savedRules.Count == 0)
        {
            string directoryString = PlayerPrefs.GetString(ruleSetDirectoryName);

            if (!string.IsNullOrWhiteSpace(directoryString))
            {
                List<string> directoryJson = JsonConvert.DeserializeObject<List<string>>(directoryString);

                foreach (string ruleGuid in directoryJson)
                {
                    string ruleString = PlayerPrefs.GetString(ruleGuid);

                    if (string.IsNullOrWhiteSpace(ruleString))
                    {
                        Debug.LogError($"Saved rule set at {ruleGuid} was empty, disregarding");
                    }
                    else
                    {
                        GameRules deserializedRules = JsonConvert.DeserializeObject<GameRules>(ruleString);
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

            GameRules matchingRules = GetSavedRuleSets().FirstOrDefault(rules => rules.RuleSetGUID == lastUsedRuleSetGuid);

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
        if (string.IsNullOrEmpty(toValue.RuleSetGUID))
        {
            Debug.LogError("Attempted to set a last used game rule to a set without a GUID");
            return;
            
        }

        PlayerPrefs.SetString(lastUsedRuleSetName, toValue.RuleSetGUID);
    }

    public static GameRules GetNextRuleSet(GameRules afterThis)
    {
        GameRules matchingRules = savedRules.FirstOrDefault(rule => rule.RuleSetGUID == afterThis.RuleSetGUID);

        // Somehow, this rule isn't in our list. Just hand the first default.
        if (matchingRules == null)
        {
            return GetDefaultGameRules().First();
        }

        // There aren't any other saved rules, so just hand the first default.
        if (savedRules.Count == 1)
        {
            return GetDefaultGameRules().First();
        }

        int saveIndex = savedRules.IndexOf(matchingRules);

        if (saveIndex == savedRules.Count - 1)
        {
            return savedRules[saveIndex - 1];
        }

        return savedRules[saveIndex + 1];
    }

    public static void DeleteRuleSet(GameRules toDelete)
    {
        if (string.IsNullOrEmpty(toDelete.RuleSetGUID))
        {
            Debug.LogError("Attempted to delete a game rule without a GUID");
            return;
        }

        GameRules matchingRules = savedRules.FirstOrDefault(rules => rules.RuleSetGUID == toDelete.RuleSetGUID);

        if (matchingRules != null)
        {
            savedRules.Remove(matchingRules);
        }

        PlayerPrefs.DeleteKey(toDelete.RuleSetGUID);
        UpdateDirectory();
    }

    static void UpdateDirectory()
    {
        string directoryJson = JsonConvert.SerializeObject(savedRules.Select(rule => rule.RuleSetGUID).ToList());
        PlayerPrefs.SetString(ruleSetDirectoryName, directoryJson);
    }
}
