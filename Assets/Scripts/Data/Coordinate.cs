using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Coordinate
{
    public int X;
    public int Y;

    public Vector3 WorldspaceCoordinate;

    public Coordinate(int inX, int inY)
    {
        this.X = inX;
        this.Y = inY;

        switch (GameRulesManager.ActiveGameRules.GridTypeRule)
        {
            default:
                WorldspaceCoordinate = new Vector3((float)X, (float)Y * 1.5f);
                break;
            case GridType.SixWay:
                WorldspaceCoordinate = new Vector3((float)X, (float)Y * 1.5f + (Mathf.Abs(X) % 2 == 1 ? .75f : 0f));
                break;
        }
    }

    public HashSet<Coordinate> GetNeighbors()
    {
        return CoordinateCachingManager.GetNeighbors(this);
    }

    public override bool Equals(object obj)
    {
        if (obj.GetType() != GetType())
        {
            return false;
        }

        Coordinate other = (Coordinate)obj;

        return other.X == X && other.Y == Y;
    }

    public override int GetHashCode()
    {
        return X + Y * 1000;
    }

    public static bool operator ==(Coordinate one, Coordinate two)
    {
        return one.Equals(two);
    }

    public static bool operator !=(Coordinate one, Coordinate two)
    {
        return !one.Equals(two);
    }

    public override string ToString()
    {
        return $"({X}, {Y})";
    }
}
