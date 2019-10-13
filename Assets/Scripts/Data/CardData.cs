using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Suit { Heart, Club, Diamond, Spade }
public struct CardData
{
    public Suit CardSuit;
    public int FaceValue;

    public CardData(Suit ofCardSuit, int ofFaceValue)
    {
        this.CardSuit = ofCardSuit;
        this.FaceValue = ofFaceValue;
    }
}
