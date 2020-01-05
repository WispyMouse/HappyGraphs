using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class stores all of the Coordinate information for Neighbors
// As a Monobehaviour, it's able to clean itself up automatically, but otherwise doesn't rely on being a GameObject
public class CoordinateCachingManager : MonoBehaviour
{
    static Dictionary<Coordinate, HashSet<Coordinate>> NeighborsCache { get; set; } = new Dictionary<Coordinate, HashSet<Coordinate>>();
    static Dictionary<Coordinate, Vector3> PositionCache { get; set; } = new Dictionary<Coordinate, Vector3>();

    void Awake()
    {
        CoordinateCache.ClearCache();
        PositionCache.Clear();
    }

    public static Vector3 GetWorldspacePosition(Coordinate forCoordinate)
    {
        Vector3 position;

        if (PositionCache.TryGetValue(forCoordinate, out position))
        {
            return position;
        }

        switch (GameRulesManager.ActiveGameRules.GridTypeRule)
        {
            default:
                position = new Vector3((float)forCoordinate.X, (float)forCoordinate.Y * 1.5f);
                break;
            case GridTypeEnum.SixWay:
                position = new Vector3((float)forCoordinate.X, (float)forCoordinate.Y * 1.5f + (Mathf.Abs(forCoordinate.X) % 2 == 1 ? .75f : 0f));
                break;
        }

        PositionCache.Add(forCoordinate, position);

        return position;
    }
}

public static class CoordinateExtensions
{
    public static Vector3 GetWorldspacePosition(this Coordinate forCoordinate)
    {
        return CoordinateCachingManager.GetWorldspacePosition(forCoordinate);
    }
}