using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardMovementManager : MonoBehaviour
{
    public Camera MainCamera;
    public LayerMask PlayingCardLayer;

    public PlayingCard DraggedCard;
    public Vector2 DraggingOffset;

    private void Update()
    {
        if (DraggedCard != null)
        {
            if (Input.GetMouseButton(0))
            {
                DraggedCard.transform.position = GetMouseLocation() + DraggingOffset;
            }
            else
            {
                DraggedCard = null;
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                PlayingCard hoveredCard = GetHoveredCard();

                if (hoveredCard != null && hoveredCard.IsDraggable)
                {
                    DraggedCard = hoveredCard;
                    DraggingOffset = (Vector2)DraggedCard.transform.position - GetMouseLocation();
                }
            }
        }
    }

    Vector2 GetMouseLocation()
    {
        Vector3 mousePosition = MainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mousePosition2D = new Vector2(mousePosition.x, mousePosition.y);
        return mousePosition2D;
    }

    PlayingCard GetHoveredCard()
    {
        RaycastHit2D hit = Physics2D.Raycast(GetMouseLocation(), Vector2.zero, float.MaxValue, PlayingCardLayer);

        if (hit.collider != null)
        {
            PlayingCard hitCard = hit.collider.gameObject.GetComponent<PlayingCard>();
            return hitCard;
        }

        return null;
    }
}
