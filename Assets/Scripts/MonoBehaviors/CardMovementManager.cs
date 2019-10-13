using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardMovementManager : MonoBehaviour
{
    public PlayFieldManager PlayFieldManagerInstance;
    public Camera MainCamera;
    public LayerMask PlayingCardLayer;
    public LayerMask PlayableSpotLayer;

    public PlayingCard DraggedCard { get; set; }
    public Vector2 DraggingOffset { get; set; }
    public Vector2 PickedUpFrom { get; set; }

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
                PlayableSpot hoveredSpot = GetHoveredSpot();

                if (hoveredSpot == null)
                {
                    DraggedCard.AnimateMovement(PickedUpFrom, DegreesOfSpeed.Quickly);
                    DraggedCard = null;
                    PlayFieldManagerInstance.UpdateValidityOfPlayableSpots(null);
                }
                else
                {
                    if (!PlayFieldManagerInstance.TryPlayerPlaysCard(DraggedCard, hoveredSpot.OnCoordinate))
                    {
                        DraggedCard.AnimateMovement(PickedUpFrom, DegreesOfSpeed.Quickly);
                        PlayFieldManagerInstance.UpdateValidityOfPlayableSpots(null);
                    }

                    DraggedCard = null;
                }               
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
                    PickedUpFrom = hoveredCard.transform.position;
                    DraggingOffset = (Vector2)DraggedCard.transform.position - GetMouseLocation();
                    PlayFieldManagerInstance.UpdateValidityOfPlayableSpots(DraggedCard);
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

    PlayableSpot GetHoveredSpot()
    {
        RaycastHit2D hit = Physics2D.Raycast(GetMouseLocation(), Vector2.zero, float.MaxValue, PlayableSpotLayer);

        if (hit.collider != null)
        {
            PlayableSpot hitSpot = hit.collider.gameObject.GetComponent<PlayableSpot>();
            return hitSpot;
        }

        return null;
    }
}
