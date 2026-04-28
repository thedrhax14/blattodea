using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RadioKnob : MonoBehaviour, IInteractable
{
    [SerializeField]
    InteractObjectData interactObjectData;
    private Action<float> onMouseDrag;
    bool isDragged = false;
    float sensitivity = 2;
    float startMousePos = 0;
    bool IInteractable.CanShow => true;
    IInteractable.IObjectData IInteractable.ObjectData => interactObjectData;
    public void Init(Action<float> onMouseDrag)
    {
        this.onMouseDrag = onMouseDrag;
    }
    void Update()
    {
        if (isDragged)
        {
            onMouseDrag((startMousePos - Input.mousePosition.x) / (Screen.width / 2) * -1 * sensitivity);
            startMousePos = Input.mousePosition.x;
        }
    }
    void IInteractable.Interact()
    {
        isDragged = true;
        startMousePos = Input.mousePosition.x;
    }
    void IInteractable.Stop()
    {
        isDragged = false;
    }
}
