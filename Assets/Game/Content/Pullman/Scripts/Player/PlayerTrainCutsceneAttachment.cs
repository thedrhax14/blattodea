using UnityEngine;

/// <summary>
/// Temporarily reparents a chosen player visual root to the carriage during the train-stop cutscene.
/// Add this to the spawned player prefab, assign <c>_targetRoot</c> to the transform that should visually
/// move with the train, and optionally assign <c>_carriage</c> if you do not want it resolved at runtime from
/// the scene <see cref="PullmanSequenceNetwork"/> object. This component preserves world pose on attach and
/// detach so the avatar does not snap. Use it together with
/// <see cref="PlayerTrainCutsceneCoordinator"/>, which triggers <see cref="AttachToCarriage"/> and
/// <see cref="DetachFromCarriage"/> from the shared cutscene events.
/// </summary>
[DisallowMultipleComponent]
public sealed class PlayerTrainCutsceneAttachment : MonoBehaviour
{
    [SerializeField]
    private Transform _targetRoot;
    [SerializeField]
    private Transform _carriage;

    private Transform _originalParent;
    private int _originalSiblingIndex;
    private bool _isAttached;

    private void Awake()
    {
        if (_targetRoot == null)
        {
            _targetRoot = transform;
        }

        TryResolveCarriage();
    }

    public void AttachToCarriage()
    {
        TryResolveCarriage();

        if (_isAttached || _targetRoot == null || _carriage == null)
        {
            return;
        }

        _originalParent = _targetRoot.parent;
        _originalSiblingIndex = _targetRoot.GetSiblingIndex();
        _targetRoot.SetParent(_carriage, true);
        _isAttached = true;
    }

    public void DetachFromCarriage()
    {
        if (!_isAttached || _targetRoot == null)
        {
            return;
        }
        Debug.DrawRay(_originalParent.position, Vector3.up * 2f, Color.blue, 5f);
        Debug.DrawRay(_targetRoot.position, Vector3.up * 2f, Color.green, 5f);
        _originalParent.SetPositionAndRotation(_targetRoot.position, _targetRoot.rotation);
        _targetRoot.SetParent(_originalParent);
        Debug.DrawRay(_originalParent.position, Vector3.up * 2f, Color.blue, 5f);
        Debug.DrawRay(_targetRoot.position, Vector3.up * 2f, Color.green, 5f);
        if (_originalParent != null)
        {
            int siblingIndex = Mathf.Clamp(_originalSiblingIndex, 0, _originalParent.childCount - 1);
            _targetRoot.SetSiblingIndex(siblingIndex);
            _targetRoot.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            Debug.DrawRay(_originalParent.position, Vector3.up * 2f, Color.blue, 5f);
            Debug.DrawRay(_targetRoot.position, Vector3.up * 2f, Color.green, 5f);
        }

        _isAttached = false;
    }

    private void TryResolveCarriage()
    {
        if (_carriage != null)
        {
            return;
        }

        if (PullmanSequenceNetwork.TryGetInstance(out PullmanSequenceNetwork sequenceNetwork))
        {
            sequenceNetwork.TryGetCarriage(out _carriage);
        }
    }
}