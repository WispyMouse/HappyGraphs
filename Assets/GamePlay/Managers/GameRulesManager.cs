using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameRulesManager : MonoBehaviour
{
    public static GameRules ActiveGameRules { get; set; }
    public static GameRules FutureGameRules { get; set; }

    public Text RuleSetTitle;
    public RulesDialogWindow RulesDialogWindow;

    public void StartWithRules(GameRules rules)
    {
        if (rules == null)
        {
            rules = SaveDataManager.GetInitialGameRule();
        }

        ActiveGameRules = rules.CloneRules();
        RuleSetTitle.text = ActiveGameRules.RuleSetName;
        CoordinateCachingManager.ClearCache();
        CoordinateCache.ClearCache();
        CoordinateCache.ActiveGridType = ActiveGameRules.GridTypeRule;

        RulesDialogWindow.CloseDialog();
    }
}
