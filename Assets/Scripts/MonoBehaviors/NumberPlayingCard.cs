using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NumberPlayingCard : PlayingCard
{
    public List<Sprite> CardSprites;
    public Sprite HappySprite;
    public SpriteRenderer CardSprite;

    protected override void UpdateVisual()
    {
        if (CoordinateSet)
        {
            CardSprite.sortingOrder = -1;
        }
        else
        {
            CardSprite.sortingOrder = 0;
        }

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
                CardSprite.sprite = CardSprites[RepresentingCard.FaceValue - 1 - NeighborCount];
            }
            else
            {
                CardSprite.color = Color.white;
                CardSprite.sprite = CardSprites[RepresentingCard.FaceValue - 1 - NeighborCount];

            }
        }
    }
}
