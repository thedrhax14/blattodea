using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public interface IInteractable
{
    bool CanShow { get; }
    void Interact();
    void Stop();
    IObjectData ObjectData { get; }

    public interface IObjectData
    {
        string InteractText { get; }
        KeyCode InteractKey { get; }
        bool LockCamera { get; }
        Sprite Icon { get; }
    }
}

[Serializable]
public class InteractObjectData : IInteractable.IObjectData
{
    public string InteractText;
    public KeyCode InteractKey;
    public bool LockCamera;
    public Sprite Icon;
    string IInteractable.IObjectData.InteractText => InteractText;
    KeyCode IInteractable.IObjectData.InteractKey => InteractKey;
    bool IInteractable.IObjectData.LockCamera => LockCamera;
    Sprite IInteractable.IObjectData.Icon => Icon;
}

public class InteractionManager : MonoBehaviour
{
    [SerializeField]
    Image interactIcon;
    [SerializeField]
    Text interactText;
    [SerializeField] private InputActionReference interactInputAction;
    [SerializeField] private float interactDistance = 3f;
    [SerializeField] private LayerMask interactLayer;
    Vector3 screenCenter = new Vector3(0.5f, 0.5f, 0);
    bool updateCursor = true;
    bool ownsInteractAction;
    IInteractable hoveredInteractable;
    IInteractable activeInteractable;

    public bool CameraIsLocked { get; private set; } = false;

    private void Awake()
    {
        interactIcon.gameObject.SetActive(value: false);
        interactText.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (interactInputAction == null || interactInputAction.action == null)
        {
            return;
        }

        ownsInteractAction = !interactInputAction.action.enabled;
        interactInputAction.action.Enable();
        interactInputAction.action.started += OnInteractStarted;
        interactInputAction.action.canceled += OnInteractCanceled;
    }

    private void OnDisable()
    {
        if (interactInputAction != null && interactInputAction.action != null)
        {
            interactInputAction.action.started -= OnInteractStarted;
            interactInputAction.action.canceled -= OnInteractCanceled;

            if (ownsInteractAction)
            {
                interactInputAction.action.Disable();
            }
        }

        ReleaseActiveInteractable();
        hoveredInteractable = null;
        ownsInteractAction = false;
    }

    private void Update()
    {
        if (Camera.main == null)
        {
            return;
        }

        if (updateCursor)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            updateCursor = false;
        }

        if (interactIcon == null)
        {
            return;
        }

        Ray ray = Camera.main.ViewportPointToRay(screenCenter);
        bool foundedInteractObject = false;
        IInteractable interactable = null;

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactLayer) && hit.collider.TryGetComponent(out interactable))
        {
            foundedInteractObject = true;
        }

        hoveredInteractable = foundedInteractObject ? interactable : null;

        if (foundedInteractObject && interactable.CanShow)
        {
            interactIcon.gameObject.SetActive(true);
            interactText.gameObject.SetActive(true);
            interactText.text = interactable.ObjectData.InteractText;
            interactIcon.sprite = interactable.ObjectData.Icon;
        }
        else
        {
            interactIcon.gameObject.SetActive(value: false);
            interactText.gameObject.SetActive(false);
        }
    }

    private void OnInteractStarted(InputAction.CallbackContext context)
    {
        if (hoveredInteractable == null)
        {
            return;
        }

        activeInteractable = hoveredInteractable;
        activeInteractable.Interact();
        CameraIsLocked = activeInteractable.ObjectData.LockCamera;
        Cursor.lockState = CameraIsLocked ? CursorLockMode.Confined : CursorLockMode.Locked;
    }

    private void OnInteractCanceled(InputAction.CallbackContext context)
    {
        ReleaseActiveInteractable();
    }

    private void ReleaseActiveInteractable()
    {
        if (activeInteractable != null)
        {
            activeInteractable.Stop();
            activeInteractable = null;
        }

        CameraIsLocked = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}
