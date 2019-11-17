using System.Collections;
using System.Collections.Generic;

public static class CoordinateCache
{
    public static GridType ActiveGridType { get; set; } = GridType.FourWay;
    static Dictionary<Coordinate, HashSet<Coordinate>> NeighborsCache { get; set; } = new Dictionary<Coordinate, HashSet<Coordinate>>();

    public static void ClearCache()
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

        neighbors = GenerateNeighbors(forCoordinate);

        NeighborsCache.Add(forCoordinate, neighbors);

        return neighbors;
    }

    static HashSet<Coordinate> GenerateNeighbors(Coordinate forCoordinate)
    {
        switch (ActiveGridType)
        {
            default:
            case GridType.FourWay:
                return new HashSet<Coordinate>() { new Coordinate(forCoordinate.X - 1, forCoordinate.Y), new Coordinate(forCoordinate.X + 1, forCoordinate.Y),
                    new Coordinate(forCoordinate.X, forCoordinate.Y + 1), new Coordinate(forCoordinate.X, forCoordinate.Y - 1) };
            case GridType.SixWay:
                if (forCoordinate.X % 2 == 0)
                {
                    return new HashSet<Coordinate>() { new Coordinate(forCoordinate.X - 1, forCoordinate.Y - 1), new Coordinate(forCoordinate.X, forCoordinate.Y - 1),
                    new Coordinate(forCoordinate.X + 1, forCoordinate.Y - 1), new Coordinate(forCoordinate.X + 1, forCoordinate.Y),
                new Coordinate(forCoordinate.X, forCoordinate.Y + 1), new Coordinate(forCoordinate.X - 1, forCoordinate.Y)};
                }
                else
                {
                    return new HashSet<Coordinate>() { new Coordinate(forCoordinate.X - 1, forCoordinate.Y), new Coordinate(forCoordinate.X, forCoordinate.Y - 1),
                    new Coordinate(forCoordinate.X + 1, forCoordinate.Y), new Coordinate(forCoordinate.X + 1, forCoordinate.Y + 1),
                new Coordinate(forCoordinate.X, forCoordinate.Y + 1), new Coordinate(forCoordinate.X - 1, forCoordinate.Y + 1)};
                }
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
