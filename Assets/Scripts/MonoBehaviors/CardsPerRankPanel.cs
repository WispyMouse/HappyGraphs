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
        NumberInput.text = PlayFieldManager.GetCardsPerRank(rank).ToString();
    }

    public void TickUp()
    {
        PlayFieldManager.SetCardsPerRank(RepresentedNumber, PlayFieldManager.GetCardsPerRank(RepresentedNumber) + 1);
        NumberInput.text = PlayFieldManager.GetCardsPerRank(RepresentedNumber).ToString();
    }

    public void TickDown()
    {
        PlayFieldManager.SetCardsPerRank(RepresentedNumber, Mathf.Max(0, PlayFieldManager.GetCardsPerRank(RepresentedNumber) - 1));
        NumberInput.text = PlayFieldManager.GetCardsPerRank(RepresentedNumber).ToString();
    }

    public void NumberInputValueChanged()
    {
        int parsedValue;

        if (int.TryParse(NumberInput.text, out parsedValue))
        {
            PlayFieldManager.SetCardsPerRank(RepresentedNumber, Mathf.Max(0, parsedValue));
            NumberInput.text = PlayFieldManager.GetCardsPerRank(RepresentedNumber).ToString();
        }
        else
        {
            NumberInput.text = PlayFieldManager.GetCardsPerRank(RepresentedNumber).ToString();
        }
    }
}
