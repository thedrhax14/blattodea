using UnityEngine;

/// <summary>
/// Disables the configured gameplay behaviours and GameObjects while the train-stop cutscene is active.
/// Add this to the spawned player prefab on the player object that owns movement and interaction scripts,
/// then assign the components and objects which must be turned off during the cutscene, such as locomotion,
/// interaction, input, or gameplay-only visuals. Pair it with <see cref="PlayerTrainCutsceneCoordinator"/>,
/// which also belongs on the same player prefab and calls <see cref="SetLocked(bool)"/> automatically from
/// the replicated Pullman cutscene events.
/// </summary>
[DisallowMultipleComponent]
public sealed class PlayerCutsceneLock : MonoBehaviour
{
    [SerializeField]
    private Behaviour[] _behavioursToDisable;
    [SerializeField]
    private GameObject[] _gameObjectsToDisable;

    public bool IsLocked { get; private set; }

    public void SetLocked(bool isLocked)
    {
        if (IsLocked == isLocked)
        {
            return;
        }

        IsLocked = isLocked;

        for (int index = 0; index < _behavioursToDisable.Length; index++)
        {
            Behaviour behaviour = _behavioursToDisable[index];
            if (behaviour != null)
            {
                behaviour.enabled = !isLocked;
            }
        }

        for (int index = 0; index < _gameObjectsToDisable.Length; index++)
        {
            GameObject target = _gameObjectsToDisable[index];
            if (target != null)
            {
                target.SetActive(!isLocked);
            }
        }
    }
}