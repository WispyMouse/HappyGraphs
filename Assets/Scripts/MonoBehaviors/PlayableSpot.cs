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

    public HashSet<PlayingCard> WouldMakeHappy { get; set; } = new HashSet<PlayingCard>();
    public HashSet<PlayingCard> WouldMakeIncompleteable { get; set; } = new HashSet<PlayingCard>();

    public void SetCoordinate(Coordinate toCoordinate)
    {
        OnCoordinate = toCoordinate;
        transform.localPosition = toCoordinate.WorldspaceCoordinate;

#if UNITY_EDITOR
        name = $"PlayableSpot {toCoordinate.ToString()}";
#endif
    }

    public void SetWouldEffects(HashSet<PlayingCard> wouldMakeHappy, HashSet<PlayingCard> wouldMakeIncompleteable)
    {
        WouldMakeHappy = wouldMakeHappy;
        WouldMakeIncompleteable = wouldMakeIncompleteable;
    }

    public void ClearWouldEffects()
    {
        WouldMakeHappy.Clear();
        WouldMakeIncompleteable.Clear();
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

            foreach (PlayingCard curCard in WouldMakeHappy)
            {
                curCard.StartBlinking(BlinkType.Happy);
            }

            foreach (PlayingCard curCard in WouldMakeIncompleteable)
            {
                curCard.StartBlinking(BlinkType.Sad);
            }
        }
        else
        {
            SetValidity(previousStatus);

            foreach (PlayingCard curCard in WouldMakeHappy)
            {
                curCard.StartBlinking(BlinkType.None);
            }

            foreach (PlayingCard curCard in WouldMakeIncompleteable)
            {
                curCard.StartBlinking(BlinkType.None);
            }
        }
    }
}
