﻿using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class GameAction
{
    public CardData? CardDrawn { get; set; }
    public CardData? CardPlayed { get; set; }
    public Coordinate? CoordinatePlayedOn { get; set; }

    public string GetActionText()
    {
        StringBuilder actionText = new StringBuilder();

        if (CardDrawn.HasValue)
        {
            actionText.Append($"[Draw {CardDrawn.Value.FaceValue}]");
        }

        if (CardPlayed.HasValue)
        {
            actionText.Append($"[Play {CardPlayed.Value.FaceValue} at ({CoordinatePlayedOn.Value.X}, {CoordinatePlayedOn.Value.Y})]");
        }

        return actionText.ToString();
    }

    public static GameAction FromCardDrawnFromDeck(CardData drawnCard)
    {
        return new GameAction() { CardDrawn = drawnCard };
    }

    public static GameAction FromCardPlayed(CardData playedCard, Coordinate onCoordinate)
    {
        return new GameAction() { CardPlayed = playedCard, CoordinatePlayedOn = onCoordinate };
    }
}