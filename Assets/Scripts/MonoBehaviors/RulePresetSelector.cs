using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RulePresetSelector : MonoBehaviour
{
    GameRulesManager GameRulesManagerInstance { get; set; }
    public GameRules RepresentedRules { get; set; }
    public Button SelectionButton;
    public Text RulesNameLabel;

    public void SetRepresentedRules(GameRules rulesData, GameRulesManager gameRulesManagerInstance)
    {
        GameRulesManagerInstance = gameRulesManagerInstance;
        RepresentedRules = rulesData;
        RulesNameLabel.text = rulesData.RuleSetName;

        SelectionButton.onClick.RemoveAllListeners();
        SelectionButton.onClick.AddListener(() => { GameRulesManagerInstance.SetRulesFromPreset(RepresentedRules); });
    }
}
