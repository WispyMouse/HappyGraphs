using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct CardData
{
    public int FaceValue;

    public CardData(int ofFaceValue)
    {
        this.FaceValue = ofFaceValue;
    }

    public override bool Equals(object obj)
    {
        if (obj.GetType() != GetType())
        {
            return false;
        }

        CardData other = (CardData)obj;

        return other.FaceValue == this.FaceValue;
    }

    public override int GetHashCode()
    {
        return FaceValue;
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
