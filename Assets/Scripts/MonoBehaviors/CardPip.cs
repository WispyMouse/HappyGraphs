using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CardPipMode { Off, Highlight, On }
public class CardPip : MonoBehaviour
{
    public SpriteRenderer PipRender;

    public Sprite OffPip;
    public Sprite HighlightPip;
    public Sprite OnPip;

    CardPipMode currentMode { get; set; } = CardPipMode.Off;

    private void Awake()
    {
        SetCardPipMode(CardPipMode.Off);
    }

    public void SetCardPipMode(CardPipMode toMode)
    {
        currentMode = toMode;

        switch (currentMode)
        {
            case CardPipMode.Off:
                PipRender.sprite = OffPip;
                break;
            case CardPipMode.On:
                PipRender.sprite = OnPip;
                break;
            case CardPipMode.Highlight:
                PipRender.sprite = HighlightPip;
                break;
        }
    }
}
