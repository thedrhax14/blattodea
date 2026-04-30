using UnityEngine;

/// <summary>
/// Bridges the replicated Pullman train-stop cutscene events to the local player setup.
/// Add this to the spawned player prefab alongside <see cref="PlayerCutsceneLock"/> and
/// <see cref="PlayerTrainCutsceneAttachment"/>, then assign those references or use Reset to auto-fill them.
/// Do not place this in the scene. When the cutscene starts it locks that player and attaches the configured
/// visual root to the carriage; when the cutscene ends it detaches the visual root and restores player control.
/// This works on spawned player prefabs as well, because it listens to shared events and also synchronizes
/// immediately with the current cutscene state when enabled.
/// </summary>
[DisallowMultipleComponent]
public sealed class PlayerTrainCutsceneCoordinator : MonoBehaviour
{
    [SerializeField]
    private PlayerCutsceneLock _cutsceneLock;
    [SerializeField]
    private PlayerTrainCutsceneAttachment _attachment;

    private void Reset()
    {
        if (_cutsceneLock == null)
        {
            _cutsceneLock = GetComponent<PlayerCutsceneLock>();
        }

        if (_attachment == null)
        {
            _attachment = GetComponent<PlayerTrainCutsceneAttachment>();
        }
    }

    private void OnEnable()
    {
        GameEvents.Instance.TrainStopCutsceneStarted += OnTrainStopCutsceneStarted;
        GameEvents.Instance.TrainStopCutsceneEnded += OnTrainStopCutsceneEnded;

        if (PullmanSequenceNetwork.IsTrainStopCutsceneActive)
        {
            OnTrainStopCutsceneStarted();
        }
    }

    private void OnDisable()
    {
        GameEvents.Instance.TrainStopCutsceneStarted -= OnTrainStopCutsceneStarted;
        GameEvents.Instance.TrainStopCutsceneEnded -= OnTrainStopCutsceneEnded;
    }

    private void OnTrainStopCutsceneStarted()
    {
        _cutsceneLock?.SetLocked(true);
        _attachment?.AttachToCarriage();
    }

    private void OnTrainStopCutsceneEnded()
    {
        _attachment?.DetachFromCarriage();
        _cutsceneLock?.SetLocked(false);
    }
}