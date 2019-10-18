using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class stores all of the Coordinate information for Neighbors
// As a Monobehaviour, it's able to clean itself up automatically, but otherwise doesn't rely on being a GameObject
public class CoordinateCachingManager : MonoBehaviour
{
    static Dictionary<Coordinate, HashSet<Coordinate>> NeighborsCache { get; set; } = new Dictionary<Coordinate, HashSet<Coordinate>>();

    void Awake()
    {
        NeighborsCache.Clear();
    }

    public static HashSet<Coordinate> GetNeighbors(Coordinate forCoordinate)
    {
        HashSet<Coordinate> neighbors;

        if (NeighborsCache.TryGetValue(forCoordinate, out neighbors))
        {
            return neighbors;
        }

        neighbors = GenerateNeighbors(forCoordinate, PlayFieldManager.ActiveGridType);

        NeighborsCache.Add(forCoordinate, neighbors);

        return neighbors;
    }

    static HashSet<Coordinate> GenerateNeighbors(Coordinate forCoordinate, GridType forType)
    {
        switch (forType)
        {
            default:
            case GridType.FourWay:
                return new HashSet<Coordinate>() { new Coordinate(forCoordinate.X - 1, forCoordinate.Y), new Coordinate(forCoordinate.X + 1, forCoordinate.Y),
                    new Coordinate(forCoordinate.X, forCoordinate.Y + 1), new Coordinate(forCoordinate.X, forCoordinate.Y - 1) };
            case GridType.EightWay:
                return new HashSet<Coordinate>() { new Coordinate(forCoordinate.X - 1, forCoordinate.Y - 1), new Coordinate(forCoordinate.X - 1, forCoordinate.Y),
                    new Coordinate(forCoordinate.X - 1, forCoordinate.Y + 1), new Coordinate(forCoordinate.X, forCoordinate.Y - 1),
                    new Coordinate(forCoordinate.X, forCoordinate.Y + 1), new Coordinate(forCoordinate.X + 1, forCoordinate.Y - 1),
                    new Coordinate(forCoordinate.X + 1, forCoordinate.Y), new Coordinate(forCoordinate.X + 1, forCoordinate.Y + 1)};
        }
    }

    public static HashSet<Coordinate> GetNeighborsInRadius(Coordinate forCoordinate, int radius)
    {
        HashSet<Coordinate> neighbors = new HashSet<Coordinate>();
        HashSet<Coordinate> frontier = new HashSet<Coordinate>() { forCoordinate };

        for (int ii = 0; ii < radius; ii++)
        {
            HashSet<Coordinate> newFrontier = new HashSet<Coordinate>();

            foreach (Coordinate curCoordinate in frontier)
            {
                newFrontier.UnionWith(GetNeighbors(curCoordinate));
            }

            neighbors.UnionWith(frontier);
            frontier = newFrontier;
        }

        neighbors.UnionWith(frontier);

        return neighbors;
    }
}
