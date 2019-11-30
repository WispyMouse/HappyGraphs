﻿public struct CardData
{
    public int FaceValue;
    public int FlavorCode;

    public CardData(int ofFaceValue, int ofFlavorCode = -1)
    {
        this.FaceValue = ofFaceValue;
        this.FlavorCode = ofFlavorCode;
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
