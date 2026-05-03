using UnityEngine;

public class Dummy : MonoBehaviour, IBarnacleAttackable
{
    [SerializeField]
    Transform barnacleAttackPoint;

    Vector3 IBarnacleAttackable.CapturePointPosition => barnacleAttackPoint.position;

    private void Awake()
    {
    }

    void IBarnacleAttackable.Capture(Transform barnacle)
    {
        transform.SetParent(barnacle);
    }

    void IBarnacleAttackable.Release()
    {
        transform.SetParent(null);
    }
}
