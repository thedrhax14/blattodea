using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public interface IInteractable
{
    void Interact();
    void Stop();
    string InteractText { get; }
    KeyCode InteractKey { get; }
    bool LockCamera { get; }
    Sprite Icon { get; }
}
[Serializable]
public class InteractObject
{
    public string InteractText;
    public KeyCode InteractKey;
    public bool LockCamera;
    public Sprite Icon;

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
    public bool CameraIsLocked { get; private set; } = false;
    private void Awake()
    {
        interactIcon.gameObject.SetActive(value: false);
        interactText.gameObject.SetActive(false);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    private void Update()
    {
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
                if (Input.GetKeyDown(interactable.InteractKey))
                {
                    interactable.Interact();
                    CameraIsLocked = interactable.LockCamera;
                    Cursor.lockState = CameraIsLocked ? CursorLockMode.Confined : CursorLockMode.Locked;
                }
                if (Input.GetKeyUp(interactable.InteractKey))
                {
                    interactable.Stop();
                    CameraIsLocked = false;
                }
            }
        }
        if (foundedInteractObject)
        {
            interactIcon.gameObject.SetActive(true);
            interactText.gameObject.SetActive(true);
            interactText.text = interactable.InteractText;
            interactIcon.sprite = interactable.Icon;
        }
        else
        {
            interactIcon.gameObject.SetActive(value: false);
            interactText.gameObject.SetActive(false);
        }
    }
}
