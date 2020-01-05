using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HoverableButton : Button
{
    public UnityEvent OnHover;

    public override void OnPointerEnter(PointerEventData eventData)
    {
        OnHover.Invoke();
    }
}
