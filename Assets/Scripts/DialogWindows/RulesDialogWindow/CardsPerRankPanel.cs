using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardsPerRankPanel : MonoBehaviour
{
    public RulesDialogWindow RulesDialogWindowInstance { get; set; }

    public int RepresentedNumber { get; set; }

    public Text NumberLabel;
    public InputField NumberInput;

    public void SetRank(int rank)
    {
        RepresentedNumber = rank;
        NumberLabel.text = rank.ToString();
        NumberInput.text = RulesDialogWindowInstance.CurrentlyWorkshoppedGameRules.GetCardsPerRank(rank).ToString();
    }

    public void TickUp()
    {
        RulesDialogWindowInstance.CurrentlyWorkshoppedGameRules.SetCardsPerRank(RepresentedNumber, RulesDialogWindowInstance.CurrentlyWorkshoppedGameRules.GetCardsPerRank(RepresentedNumber) + 1);
        NumberInput.text = RulesDialogWindowInstance.CurrentlyWorkshoppedGameRules.GetCardsPerRank(RepresentedNumber).ToString();
        RulesDialogWindowInstance.MarkRuleAsDirty();
    }

    public void TickDown()
    {
        RulesDialogWindowInstance.CurrentlyWorkshoppedGameRules.SetCardsPerRank(RepresentedNumber, Mathf.Max(0, RulesDialogWindowInstance.CurrentlyWorkshoppedGameRules.GetCardsPerRank(RepresentedNumber) - 1));
        NumberInput.text = RulesDialogWindowInstance.CurrentlyWorkshoppedGameRules.GetCardsPerRank(RepresentedNumber).ToString();
        RulesDialogWindowInstance.MarkRuleAsDirty();
    }

    public void NumberInputValueChanged()
    {
        int parsedValue;

        if (int.TryParse(NumberInput.text, out parsedValue))
        {
            RulesDialogWindowInstance.CurrentlyWorkshoppedGameRules.SetCardsPerRank(RepresentedNumber, Mathf.Max(0, parsedValue));
            NumberInput.text = RulesDialogWindowInstance.CurrentlyWorkshoppedGameRules.GetCardsPerRank(RepresentedNumber).ToString();
            RulesDialogWindowInstance.MarkRuleAsDirty();
        }
        else
        {
            NumberInput.text = RulesDialogWindowInstance.CurrentlyWorkshoppedGameRules.GetCardsPerRank(RepresentedNumber).ToString();
        }
    }
}
