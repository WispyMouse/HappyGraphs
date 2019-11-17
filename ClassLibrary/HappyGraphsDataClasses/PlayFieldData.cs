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
    protected bool HappyCoordinatesAreDirty { get; set; } = true;

    public PlayFieldData CloneData()
    {
        PlayFieldData newField = new PlayFieldData();
        newField.PlayedCards = new Dictionary<Coordinate, CardData>(PlayedCards);
        newField.ValidPlayableSpacesCache = new HashSet<Coordinate>(ValidPlayableSpacesCache);
        newField.HappyCoordinatesCache = new HashSet<Coordinate>(HappyCoordinatesCache);
        newField.NotHappyCoordinatesCache = new HashSet<Coordinate>(NotHappyCoordinatesCache);
        newField.HappyCoordinatesAreDirty = HappyCoordinatesAreDirty;
        return newField;
    }

    public void SetCard(CardData forCard, Coordinate onCoordinate)
    {
        ValidPlayableSpacesCache = null;
        NotHappyCoordinatesCache.Add(onCoordinate);
        HappyCoordinatesAreDirty = true;

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
        PlayedCards.Remove(atCoordinate);

        ValidPlayableSpacesCache = null;
        HappyCoordinatesCache = new HashSet<Coordinate>();
        NotHappyCoordinatesCache = new HashSet<Coordinate>(PlayedCards.Keys);
        HappyCoordinatesAreDirty = true;
    }

    public bool IsSpotValidForCard(CardData forCard, Coordinate onCoordinate)
    {
        int neighborsAtPosition = OccuppiedNeighborsAtCoordinate(onCoordinate);
        return forCard.FaceValue >= neighborsAtPosition;
    }

    public HashSet<Coordinate> GetHappyCoordinates()
    {
        if (!HappyCoordinatesAreDirty)
        {
            return HappyCoordinatesCache;
        }

        HashSet<Coordinate> newlyHappyCoordinates = new HashSet<Coordinate>();
        HashSet<Coordinate> notHappyCoordinates = new HashSet<Coordinate>();

        foreach (Coordinate curCoordinate in NotHappyCoordinatesCache)
        {
            if (ShouldCoordinateBeHappy(curCoordinate))
            {
                newlyHappyCoordinates.Add(curCoordinate);
            }
            else
            {
                notHappyCoordinates.Add(curCoordinate);
            }
        }

        HappyCoordinatesCache.UnionWith(newlyHappyCoordinates);
        NotHappyCoordinatesCache = notHappyCoordinates;

        HappyCoordinatesAreDirty = false;
        return HappyCoordinatesCache;
    }

    public HashSet<Coordinate> GetNotHappyCoordinates()
    {
        if (HappyCoordinatesAreDirty)
        {
            GetHappyCoordinates();
        }

        return NotHappyCoordinatesCache;
    }

    public bool ShouldCoordinateBeHappy(Coordinate forCoordinate)
    {
        if (!PlayedCards.ContainsKey(forCoordinate))
        {
            // TODO: Find a good way to log an error
            // Debug.LogError($"Asked if coordinate {forCoordinate.ToString()} should be happy, but that coordinate isn't in this play field.");
            return false;
        }

        int neighbors = OccuppiedNeighborsAtCoordinate(forCoordinate);

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

    public bool AreAnyCoordinatesAreIncompleteable()
    {
        foreach (Coordinate curCoordinate in GetNotHappyCoordinates())
        {
            if (ShouldCoordinateBeIncompletable(curCoordinate))
            {
                return true;
            }
        }
        return false;
    }

    public HashSet<Coordinate> GetValidPlayableSpaces()
    {
        if (ValidPlayableSpacesCache != null && ValidPlayableSpacesCache.Count > 0)
        {
            return ValidPlayableSpacesCache;
        }

        if (PlayedCards.Count == 0)
        {
            ValidPlayableSpacesCache = new HashSet<Coordinate>() { new Coordinate(0, 0) };
            return ValidPlayableSpacesCache;
        }

        // For every played card, add every neighboring coordinate to a HashSet (no duplicates)
        HashSet<Coordinate> neighborsOfHappyCoordinates = new HashSet<Coordinate>(GetHappyCoordinates().SelectMany(coordinate => coordinate.GetNeighbors()));
        HashSet<Coordinate> neighborsOfNotHappyCoordinates = new HashSet<Coordinate>(GetNotHappyCoordinates().SelectMany(coordinate => coordinate.GetNeighbors()));
        HashSet<Coordinate> finalCut = new HashSet<Coordinate>();

        // For each considered Coordinate, if all the following is true, consider that spot valid:
        // - there are no cards on that spot
        // - there are no happy cards neighboring that spot
        foreach (Coordinate consideredCoordinate in neighborsOfNotHappyCoordinates)
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
        return ValidPlayableSpacesCache;
    }

    public bool ShouldCoordinateBeIncompletable(Coordinate forCoordinate)
    {
        if (!PlayedCards.ContainsKey(forCoordinate))
        {
            // TODO: Find a good way to log an error
            // Debug.LogError($"Asked if coordinate {forCoordinate.ToString()} should be incompleteable, but that coordinate isn't in this play field.");
            return false;
        }

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
        int answer = forCoordinate.GetNeighbors().Count(neighbor => PlayedCards.ContainsKey(neighbor));

        return answer;
    }

    public bool AreAnyMovesPossible(List<CardData> hand)
    {
        foreach (Coordinate coordinate in GetValidPlayableSpaces())
        {
            foreach (CardData card in hand)
            {
                if (IsSpotValidForCard(card, coordinate))
                {
                    return true;
                }
            }
        }
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
            sum += PlayedCards[curCoordinate].FaceValue - OccuppiedNeighborsAtCoordinate(curCoordinate);
        }

        return sum;
    }
}
