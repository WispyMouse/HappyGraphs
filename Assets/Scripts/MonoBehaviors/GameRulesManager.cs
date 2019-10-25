using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameRulesManager : MonoBehaviour
{
    public static GameRules ActiveGameRules { get; set; } = new GameRules();
    public static GameRules FutureGameRules { get; set; } = new GameRules();

    public Text RulesDialogButtonText;
    public GameObject RulesDialog;

    public InputField RuleSetName;
    public CardsPerRankPanel CardsPerRankPanelPF;
    public Transform CardsPerRankHolder;
    HashSet<CardsPerRankPanel> CardsPerRankPanels { get; set; }
    public InputField HandSizeField;
    public Dropdown GridTypeDropdown;
    public Toggle StackDeckToggle;

    public RulePresetSelector RulesPresetSelectorPF;
    public Transform RulesPresetHolder;
    HashSet<RulePresetSelector> RulePresetSelectors { get; set; } = new HashSet<RulePresetSelector>();

    private void Awake()
    {
        ActiveGameRules = FutureGameRules.CloneRules();

        RulesDialogButtonText.text = "SHOW";

        CardsPerRankPanels = new HashSet<CardsPerRankPanel>();
        for (int rank = 1; rank <= 8; rank++)
        {
            CardsPerRankPanel panel = ObjectPooler.GetObject<CardsPerRankPanel>(CardsPerRankPanelPF, CardsPerRankHolder);
            panel.SetRank(rank);
            CardsPerRankPanels.Add(panel);
        }

        HydrateRulePanel();
        HydrateRulePresetPanel();
    }

    void HydrateRulePanel()
    {
        RuleSetName.text = FutureGameRules.RuleSetName;
        HandSizeField.text = FutureGameRules.HandSizeRule.ToString();
        StackDeckToggle.isOn = FutureGameRules.StackDeck;

        switch (FutureGameRules.GridTypeRule)
        {
            default:
            case GridType.FourWay:
                GridTypeDropdown.value = 0;
                break;
            case GridType.SixWay:
                GridTypeDropdown.value = 1;
                break;
            case GridType.EightWay:
                GridTypeDropdown.value = 2;
                break;
        }

        foreach (CardsPerRankPanel panel in CardsPerRankPanels)
        {
            panel.SetRank(panel.RepresentedNumber);
        }
    }

    void HydrateRulePresetPanel()
    {
        HashSet<GameRules> rulesToSet = new HashSet<GameRules>();

        rulesToSet.UnionWith(SaveDataManager.GetDefaultGameRules());
        rulesToSet.UnionWith(SaveDataManager.GetSavedRuleSets());

        foreach (GameRules rules in rulesToSet)
        {
            RulePresetSelector newRulePresetSelector = ObjectPooler.GetObject(RulesPresetSelectorPF, RulesPresetHolder);
            newRulePresetSelector.SetRepresentedRules(rules, this);
            RulePresetSelectors.Add(newRulePresetSelector);
        }
    }

    public void SetRulesFromPreset(GameRules rules)
    {
        FutureGameRules = rules.CloneRules();
        HydrateRulePanel();
    }

    public void RuleSetNameChanged()
    {
        if (!string.IsNullOrEmpty(RuleSetName.text))
        {
            FutureGameRules.RuleSetName = RuleSetName.text;
        }
        else
        {
            RuleSetName.text = FutureGameRules.RuleSetName;
        }
    }

    public void HandSizeRuleChanged()
    {
        int parsedValue;

        if (int.TryParse(HandSizeField.text, out parsedValue))
        {
            FutureGameRules.HandSizeRule = Mathf.Max(1, parsedValue);
            HandSizeField.text = FutureGameRules.HandSizeRule.ToString();
        }
        else
        {
            HandSizeField.text = FutureGameRules.HandSizeRule.ToString();
        }
    }

    public void GridTypeRuleChanged()
    {
        switch (GridTypeDropdown.value)
        {
            default:
            case 0:
                FutureGameRules.GridTypeRule = GridType.FourWay;
                break;
            case 1:
                FutureGameRules.GridTypeRule = GridType.SixWay;
                break;
            case 2:
                FutureGameRules.GridTypeRule = GridType.EightWay;
                break;
        }
    }

    public void ToggleRulesDialog()
    {
        if (!RulesDialog.activeSelf)
        {
            RulesDialog.SetActive(true);
            RulesDialogButtonText.text = "HIDE";
        }
        else
        {
            RulesDialog.SetActive(false);
            RulesDialogButtonText.text = "SHOW";
        }
    }

    public void HandSizeTick(int direction)
    {
        FutureGameRules.HandSizeRule = Mathf.Max(1, FutureGameRules.HandSizeRule + direction);
        HandSizeField.text = FutureGameRules.HandSizeRule.ToString();
    }

    public void StackDeckToggleChanged()
    {
        FutureGameRules.StackDeck = StackDeckToggle.isOn;
    }

    public void SaveAsNewButton()
    {
        RulePresetSelector newRulesPreset = ObjectPooler.GetObject(RulesPresetSelectorPF, RulesPresetHolder);
        newRulesPreset.SetRepresentedRules(FutureGameRules, this);
        SaveDataManager.SaveNewRuleSet(FutureGameRules);
        FutureGameRules = FutureGameRules.CloneRules();
    }
}
