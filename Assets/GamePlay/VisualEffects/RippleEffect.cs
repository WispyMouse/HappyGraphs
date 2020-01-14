using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RippleEffect : MonoBehaviour
{
    float maxRippleDistance { get; set; }
    float curRippleDistance { get; set; }

    HashSet<PlayingCard> cardRippledOn { get; set; } = new HashSet<PlayingCard>();
    public LayerMask PlayingCardLayerMask;
    public MeshRenderer Renderer;

    public void SetPosition(Coordinate toCoordinate)
    {
        transform.position = toCoordinate.GetWorldspacePosition() + Vector3.forward * .5f;
        curRippleDistance = 0;
        cardRippledOn.Clear();
        Renderer.material.SetFloat("_Radius", 0);

        maxRippleDistance = transform.localScale.x;
    }

    private void Update()
    {
        curRippleDistance += Time.deltaTime * 5f;

        if (curRippleDistance >= maxRippleDistance)
        {
            ObjectPooler.ReturnObject(this);
            return;
        }

        float ripplePercentile = curRippleDistance / maxRippleDistance;
        Renderer.material.SetFloat("_Radius", ripplePercentile);

        Collider2D[] collisions = Physics2D.OverlapCircleAll(transform.position, curRippleDistance * .5f, PlayingCardLayerMask);
        foreach (Collider2D curCollider in collisions)
        {
            PlayingCard curCard = curCollider.GetComponent<PlayingCard>();

            if (!cardRippledOn.Contains(curCard))
            {
                if (curCard.CoordinateSet)
                {
                    curCard.PlayCardSound();
                }
                
                cardRippledOn.Add(curCard);
            }
        }
    }
}
