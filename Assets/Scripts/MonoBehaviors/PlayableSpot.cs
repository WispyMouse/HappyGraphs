using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SpotValidity { Possible, Valid, Invalid, PossibleUnsolveable }
public class PlayableSpot : MonoBehaviour
{
    public Coordinate OnCoordinate { get; private set; }

    public Sprite PossibleSprite;
    public Sprite ValidSprite;
    public Sprite InvalidSprite;
    public Sprite HoveredSprite;
    public Sprite UnsolveableSprite;
    public Sprite UnsolveableHoverSprite;
    public SpriteRenderer PlayableSpriteRenderer;
    SpotValidity previousStatus { get; set; }

    public Dictionary<PlayingCard, int> WouldMakeHappy { get; set; } = new Dictionary<PlayingCard, int>();
    public Dictionary<PlayingCard, int> WouldMakeIncompleteable { get; set; } = new Dictionary<PlayingCard, int>();
    public Dictionary<PlayingCard, int> WouldAffectNeighbors { get; set; } = new Dictionary<PlayingCard, int>();

    public void SetCoordinate(Coordinate toCoordinate)
    {
        OnCoordinate = toCoordinate;
        transform.localPosition = toCoordinate.GetWorldspacePosition();

#if UNITY_EDITOR
        name = $"PlayableSpot {toCoordinate.ToString()}";
#endif
    }

    public void SetWouldEffects(Dictionary<PlayingCard, int> wouldMakeHappy, Dictionary<PlayingCard, int> wouldMakeIncompleteable, Dictionary<PlayingCard, int> wouldAffectNeighbors)
    {
        WouldMakeHappy = wouldMakeHappy;
        WouldMakeIncompleteable = wouldMakeIncompleteable;
        WouldAffectNeighbors = wouldAffectNeighbors;
    }

    public void ClearWouldEffects()
    {
        WouldMakeHappy.Clear();
        WouldMakeIncompleteable.Clear();
        WouldAffectNeighbors.Clear();
    }

    public void SetValidity(SpotValidity toState)
    {
        previousStatus = toState;

        switch (toState)
        {
            case SpotValidity.Valid:
                PlayableSpriteRenderer.sprite = ValidSprite;
                break;
            case SpotValidity.Invalid:
                PlayableSpriteRenderer.sprite = InvalidSprite;
                break;
            case SpotValidity.Possible:
                PlayableSpriteRenderer.sprite = PossibleSprite;
                break;
            case SpotValidity.PossibleUnsolveable:
                PlayableSpriteRenderer.sprite = UnsolveableSprite;
                break;
        }
    }

    public void SetHovered(bool state)
    {
        if (state && previousStatus != SpotValidity.Invalid)
        {
            switch (previousStatus)
            {
                case SpotValidity.PossibleUnsolveable:
                    PlayableSpriteRenderer.sprite = UnsolveableHoverSprite;
                    break;
                default:
                    PlayableSpriteRenderer.sprite = HoveredSprite;
                    break;
            }

            foreach (PlayingCard curCard in WouldAffectNeighbors.Keys)
            {
                curCard.HighlightState(BlinkType.None, WouldAffectNeighbors[curCard]);
            }

            foreach (PlayingCard curCard in WouldMakeHappy.Keys)
            {
                curCard.HighlightState(BlinkType.Happy, WouldMakeHappy[curCard]);
            }

            foreach (PlayingCard curCard in WouldMakeIncompleteable.Keys)
            {
                curCard.HighlightState(BlinkType.Sad, WouldMakeIncompleteable[curCard]);
            }
        }
        else
        {
            SetValidity(previousStatus);

            foreach (PlayingCard curCard in WouldAffectNeighbors.Keys)
            {
                curCard.HighlightState(BlinkType.None);
            }

            foreach (PlayingCard curCard in WouldMakeHappy.Keys)
            {
                curCard.HighlightState(BlinkType.None);
            }

            foreach (PlayingCard curCard in WouldMakeIncompleteable.Keys)
            {
                curCard.HighlightState(BlinkType.None);
            }
        }
    }
}
