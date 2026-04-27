using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RadioKnob : MonoBehaviour, IInteractable
{
    [SerializeField]
    InteractObject interactObject;
    private Action<float> onMouseDrag;
    bool isDragged = false;
    float sensitivity = 2;
    float startMousePos = 0;

    string IInteractable.InteractText => interactObject.InteractText;

    KeyCode IInteractable.InteractKey => interactObject.InteractKey;

    bool IInteractable.LockCamera => interactObject.LockCamera;

    Sprite IInteractable.Icon => interactObject.Icon;
    bool IInteractable.CanShow => true;


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
