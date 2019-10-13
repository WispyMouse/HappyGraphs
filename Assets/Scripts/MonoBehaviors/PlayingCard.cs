﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayingCard : MonoBehaviour
{
    public CardData RepresentingCard { get; private set; }
    public Coordinate OnCoordinate { get; private set; }

    public List<Sprite> CardSprites;
    public Sprite HappySprite;
    public SpriteRenderer SpriteRenderer;

    public bool CoordinateSet { get; set; }
    public bool IsHappy { get; set; }

    public void SetCardData(CardData toCardData)
    {
        RepresentingCard = toCardData;
        SpriteRenderer.sprite = CardSprites[toCardData.FaceValue - 1];
    }

    public void SetCoordinate(Coordinate toCoordinate)
    {
        OnCoordinate = toCoordinate;
        transform.position = toCoordinate.WorldspaceCoordinate;
        CoordinateSet = true;
    }

    public bool IsDraggable
    {
        get
        {
            return !CoordinateSet;
        }
    }

    public void BecomeHappy()
    {
        SpriteRenderer.sprite = HappySprite;
        IsHappy = true;
    }
}
