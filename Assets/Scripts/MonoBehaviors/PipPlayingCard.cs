using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PipPlayingCard : PlayingCard
{
    public Sprite BlankSprite;
    public SpriteRenderer HappySpriteRenderer;
    public SpriteRenderer CardBaseSprite;

    public CardPip CardPipPF;
    public List<CardPip> CardPips { get; set; } = null;

    int HighlightNeighborCount { get; set; } = -1;

    protected override void UpdateVisual()
    {
        HappySpriteRenderer.gameObject.SetActive(false);

        if (CardPips == null)
        {
            CardPips = new List<CardPip>();

            foreach (Vector3 position in GetPipLocationOffsets(RepresentingCard.FaceValue))
            {
                CardPip newPip = ObjectPooler.GetObject(CardPipPF, transform);
                newPip.transform.localPosition = ScalePipLocationOffset(position);
                CardPips.Add(newPip);
            }
        }

        if (CoordinateSet)
        {
            CardBaseSprite.sortingOrder = -1;
        }
        else
        {
            CardBaseSprite.sortingOrder = 0;
        }

        if (ShowingActualValue)
        {
            CardBaseSprite.color = Color.white;
            CardBaseSprite.sprite = CardSprites[RepresentingCard.FaceValue - 1];

            foreach (CardPip pip in CardPips)
            {
                pip.gameObject.SetActive(false);
            }

            return;
        }

        CardBaseSprite.sprite = BlankSprite;
        CardBaseSprite.color = FlavorColor;

        if (IsHappy)
        {
            HappySpriteRenderer.gameObject.SetActive(true);

            foreach (CardPip pip in CardPips)
            {
                pip.gameObject.SetActive(false);
            }

            return;
        }

        if (CannotBeCompleted)
        {
            CardBaseSprite.color = Color.Lerp(FlavorColor, Color.black, .25f);
        }

        for (int index = 0; index < CardPips.Count; index++)
        {
            if (NeighborCount > index)
            {
                CardPips[index].SetCardPipMode(CardPipMode.On);
            }
            else if (HighlightNeighborCount > index)
            {
                CardPips[index].SetCardPipMode(CardPipMode.Highlight);
            }
            else
            {
                CardPips[index].SetCardPipMode(CardPipMode.Off);
            }

            if (CoordinateSet)
            {
                CardPips[index].PipRender.sortingOrder = -1;
            }
            else
            {
                CardPips[index].PipRender.sortingOrder = 0;
            }

            CardPips[index].gameObject.SetActive(true);
        }
    }

    protected override void ResetVisuals()
    {
        if (CardPips == null)
        {
            return;
        }

        foreach (CardPip pip in CardPips)
        {
            ObjectPooler.ReturnObject(pip);
        }

        CardPips = null;
        HappySpriteRenderer.gameObject.SetActive(false);
    }

    static List<Vector3> GetPipLocationOffsets(int numberOfPips)
    {
        switch (numberOfPips)
        {
            case 1:
                return new List<Vector3>() { Vector3.zero };
            case 2:
                return new List<Vector3>() { Vector3.left + Vector3.down, Vector3.right + Vector3.up };
            case 3:
                return new List<Vector3>() { Vector3.left + Vector3.down, Vector3.zero, Vector3.right + Vector3.up };
            case 4:
                return new List<Vector3>() { Vector3.left + Vector3.down, Vector3.right + Vector3.down, Vector3.right + Vector3.up, Vector3.left + Vector3.up };
            case 5:
                return new List<Vector3>() { Vector3.left + Vector3.down, Vector3.right + Vector3.down, Vector3.zero, Vector3.right + Vector3.up, Vector3.left + Vector3.up };
            case 6:
                return new List<Vector3>() { Vector3.left + Vector3.down, Vector3.right + Vector3.down, Vector3.right, Vector3.right + Vector3.up, Vector3.left + Vector3.up, Vector3.left };
            case 7:
                return new List<Vector3>() { Vector3.left + Vector3.down, Vector3.right + Vector3.down, Vector3.right, Vector3.right + Vector3.up, Vector3.left + Vector3.up, Vector3.left, Vector3.zero };
            case 8:
                return new List<Vector3>() { Vector3.left + Vector3.down, Vector3.down, Vector3.right + Vector3.down, Vector3.right, Vector3.right + Vector3.up, Vector3.up, Vector3.left + Vector3.up, Vector3.left };
            default:
                Debug.LogError($"Could not recognize amount of pips to generate: {numberOfPips}");
                return new List<Vector3>();
        }
    }

    static Vector3 ScalePipLocationOffset(Vector3 original)
    {
        return new Vector3(original.x * .3f, original.y * .35f, -.01f);
    }

    public override void HighlightState(BlinkType toType, int possibleNeighbors)
    {
        HighlightNeighborCount = possibleNeighbors;
        base.HighlightState(toType, possibleNeighbors);
    }
}
