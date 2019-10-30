using System.Collections;
using System.Collections.Generic;
using System.Linq;

// This class is intended to be a serializable representation of the board state
// If someone were to save their playing field, or given one, it should be seeded from this class
public class PlayFieldData
{
    public Dictionary<Coordinate, CardData> PlayedCards { get; private set; } = new Dictionary<Coordinate, CardData>();
    protected HashSet<Coordinate> ValidPlayableSpacesCache { get; set; } = null;
    protected HashSet<Coordinate> HappyCoordinatesCache { get; set; } = new HashSet<Coordinate>();
    protected HashSet<Coordinate> NotHappyCoordinatesCache { get; set; } = new HashSet<Coordinate>();

    public PlayFieldData CloneData()
    {
        PlayFieldData newField = new PlayFieldData();
        newField.PlayedCards = new Dictionary<Coordinate, CardData>(PlayedCards);
        newField.ValidPlayableSpacesCache = new HashSet<Coordinate>(ValidPlayableSpacesCache);
        newField.HappyCoordinatesCache = new HashSet<Coordinate>(HappyCoordinatesCache);
        newField.NotHappyCoordinatesCache = new HashSet<Coordinate>(NotHappyCoordinatesCache);
        SolutionEngine.TimesFieldIsCloned++;
        return newField;
    }

    public void SetCard(CardData forCard, Coordinate onCoordinate)
    {
        ValidPlayableSpacesCache = null;
        HappyCoordinatesCache = null;
        NotHappyCoordinatesCache = null;

        if (PlayedCards.ContainsKey(onCoordinate))
        {
            PlayedCards[onCoordinate] = forCard;
        }
        else
        {
            PlayedCards.Add(onCoordinate, forCard);
        }
    }

    public void RemoveCard(Coordinate atCoordinate)
    {
        ValidPlayableSpacesCache = null;
        HappyCoordinatesCache = null;
        NotHappyCoordinatesCache = null;

        PlayedCards.Remove(atCoordinate);
    }

    public bool IsSpotValidForCard(CardData forCard, Coordinate onCoordinate)
    {
        SolutionEngine.IsSpotValidForCardCheck++;
        int neighborsAtPosition = OccuppiedNeighborsAtCoordinate(onCoordinate);
        return forCard.FaceValue >= neighborsAtPosition;
    }

    public HashSet<Coordinate> GetHappyCoordinates()
    {
        if (HappyCoordinatesCache != null && NotHappyCoordinatesCache != null)
        {
            SolutionEngine.HappyCoordinatesCacheUsed++;
            return HappyCoordinatesCache;
        }

        SolutionEngine.TimeSpentCalculatingHappyCoordinates.Start();
        HashSet<Coordinate> happyCoordinates = new HashSet<Coordinate>();
        HashSet<Coordinate> notHappyCoordinates = new HashSet<Coordinate>();

        foreach (Coordinate curCoordinate in PlayedCards.Keys)
        {
            if (ShouldCoordinateBeHappy(curCoordinate))
            {
                happyCoordinates.Add(curCoordinate);
            }
            else
            {
                notHappyCoordinates.Add(curCoordinate);
            }
        }

        HappyCoordinatesCache = happyCoordinates;
        NotHappyCoordinatesCache = notHappyCoordinates;
        SolutionEngine.TimeSpentCalculatingHappyCoordinates.Stop();
        return happyCoordinates;
    }

    public HashSet<Coordinate> GetNotHappyCoordinates()
    {
        if (NotHappyCoordinatesCache == null)
        {
            GetHappyCoordinates();
        }

        return NotHappyCoordinatesCache;
    }

    public bool ShouldCoordinateBeHappy(Coordinate forCoordinate)
    {
        if (!PlayedCards.ContainsKey(forCoordinate))
        {
            Debug.LogError($"Asked if coordinate {forCoordinate.ToString()} should be happy, but that coordinate isn't in this play field.");
            return false;
        }

        int neighbors = OccuppiedNeighborsAtCoordinate(forCoordinate);
        SolutionEngine.ShouldCoordinateBeHappyCheck++;

        if (PlayedCards[forCoordinate].FaceValue == neighbors)
        {
            return true;
        }

        return false;
    }

    public HashSet<Coordinate> GetIncompleteableCoordinates()
    {
        HashSet<Coordinate> incompleteableCoordinates = new HashSet<Coordinate>();

        foreach (Coordinate curCoordinate in GetNotHappyCoordinates())
        {
            if (ShouldCoordinateBeIncompletable(curCoordinate))
            {
                incompleteableCoordinates.Add(curCoordinate);
            }
        }

        return incompleteableCoordinates;
    }

    public HashSet<Coordinate> GetValidPlayableSpaces()
    {
        if (ValidPlayableSpacesCache != null)
        {
            SolutionEngine.ValidPlayableSpaceCacheUsed++;
            return ValidPlayableSpacesCache;
        }

        // For every played card, add every neighboring coordinate to a HashSet (no duplicates)
        HashSet<Coordinate> happyCoordinates = GetHappyCoordinates();

        SolutionEngine.TimeSpentCalculatingValidPlayableSpace.Start();

        HashSet<Coordinate> neighborsOfHappyCoordinates = new HashSet<Coordinate>(happyCoordinates.SelectMany(coordinate => coordinate.GetNeighbors()));

        HashSet<Coordinate> consideredCoordinates = new HashSet<Coordinate>();
        HashSet<Coordinate> finalCut = new HashSet<Coordinate>();

        foreach (Coordinate currentCoordinate in NotHappyCoordinatesCache)
        {
            consideredCoordinates.UnionWith(currentCoordinate.GetNeighbors());
        }

        // For each considered Coordinate, if all the following is true, consider that spot valid:
        // - there are no cards on that spot
        // - there are no happy cards neighboring that spot

        foreach (Coordinate consideredCoordinate in consideredCoordinates)
        {
            if (PlayedCards.ContainsKey(consideredCoordinate))
            {
                continue;
            }

            if (neighborsOfHappyCoordinates.Contains(consideredCoordinate))
            {
                continue;
            }

            finalCut.Add(consideredCoordinate);
        }

        ValidPlayableSpacesCache = finalCut;

        SolutionEngine.TimeSpentCalculatingValidPlayableSpace.Stop();

        return finalCut;
    }

    public bool ShouldCoordinateBeIncompletable(Coordinate forCoordinate)
    {
        if (!PlayedCards.ContainsKey(forCoordinate))
        {
            Debug.LogError($"Asked if coordinate {forCoordinate.ToString()} should be incompleteable, but that coordinate isn't in this play field.");
            return false;
        }

        SolutionEngine.ShouldCoordinateBeIncompletableCheck++;

        int requiredNeighbors = PlayedCards[forCoordinate].FaceValue;
        int occuppiedNeighbors = OccuppiedNeighborsAtCoordinate(forCoordinate);

        if (requiredNeighbors == occuppiedNeighbors)
        {
            return false; // Presumably already happy
        }

        HashSet<Coordinate> validSpaces = GetValidPlayableSpaces();

        int playableSpotNeighbors = forCoordinate.GetNeighbors().Count(neighbor => validSpaces.Contains(neighbor));

        if (occuppiedNeighbors + playableSpotNeighbors < requiredNeighbors)
        {
            return true;
        }

        return false;
    }

    public int OccuppiedNeighborsAtCoordinate(Coordinate forCoordinate)
    {

        SolutionEngine.TimesOccuppiedNeighborCountIsAskedFor++;
        int answer = forCoordinate.GetNeighbors().Count(neighbor => PlayedCards.ContainsKey(neighbor));

        return answer;
    }

    public bool AreAnyMovesPossible(List<CardData> hand)
    {
        SolutionEngine.RoamingCheck.Start();
        foreach (Coordinate coordinate in GetValidPlayableSpaces())
        {
            foreach (CardData card in hand)
            {
                if (IsSpotValidForCard(card, coordinate))
                {
                    SolutionEngine.RoamingCheck.Stop();
                    return true;
                }
            }
        }
        SolutionEngine.RoamingCheck.Stop();
        return false;
    }

    public int CountOfCardsThatAreNotHappy()
    {
        return GetNotHappyCoordinates().Count;
    }

    public int NeededNeighbors()
    {
        int sum = 0;

        foreach (Coordinate curCoordinate in GetNotHappyCoordinates())
        {
            SolutionEngine.NeededNeighborsCheck++;
            sum += PlayedCards[curCoordinate].FaceValue - OccuppiedNeighborsAtCoordinate(curCoordinate);
        }

        return sum;
    }
}
