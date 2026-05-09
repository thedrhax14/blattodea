using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class RadioKnob : MonoBehaviour, IInteractable, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [SerializeField]
    private InteractObjectData interactObjectData;
    private Action<float> onMouseDrag;
    [SerializeField]
    private bool isDragged = false;
    [SerializeField]
    private float sensitivity = 1f;
    bool IInteractable.CanShow => true;
    IInteractable.IObjectData IInteractable.ObjectData => interactObjectData;

    public void Init(Action<float> onMouseDrag)
    {
        this.onMouseDrag = onMouseDrag;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isDragged = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log("Dragging");
        if (!isDragged || onMouseDrag == null)
        {
            return;
        }

        Vector2 pointerDelta = eventData.delta;
        onMouseDrag(pointerDelta.x * sensitivity);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragged = false;
    }

    void IInteractable.Interact()
    {
        // isDragged = true;
    }

    void IInteractable.Stop()
    {
        // isDragged = false;
    }
}
