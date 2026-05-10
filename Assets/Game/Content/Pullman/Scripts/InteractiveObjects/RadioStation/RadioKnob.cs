using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class RadioKnob : MonoBehaviour, IInteractable
{
    [SerializeField]
    private InteractObjectData interactObjectData;
    [SerializeField]
    private InputActionReference dragInputAction;
    private Action<float> onMouseDrag;
    [SerializeField]
    private bool isDragged = false;
    [SerializeField]
    private float sensitivity = 1f;
    private bool _ownsDragAction;

    bool IInteractable.CanShow => true;
    IInteractable.IObjectData IInteractable.ObjectData => interactObjectData;

    private void OnEnable()
    {
        _ownsDragAction = EnableActionIfNeeded(dragInputAction);
    }

    private void OnDisable()
    {
        _ownsDragAction = false;
        isDragged = false;
    }

    private void Update()
    {
        if (!isDragged || onMouseDrag == null)
        {
            return;
        }

        Vector2 pointerDelta = ReadVector2(dragInputAction);
        if (pointerDelta.sqrMagnitude <= 0f)
        {
            return;
        }

        onMouseDrag(pointerDelta.x * sensitivity);
    }

    public void Init(Action<float> onMouseDrag)
    {
        this.onMouseDrag = onMouseDrag;
    }

    void IInteractable.Interact()
    {
        isDragged = true;
    }

    void IInteractable.Stop()
    {
        isDragged = false;
    }

    private static Vector2 ReadVector2(InputActionReference actionReference)
    {
        if (actionReference == null || actionReference.action == null)
        {
            return Vector2.zero;
        }

        return actionReference.action.ReadValue<Vector2>();
    }

    private static bool EnableActionIfNeeded(InputActionReference actionReference)
    {
        if (actionReference == null || actionReference.action == null || actionReference.action.enabled)
        {
            return false;
        }

        actionReference.action.Enable();
        return true;
    }
}
