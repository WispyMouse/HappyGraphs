using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DegreesOfSpeed { Instantly, Quickly, Slowly, None }
public enum BlinkType { None, Happy, Sad }
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
    static bool ShowingActualValue { get; set; } = false;

    public Vector3 AnimationTargetLocation { get; private set; }
    public DegreesOfSpeed AnimationSpeed { get; private set; } = DegreesOfSpeed.None;

    BlinkType CurBlinkType { get; set; } = BlinkType.None;
    public SpriteRenderer BlinkOverlaySpriteRenderer;
    Color blinkTargetColor { get; set; }

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
        IsHappy = isHappy;
        UpdateVisual();
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
        CannotBeCompleted = cannotBeCompleted;
        UpdateVisual();
    }

    public void Reset()
    {
        SetIncompleteness(false);
        SetHappiness(false);
        CoordinateSet = false;
    }

    public void StartBlinking(BlinkType toType)
    {
        CurBlinkType = toType;
        BlinkOverlaySpriteRenderer.color = Color.white;

        switch (toType)
        {
            default:
            case BlinkType.None:
                BlinkOverlaySpriteRenderer.gameObject.SetActive(false);
                break;
            case BlinkType.Happy:
                BlinkOverlaySpriteRenderer.gameObject.SetActive(true);
                blinkTargetColor = Color.green;
                BlinkOverlaySpriteRenderer.color = blinkTargetColor;
                break;
            case BlinkType.Sad:
                BlinkOverlaySpriteRenderer.gameObject.SetActive(true);
                blinkTargetColor = Color.black;
                BlinkOverlaySpriteRenderer.color = blinkTargetColor;
                break;
        }
    }

    public void ActualValueToggle(bool toState)
    {
        ShowingActualValue = toState;
        UpdateVisual();
    }

    void UpdateVisual()
    {
        if (ShowingActualValue)
        {
            CardSprite.color = Color.white;
            CardSprite.sprite = CardSprites[RepresentingCard.FaceValue - 1];
        }
        else
        {
            if (IsHappy)
            {
                CardSprite.color = Color.white;
                CardSprite.sprite = HappySprite;
            }
            else if (CannotBeCompleted)
            {
                CardSprite.color = Color.gray;
                CardSprite.sprite = CardSprites[RepresentingCard.FaceValue - 1];
            }
            else
            {
                CardSprite.color = Color.white;
                CardSprite.sprite = CardSprites[RepresentingCard.FaceValue - 1];

            }
        }
    }
}
