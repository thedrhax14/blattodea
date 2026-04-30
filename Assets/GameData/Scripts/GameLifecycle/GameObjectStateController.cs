using Blattodea.Core.Observers;
using UnityEngine;

public sealed class GameObjectStateController : MonoBehaviour, IModelObserver<GameLifecycleState>
{
    [SerializeField]
    private GameLifecycleState _expectedState = GameLifecycleState.InLobby;
    [SerializeField]
    private GameObject _target;

    private bool _isSubscribed;

    private void Awake()
    {
        if (_target == null)
        {
            _target = gameObject;
        }

        if (_isSubscribed)
        {
            return;
        }

        GameLifecycle.State.Subscribe(this);
        _isSubscribed = true;
    }

    private void OnDestroy()
    {
        if (!_isSubscribed)
        {
            return;
        }

        GameLifecycle.State.Unsubscribe(this);
        _isSubscribed = false;
    }

    public void OnModelChanged(GameLifecycleState model)
    {
        if (_target == null)
        {
            return;
        }

        _target.SetActive(model == _expectedState);
    }
}