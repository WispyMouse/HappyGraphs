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
    public SpriteRenderer PlayableSpriteRenderer;

    public void SetCoordinate(Coordinate toCoordinate)
    {
        OnCoordinate = toCoordinate;
        transform.localPosition = toCoordinate.WorldspaceCoordinate;
    }

    public void SetValidity(SpotValidity toState)
    {
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
}
