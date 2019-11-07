using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public enum UndoType { ContinueAfter, StopAfter, CannotUndo }
public class GameAction
{
    public CardData? CardDrawn { get; set; }
    public CardData? CardPlayed { get; set; }
    public Coordinate? CoordinatePlayedOn { get; set; }

    public CardData? SeedCard { get; set; }
    public PlayFieldRuntime PreviousPlayfield { get; set; }

    public UndoType ActionUndoType { get; set; }

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

        if (SeedCard.HasValue)
        {
            actionText.Append($"[Seed {CardPlayed.Value.FaceValue}]");
        }

        if (PreviousPlayfield != null)
        {
            actionText.Append("[Switch Field]");
        }

        if (ActionUndoType == UndoType.ContinueAfter)
        {
            actionText.Append("[Continue]");
        }

        return actionText.ToString();
    }

    public static GameAction FromCardDrawnFromDeck(CardData drawnCard)
    {
        return new GameAction() { CardDrawn = drawnCard, ActionUndoType = UndoType.ContinueAfter };
    }

    public static GameAction FromCardPlayed(CardData playedCard, Coordinate onCoordinate)
    {
        return new GameAction() { CardPlayed = playedCard, CoordinatePlayedOn = onCoordinate, ActionUndoType = UndoType.StopAfter };
    }

    public static GameAction FromNewPlayingField(CardData seedCard, PlayFieldRuntime previousField)
    {
        return new GameAction() { SeedCard = seedCard, CoordinatePlayedOn = new Coordinate(0, 0), PreviousPlayfield = previousField, ActionUndoType = UndoType.ContinueAfter };
    }
}
