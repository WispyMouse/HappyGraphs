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

    public override bool Equals(object obj)
    {
        if (obj.GetType() != GetType())
        {
            return false;
        }

        CardData other = (CardData)obj;

        return other.CardSuit == this.CardSuit && other.FaceValue == this.FaceValue;
    }

    public override int GetHashCode()
    {
        return (int)CardSuit + FaceValue * 1000;
    }

    public static bool operator ==(CardData one, CardData two)
    {
        return one.Equals(two);
    }

    public static bool operator !=(CardData one, CardData two)
    {
        return !one.Equals(two);
    }
}
