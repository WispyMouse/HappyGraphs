using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// This class is intended to be a serializable representation of the board state
// If someone were to save their playing field, or given one, it should be seeded from this class
public class PlayFieldData
{
    public Dictionary<Coordinate, CardData> PlayedCards { get; private set; } = new Dictionary<Coordinate, CardData>();
    private HashSet<Coordinate> ValidPlayableSpacesCache { get; set; } = null;
    private HashSet<Coordinate> HappyCoordinatesCache { get; set; } = new HashSet<Coordinate>();

    public PlayFieldData CloneData()
    {
        PlayFieldData newField = new PlayFieldData();
        newField.PlayedCards = new Dictionary<Coordinate, CardData>(PlayedCards);
        return newField;
    }

    public void SetCard(CardData forCard, Coordinate onCoordinate)
    {
        ValidPlayableSpacesCache = null;
        HappyCoordinatesCache = null;

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

        PlayedCards.Remove(atCoordinate);
    }

    public bool IsSpotValidForCard(CardData forCard, Coordinate onCoordinate)
    {
        int neighborsAtPosition = OccuppiedNeighborsAtCoordinate(onCoordinate);
        return forCard.FaceValue >= neighborsAtPosition;
    }

    public HashSet<Coordinate> GetHappyCoordinates()
    {
        if (HappyCoordinatesCache != null)
        {
            return HappyCoordinatesCache;
        }

        HashSet<Coordinate> happyCoordinates = new HashSet<Coordinate>();

        foreach (Coordinate curCoordinate in PlayedCards.Keys)
        {
            if (ShouldCoordinateBeHappy(curCoordinate))
            {
                happyCoordinates.Add(curCoordinate);
            }
        }

        HappyCoordinatesCache = happyCoordinates;
        return happyCoordinates;
    }

    public bool ShouldCoordinateBeHappy(Coordinate forCoordinate)
    {
        if (!PlayedCards.ContainsKey(forCoordinate))
        {
            Debug.LogError($"Asked if coordinate {forCoordinate.ToString()} should be happy, but that coordinate isn't in this play field.");
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
        HashSet<Coordinate> validPlayableSpaces = GetValidPlayableSpaces();

        foreach (Coordinate curCoordinate in PlayedCards.Keys)
        {
            if (ShouldCoordinateBeIncompletable(curCoordinate, validPlayableSpaces))
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
            return ValidPlayableSpacesCache;
        }

        // For every played card, add every neighboring coordinate to a HashSet (no duplicates)
        HashSet<Coordinate> happyCoordinates = GetHappyCoordinates();
        HashSet<Coordinate> neighborsOfHappyCoordinates = new HashSet<Coordinate>(happyCoordinates.SelectMany(coordinate => coordinate.GetNeighbors()));
        
        HashSet<Coordinate> consideredCoordinates = new HashSet<Coordinate>();
        HashSet<Coordinate> finalCut = new HashSet<Coordinate>();

        foreach (Coordinate currentCoordinate in PlayedCards.Keys.Where(coordinate => !happyCoordinates.Contains(coordinate)))
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

        return finalCut;
    }

    public bool ShouldCoordinateBeIncompletable(Coordinate forCoordinate, HashSet<Coordinate> validPlayableSpaces)
    {
        if (!PlayedCards.ContainsKey(forCoordinate))
        {
            Debug.LogError($"Asked if coordinate {forCoordinate.ToString()} should be incompleteable, but that coordinate isn't in this play field.");
            return false;
        }

        int requiredNeighbors = PlayedCards[forCoordinate].FaceValue;
        int occuppiedNeighbors = OccuppiedNeighborsAtCoordinate(forCoordinate);

        if (requiredNeighbors == occuppiedNeighbors)
        {
            return false; // Presumably already happy
        }

        int playableSpotNeighbors = forCoordinate.GetNeighbors().Count(neighbor => validPlayableSpaces.Contains(neighbor));

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

    public bool AreAnyMovesPossible(IEnumerable<CardData> hand)
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
        return PlayedCards.Keys.Count - GetHappyCoordinates().Count;
    }

    public int NeededNeighbors()
    {
        int sum = 0;

        foreach (Coordinate curCoordinate in PlayedCards.Keys)
        {
            sum += PlayedCards[curCoordinate].FaceValue - OccuppiedNeighborsAtCoordinate(curCoordinate);
        }

        return sum;
    }
}
