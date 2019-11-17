using System.Collections;
using System.Collections.Generic;

public struct Coordinate
{
    public int X;
    public int Y;

    public Coordinate(int inX, int inY)
    {
        this.X = inX;
        this.Y = inY;
    }

    public HashSet<Coordinate> GetNeighbors()
    {
        return CoordinateCache.GetNeighbors(this);
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
