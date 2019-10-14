using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class is intended to be a serializable representation of the board state
// If someone were to save their playing field, or given one, it should be seeded from this class
public class PlayFieldData
{
    public Dictionary<Coordinate, CardData> PlayedCards { get; private set; } = new Dictionary<Coordinate, CardData>();
}
