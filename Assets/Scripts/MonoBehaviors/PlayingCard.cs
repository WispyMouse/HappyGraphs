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

    public bool CoordinateSet { get; private set; }
    public bool IsHappy { get; private set; }
    public bool CannotBeCompleted { get; private set; }

    public Vector3 AnimationTargetLocation { get; private set; }
    public DegreesOfSpeed AnimationSpeed { get; private set; } = DegreesOfSpeed.None;

    public void SetCardData(CardData toCardData)
    {
        RepresentingCard = toCardData;
        CardSprite.sprite = CardSprites[toCardData.FaceValue - 1];

#if UNITY_EDITOR
        name = $"PlayingCard {toCardData.FaceValue}";
#endif
    }

    public void SetCoordinate(Coordinate toCoordinate, DegreesOfSpeed speed = DegreesOfSpeed.Slowly)
    {
        OnCoordinate = toCoordinate;
        CoordinateSet = true;
        AnimateMovement(toCoordinate.WorldspaceCoordinate, speed);
        CardSprite.sortingOrder = -1;

#if UNITY_EDITOR
        name = $"PlayingCard {RepresentingCard.FaceValue} {toCoordinate.ToString()}";
#endif
    }

    public bool IsDraggable
    {
        get
        {
            return !CoordinateSet;
        }
    }

    public void SetHappiness(bool isHappy)
    {
        if (isHappy)
        {
            CardSprite.sprite = HappySprite;
        }
        else
        {
            CardSprite.sprite = CardSprites[RepresentingCard.FaceValue - 1];
        }
        
        IsHappy = isHappy;
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
                transform.localPosition = toLocation;
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
            transform.localPosition = AnimationTargetLocation;
            AnimationSpeed = DegreesOfSpeed.None;
        }
        else if (AnimationSpeed != DegreesOfSpeed.None)
        {
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, AnimationTargetLocation, Time.deltaTime * GetAnimationSpeedValue());

            if (Vector3.Distance(transform.localPosition, AnimationTargetLocation) < 0.01f)
            {
                transform.localPosition = AnimationTargetLocation;
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

    public void SetIncompleteness(bool cannotBeCompleted)
    {
        if (cannotBeCompleted)
        {
            CardSprite.color = Color.gray;
        }
        else
        {
            CardSprite.color = Color.white;
        }
        
        CannotBeCompleted = cannotBeCompleted;
    }

    public void Reset()
    {
        SetIncompleteness(false);
        SetHappiness(false);
        CoordinateSet = false;
    }
}
