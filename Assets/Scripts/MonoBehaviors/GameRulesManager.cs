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

    private void Awake()
    {
        if (FutureGameRules == null)
        {
            FutureGameRules = SaveDataManager.GetInitialGameRule();
        }

        ActiveGameRules = FutureGameRules.CloneRules();
        RuleSetTitle.text = ActiveGameRules.RuleSetName;

        RulesDialogWindow.CloseDialog();
    }
}
