using Blattodea.Core.Observers;
using UnityEngine;
using UnityEngine.Events;

public enum StateObserverInvokeMode : byte
{
    EveryTime = 0,
    OnceUntilOpposite = 1
}

public sealed class StateObserverUnityEvent : MonoBehaviour, IModelObserver<GameLifecycleState>
{
    [SerializeField]
    private GameLifecycleState _expectedState = GameLifecycleState.InLobby;
    [SerializeField]
    private StateObserverInvokeMode _matchInvokeMode = StateObserverInvokeMode.OnceUntilOpposite;
    [SerializeField]
    private StateObserverInvokeMode _mismatchInvokeMode = StateObserverInvokeMode.OnceUntilOpposite;
    [SerializeField]
    private UnityEvent _onMatched;
    [SerializeField]
    private UnityEvent _onNotMatched;

    private bool _isSubscribed;
    private bool _hasObservedState;
    private bool _lastWasMatch;

    private void Awake()
    {
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
        bool isMatch = model == _expectedState;

        if (ShouldInvoke(isMatch))
        {
            if (isMatch)
            {
                _onMatched?.Invoke();
            }
            else
            {
                _onNotMatched?.Invoke();
            }
        }

        _lastWasMatch = isMatch;
        _hasObservedState = true;
    }

    private bool ShouldInvoke(bool isMatch)
    {
        StateObserverInvokeMode mode = isMatch ? _matchInvokeMode : _mismatchInvokeMode;

        if (mode == StateObserverInvokeMode.EveryTime)
        {
            return true;
        }

        return !_hasObservedState || _lastWasMatch != isMatch;
    }
}