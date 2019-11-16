using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class RulesDialogWindow : DialogWindow
{
    public GameRules CurrentlyWorkshoppedGameRules { get; set; }

    public Text RulesDialogButtonText;

    public InputField RuleSetName;
    public InputField HandSizeField;
    public Dropdown GridTypeDropdown;
    public Toggle StackDeckToggle;

    public CardsPerRankPanel CardsPerRankPanelPF;
    public Transform CardsPerRankHolder;
    HashSet<CardsPerRankPanel> CardsPerRankPanels { get; set; }

    public RulePresetSelector RulesPresetSelectorPF;
    public Transform RulesPresetHolder;
    HashSet<RulePresetSelector> RulePresetSelectors { get; set; } = new HashSet<RulePresetSelector>();

    public Button UpdateRuleSetButton;
    public Button DeleteRuleSetButton;

    protected override void DialogOpened()
    {
        RulesDialogButtonText.text = "CLOSE";
    }

    protected override void DialogClosed()
    {
        RulesDialogButtonText.text = "SHOW";
    }

    private void Start()
    {
        HydrateRulePresetPanel();

        CurrentlyWorkshoppedGameRules = GameRulesManager.ActiveGameRules.CloneRules();

        CardsPerRankPanels = new HashSet<CardsPerRankPanel>();
        for (int rank = 1; rank <= 8; rank++)
        {
            CardsPerRankPanel panel = ObjectPooler.GetObject(CardsPerRankPanelPF, CardsPerRankHolder);
            panel.RulesDialogWindowInstance = this;
            panel.SetRank(rank);
            CardsPerRankPanels.Add(panel);
        }

        SetRulesFromPreset(CurrentlyWorkshoppedGameRules);

        HydrateRulePanel();
    }

    void HydrateRulePanel()
    {
        RuleSetName.text = CurrentlyWorkshoppedGameRules.RuleSetName;
        HandSizeField.text = CurrentlyWorkshoppedGameRules.HandSizeRule.ToString();
        StackDeckToggle.isOn = CurrentlyWorkshoppedGameRules.StackDeck;

        switch (CurrentlyWorkshoppedGameRules.GridTypeRule)
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

        if (CurrentlyWorkshoppedGameRules.IsDefaultRule)
        {
            UpdateRuleSetButton.gameObject.SetActive(false);
            DeleteRuleSetButton.gameObject.SetActive(false);
        }
        else
        {
            UpdateRuleSetButton.gameObject.SetActive(true);
            DeleteRuleSetButton.gameObject.SetActive(true);
            UpdateRuleSetButton.interactable = false;
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
        // Deselect the old rules
        if (CurrentlyWorkshoppedGameRules != null)
        {
            RulePresetSelector matchingOldSelector = MatchingSelector(CurrentlyWorkshoppedGameRules);

            if (matchingOldSelector != null)
            {
                matchingOldSelector.SetHighlightedState(false);
            }
        }

        CurrentlyWorkshoppedGameRules = rules.CloneRules();
        GameRulesManager.FutureGameRules = rules.CloneRules();
        HydrateRulePanel();
        UpdateRuleSetButton.interactable = false;

        SaveDataManager.SetLastUsedGameRule(rules);

        RulePresetSelector matchingNewSelector = MatchingSelector(CurrentlyWorkshoppedGameRules);

        if (matchingNewSelector != null)
        {
            matchingNewSelector.SetHighlightedState(true);
        }
    }

    public void RuleSetNameChanged()
    {
        if (!string.IsNullOrEmpty(RuleSetName.text) && RuleSetName.text != CurrentlyWorkshoppedGameRules.RuleSetName)
        {
            CurrentlyWorkshoppedGameRules.RuleSetName = RuleSetName.text;
            MarkRuleAsDirty();
        }
        else
        {
            RuleSetName.text = CurrentlyWorkshoppedGameRules.RuleSetName;
        }
    }

    public void HandSizeRuleChanged()
    {
        int parsedValue;

        if (int.TryParse(HandSizeField.text, out parsedValue) && parsedValue != CurrentlyWorkshoppedGameRules.HandSizeRule)
        {
            CurrentlyWorkshoppedGameRules.HandSizeRule = Mathf.Max(1, parsedValue);
            HandSizeField.text = CurrentlyWorkshoppedGameRules.HandSizeRule.ToString();
            MarkRuleAsDirty();
        }
        else
        {
            HandSizeField.text = CurrentlyWorkshoppedGameRules.HandSizeRule.ToString();
        }
    }

    public void GridTypeRuleChanged()
    {
        GridType newGridType;

        switch (GridTypeDropdown.value)
        {
            default:
            case 0:
                newGridType = GridType.FourWay;
                break;
            case 1:
                newGridType = GridType.SixWay;
                break;
            case 2:
                newGridType = GridType.EightWay;
                break;
        }

        if (CurrentlyWorkshoppedGameRules.GridTypeRule != newGridType)
        {
            CurrentlyWorkshoppedGameRules.GridTypeRule = newGridType;
            MarkRuleAsDirty();
        }
    }

    public void HandSizeTick(int direction)
    {
        CurrentlyWorkshoppedGameRules.HandSizeRule = Mathf.Max(1, CurrentlyWorkshoppedGameRules.HandSizeRule + direction);
        HandSizeField.text = CurrentlyWorkshoppedGameRules.HandSizeRule.ToString();
        MarkRuleAsDirty();
    }

    public void StackDeckToggleChanged()
    {
        CurrentlyWorkshoppedGameRules.StackDeck = StackDeckToggle.isOn;
        MarkRuleAsDirty();
    }

    public void SaveAsNewButton()
    {
        GameRules newRules = CurrentlyWorkshoppedGameRules.CloneRules();

        SaveDataManager.SaveNewRuleSet(newRules);

        RulePresetSelector newRulesPreset = ObjectPooler.GetObject(RulesPresetSelectorPF, RulesPresetHolder);
        RulePresetSelectors.Add(newRulesPreset);
        newRulesPreset.SetRepresentedRules(newRules, this);

        SetRulesFromPreset(newRules);
    }

    public void UpdateButton()
    {
        SaveDataManager.UpdateRuleSet(CurrentlyWorkshoppedGameRules);
        CurrentlyWorkshoppedGameRules = CurrentlyWorkshoppedGameRules.CloneRules();
        GameRulesManager.FutureGameRules = CurrentlyWorkshoppedGameRules.CloneRules();
        UpdateRuleSetButton.interactable = false;

        RulePresetSelector matchingSelector = MatchingSelector(CurrentlyWorkshoppedGameRules);

        if (matchingSelector != null)
        {
            matchingSelector.SetRepresentedRules(CurrentlyWorkshoppedGameRules, this);
        }
    }

    public void DeleteButton()
    {
        RulePresetSelector matchingSelector = MatchingSelector(CurrentlyWorkshoppedGameRules);
        RulePresetSelectors.Remove(matchingSelector);
        ObjectPooler.ReturnObject(matchingSelector);

        GameRules nextRuleSet = SaveDataManager.GetNextRuleSet(CurrentlyWorkshoppedGameRules);

        SaveDataManager.DeleteRuleSet(CurrentlyWorkshoppedGameRules);

        SetRulesFromPreset(nextRuleSet);
    }

    public void MarkRuleAsDirty()
    {
        UpdateRuleSetButton.interactable = true;
    }

    RulePresetSelector MatchingSelector(GameRules forRules)
    {
        return RulePresetSelectors.FirstOrDefault(preset => preset.RepresentedRules.RuleSetGUID == forRules.RuleSetGUID);
    }
}
