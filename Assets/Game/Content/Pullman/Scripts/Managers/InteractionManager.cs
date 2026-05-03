using System;
using System.Runtime.InteropServices;
using UnityEngine;
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
    [SerializeField] private float interactDistance = 3f;
    [SerializeField] private LayerMask interactLayer;
    Vector3 screenCenter = new Vector3(0.5f, 0.5f, 0);
    bool updateCursor = true;
    public bool CameraIsLocked { get; private set; } = false;
    private void Awake()
    {
        interactIcon.gameObject.SetActive(value: false);
        interactText.gameObject.SetActive(false);
        
    }
    
    private void Update()
    {
        if(Camera.main == null) {
            Debug.Log("Waiting for camera...");
            return;
        }
        if(updateCursor)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            updateCursor = false;
        }
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        if (interactIcon == null) { return; }
        Ray ray = Camera.main.ViewportPointToRay(screenCenter);
        RaycastHit hit;
        bool foundedInteractObject = false;
        IInteractable interactable = null;
        if (Physics.Raycast(ray, out hit, interactDistance, interactLayer))
        {
            if (hit.collider.TryGetComponent(out interactable))
            {
                foundedInteractObject = true;
                if (Input.GetKeyDown(interactable.ObjectData.InteractKey))
                {
                    interactable.Interact();
                    CameraIsLocked = interactable.ObjectData.LockCamera;
                    Cursor.lockState = CameraIsLocked ? CursorLockMode.Confined : CursorLockMode.Locked;
                }
                if (Input.GetKeyUp(interactable.ObjectData.InteractKey))
                {
                    interactable.Stop();
                    CameraIsLocked = false;
                    Cursor.lockState = CursorLockMode.Locked;
                }
            }
        }
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
}
