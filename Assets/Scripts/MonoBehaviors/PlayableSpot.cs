using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SpotValidity { Possible, Valid, Invalid }
public class PlayableSpot : MonoBehaviour
{
    public Coordinate OnCoordinate { get; private set; }

    public Sprite PossibleSprite;
    public Sprite ValidSprite;
    public Sprite InvalidSprite;
    public Sprite HoveredSprite;
    public SpriteRenderer PlayableSpriteRenderer;
    SpotValidity previousStatus { get; set; }

    public void SetCoordinate(Coordinate toCoordinate)
    {
        OnCoordinate = toCoordinate;
        transform.localPosition = toCoordinate.WorldspaceCoordinate;
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
        }
    }

    public void SetHovered(bool state)
    {
        if (state && previousStatus != SpotValidity.Invalid)
        {
            PlayableSpriteRenderer.sprite = HoveredSprite;
        }
        else
        {
            SetValidity(previousStatus);
        }
    }
}
