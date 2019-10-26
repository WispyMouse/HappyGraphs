using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardsPerRankPanel : MonoBehaviour
{
    public GameRulesManager GameRulesManagerInstance { get; set; }

    public int RepresentedNumber { get; set; }

    public Text NumberLabel;
    public InputField NumberInput;

    public void SetRank(int rank)
    {
        RepresentedNumber = rank;
        NumberLabel.text = rank.ToString();
        NumberInput.text = GameRulesManager.CurrentlyWorkshoppedGameRules.GetCardsPerRank(rank).ToString();
    }

    public void TickUp()
    {
        GameRulesManager.CurrentlyWorkshoppedGameRules.SetCardsPerRank(RepresentedNumber, GameRulesManager.CurrentlyWorkshoppedGameRules.GetCardsPerRank(RepresentedNumber) + 1);
        NumberInput.text = GameRulesManager.CurrentlyWorkshoppedGameRules.GetCardsPerRank(RepresentedNumber).ToString();
        GameRulesManagerInstance.MarkRuleAsDirty();
    }

    public void TickDown()
    {
        GameRulesManager.CurrentlyWorkshoppedGameRules.SetCardsPerRank(RepresentedNumber, Mathf.Max(0, GameRulesManager.CurrentlyWorkshoppedGameRules.GetCardsPerRank(RepresentedNumber) - 1));
        NumberInput.text = GameRulesManager.CurrentlyWorkshoppedGameRules.GetCardsPerRank(RepresentedNumber).ToString();
        GameRulesManagerInstance.MarkRuleAsDirty();
    }

    public void NumberInputValueChanged()
    {
        int parsedValue;

        if (int.TryParse(NumberInput.text, out parsedValue))
        {
            GameRulesManager.CurrentlyWorkshoppedGameRules.SetCardsPerRank(RepresentedNumber, Mathf.Max(0, parsedValue));
            NumberInput.text = GameRulesManager.CurrentlyWorkshoppedGameRules.GetCardsPerRank(RepresentedNumber).ToString();
            GameRulesManagerInstance.MarkRuleAsDirty();
        }
        else
        {
            NumberInput.text = GameRulesManager.CurrentlyWorkshoppedGameRules.GetCardsPerRank(RepresentedNumber).ToString();
        }
    }
}
