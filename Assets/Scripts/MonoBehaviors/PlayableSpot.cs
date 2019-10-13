using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayableSpot : MonoBehaviour
{
    public Coordinate OnCoordinate { get; private set; }

    public void SetCoordinate(Coordinate toCoordinate)
    {
        OnCoordinate = toCoordinate;
        transform.position = toCoordinate.WorldspaceCoordinate;
    }
}
