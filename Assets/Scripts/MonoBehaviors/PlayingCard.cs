using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DegreesOfSpeed { Instantly, Quickly, Slowly, None }
public class PlayingCard : MonoBehaviour
{
    float QuicklyAnimationSpeed = 18.0f;
    float SlowlyAnimationSpeed = 9.0f;

    public CardData RepresentingCard { get; private set; }
    public Coordinate OnCoordinate { get; private set; }

    public List<Sprite> CardSprites;
    public Sprite HappySprite;
    public SpriteRenderer CardSprite;

    public bool CoordinateSet { get; set; }
    public bool IsHappy { get; set; }

    public Vector3 AnimationTargetLocation { get; private set; }
    public DegreesOfSpeed AnimationSpeed { get; private set; } = DegreesOfSpeed.None;

    public void SetCardData(CardData toCardData)
    {
        RepresentingCard = toCardData;
        CardSprite.sprite = CardSprites[toCardData.FaceValue - 1];
    }

    public void SetCoordinate(Coordinate toCoordinate)
    {
        OnCoordinate = toCoordinate;
        CoordinateSet = true;
        AnimateMovement(toCoordinate.WorldspaceCoordinate, DegreesOfSpeed.Slowly);
    }

    public bool IsDraggable
    {
        get
        {
            return !CoordinateSet;
        }
    }

    public void BecomeHappy()
    {
        CardSprite.sprite = HappySprite;
        IsHappy = true;
    }

    public void AnimateMovement(Vector3 toLocation, DegreesOfSpeed speed)
    {
        switch (speed)
        {
            case DegreesOfSpeed.Quickly:
            case DegreesOfSpeed.Slowly:
                AnimationTargetLocation = toLocation;
                AnimationSpeed = speed;
                break;
            case DegreesOfSpeed.Instantly:
                transform.position = toLocation;
                AnimationSpeed = DegreesOfSpeed.None;
                break;
            default:
            case DegreesOfSpeed.None:
                AnimationSpeed = DegreesOfSpeed.None;
                break;
        }
    }

    public void StopAnimations()
    {
        AnimationSpeed = DegreesOfSpeed.None;
    }

    private void Update()
    {
        if (AnimationSpeed == DegreesOfSpeed.Instantly)
        {
            transform.position = AnimationTargetLocation;
            AnimationSpeed = DegreesOfSpeed.None;
        }
        else if (AnimationSpeed != DegreesOfSpeed.None)
        {
            transform.position = Vector3.MoveTowards(transform.position, AnimationTargetLocation, Time.deltaTime * GetAnimationSpeedValue());

            if (Vector3.Distance(transform.position, AnimationTargetLocation) == 0)
            {
                AnimationSpeed = DegreesOfSpeed.None;
            }
        }
    }

    float GetAnimationSpeedValue()
    {
        switch (AnimationSpeed)
        {
            case DegreesOfSpeed.Quickly:
                return QuicklyAnimationSpeed;
            case DegreesOfSpeed.Slowly:
                return SlowlyAnimationSpeed;
            default:
                return 0;
                
        }
    }
}
