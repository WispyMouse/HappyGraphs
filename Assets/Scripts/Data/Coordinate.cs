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

        WorldspaceCoordinate = new Vector3((float)X, (float)Y * 1.5f);
    }

    public HashSet<Coordinate> GetNeighbors()
    {
        return new HashSet<Coordinate>() { new Coordinate(X - 1, Y), new Coordinate(X + 1, Y), new Coordinate(X, Y + 1), new Coordinate(X, Y - 1) };
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
}
