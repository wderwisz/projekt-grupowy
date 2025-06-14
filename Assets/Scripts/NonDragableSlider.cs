using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DisableSliderDragging : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public void OnBeginDrag(PointerEventData eventData)
    {
        eventData.Use(); // blokuje rozpoczęcie dragowania
    }

    public void OnDrag(PointerEventData eventData)
    {
        eventData.Use(); // blokuje drag
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        eventData.Use(); // blokuje zakończenie dragowania
    }
}