using UnityEngine;

public class RigidbodiesInteractor : MonoBehaviour
{
    [SerializeField]
    float pushPower = 2.0f;
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;
        if (body == null || body.isKinematic) return;
        Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);
        body.AddForceAtPosition(pushDir * pushPower, hit.point, ForceMode.Impulse);
    }

    // this code is temp for showcase only
    [SerializeField]
    Transform carriage;

    private void Awake()
    {
        transform.SetParent(carriage);
        GameEvents.Instance.CarriageStopped += () => { transform.SetParent(null); };
    }
}
