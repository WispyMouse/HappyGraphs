using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RulePresetSelector : MonoBehaviour
{
    RulesDialogWindow RulesDialogWindowInstance { get; set; }
    public GameRules RepresentedRules { get; set; }
    public Button SelectionButton;
    public Text RulesNameLabel;

    public Image SelectionArrowImage;

    public void SetRepresentedRules(GameRules rulesData, RulesDialogWindow rulesDialogWindowInstance)
    {
        RulesDialogWindowInstance = rulesDialogWindowInstance;
        RepresentedRules = rulesData;
        RulesNameLabel.text = rulesData.RuleSetName;

        SelectionButton.onClick.RemoveAllListeners();
        SelectionButton.onClick.AddListener(() => { RulesDialogWindowInstance.SetRulesFromPreset(RepresentedRules); });
    }

    public void SetHighlightedState(bool toState)
    {
        SelectionArrowImage.gameObject.SetActive(toState);
    }
}
