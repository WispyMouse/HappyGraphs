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
}
