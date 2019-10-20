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

    public CardsPerRankPanel CardsPerRankPanelPF;
    public Transform CardsPerRankHolder;

    public InputField HandSizeField;

    public Dropdown GridTypeDropdown;

    private void Awake()
    {
        ActiveGameRules = new GameRules(FutureGameRules);

        RulesDialogButtonText.text = "SHOW";
        HandSizeField.text = ActiveGameRules.HandSizeRule.ToString();

        switch (ActiveGameRules.GridTypeRule)
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

        for (int rank = 1; rank <= 8; rank++)
        {
            CardsPerRankPanel panel = ObjectPooler.GetObject<CardsPerRankPanel>(CardsPerRankPanelPF, CardsPerRankHolder);
            panel.SetRank(rank);
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
}
