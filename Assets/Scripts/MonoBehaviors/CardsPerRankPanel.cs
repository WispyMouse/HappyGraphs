using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardsPerRankPanel : MonoBehaviour
{
    public int RepresentedNumber { get; set; }

    public Text NumberLabel;
    public InputField NumberInput;

    public void SetRank(int rank)
    {
        RepresentedNumber = rank;
        NumberLabel.text = rank.ToString();
        NumberInput.text = GameRulesManager.FutureGameRules.GetCardsPerRank(rank).ToString();
    }

    public void TickUp()
    {
        GameRulesManager.FutureGameRules.SetCardsPerRank(RepresentedNumber, GameRulesManager.FutureGameRules.GetCardsPerRank(RepresentedNumber) + 1);
        NumberInput.text = GameRulesManager.FutureGameRules.GetCardsPerRank(RepresentedNumber).ToString();
    }

    public void TickDown()
    {
        GameRulesManager.FutureGameRules.SetCardsPerRank(RepresentedNumber, Mathf.Max(0, GameRulesManager.FutureGameRules.GetCardsPerRank(RepresentedNumber) - 1));
        NumberInput.text = GameRulesManager.FutureGameRules.GetCardsPerRank(RepresentedNumber).ToString();
    }

    public void NumberInputValueChanged()
    {
        int parsedValue;

        if (int.TryParse(NumberInput.text, out parsedValue))
        {
            GameRulesManager.FutureGameRules.SetCardsPerRank(RepresentedNumber, Mathf.Max(0, parsedValue));
            NumberInput.text = GameRulesManager.FutureGameRules.GetCardsPerRank(RepresentedNumber).ToString();
        }
        else
        {
            NumberInput.text = GameRulesManager.FutureGameRules.GetCardsPerRank(RepresentedNumber).ToString();
        }
    }
}
