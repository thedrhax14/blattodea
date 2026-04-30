using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class NetworkGameLifecycle : NetworkBehaviour
{
    [SerializeField]
    private GameLifecycleState _initialState = GameLifecycleState.Offline;
    [SerializeField]
    [Min(0f)]
    private float _concludingDurationSeconds = 5f;

    private readonly SyncVar<GameLifecycleState> _state = new();
    private readonly SyncTimer _concludingTimer = new();

    public GameLifecycleState CurrentState => _state.Value;
    public float ConcludingRemaining => _concludingTimer.Remaining;

    private void Awake()
    {
        _state.SetInitialValues(_initialState);
        _state.UpdateSendRate(0f);
        _state.OnChange += OnStateChanged;

        _concludingTimer.UpdateSendRate(0f);
        _concludingTimer.OnChange += OnConcludingTimerChanged;
    }

    private void OnDestroy()
    {
        _state.OnChange -= OnStateChanged;
        _concludingTimer.OnChange -= OnConcludingTimerChanged;
    }

    private void Update()
    {
        if (!IsServerStarted && !IsClientStarted)
        {
            return;
        }

        _concludingTimer.Update();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        if (_state.Value != GameLifecycleState.InLobby)
        {
            ApplyState(GameLifecycleState.InLobby);
        }

        GameLifecycle.TrySetState(GameLifecycleState.InLobby);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        GameLifecycle.TrySetState(GetInitialClientState());
    }

    public override void OnStopNetwork()
    {
        base.OnStopNetwork();
        GameLifecycle.SetOffline();
    }

    [Server]
    public bool TryGoOffline()
    {
        return TrySetState(GameLifecycleState.Offline);
    }

    [Server]
    public bool TryEnterLobby()
    {
        return TrySetState(GameLifecycleState.InLobby);
    }

    [Server]
    public bool TryStartGame()
    {
        return TrySetState(GameLifecycleState.InProgress);
    }

    [Server]
    public bool TryBeginDoorOpens()
    {
        return TrySetState(GameLifecycleState.DoorOpens);
    }

    [Server]
    public bool TryBeginConcluding()
    {
        return TrySetState(GameLifecycleState.Concluding);
    }

    [Server]
    public bool TryFinishGame()
    {
        return TrySetState(GameLifecycleState.GameOver);
    }

    [Server]
    public bool TrySetState(GameLifecycleState nextState)
    {
        Debug.Log($"Attempting lifecycle transition {_state.Value} -> {nextState}...", this);
        if (!IsTransitionAllowed(_state.Value, nextState))
        {
            Debug.LogWarning($"Rejected lifecycle transition {_state.Value} -> {nextState}.", this);
            return false;
        }

        Debug.Log($"Lifecycle transition {_state.Value} -> {nextState} approved.", this);
        ApplyState(nextState);
        return true;
    }

    [Server]
    public void SetState(GameLifecycleState nextState)
    {
        if (!TrySetState(nextState))
        {
            Debug.LogWarning($"Rejected lifecycle transition {_state.Value} -> {nextState}.", this);
        }
    }

    private void OnStateChanged(GameLifecycleState previousState, GameLifecycleState nextState, bool asServer)
    {
        GameLifecycle.TrySetState(nextState);
    }

    private void OnConcludingTimerChanged(SyncTimerOperation operation, float previous, float next, bool asServer)
    {
        Debug.Log($"Concluding timer changed: {operation} ({previous} -> {next})", this);
        if (operation != SyncTimerOperation.Finished)
        {
            Debug.Log($"Concluding timer not finished yet ({operation}).", this);
            return;
        }

        GameLifecycle.TrySetState(GameLifecycleState.GameOver);

        if (asServer && _state.Value == GameLifecycleState.Concluding)
        {
            Debug.Log($"Concluding timer finished. Transitioning to GameOver.", this);
            ApplyState(GameLifecycleState.GameOver);
        }
    }

    [Server]
    private void ApplyState(GameLifecycleState nextState)
    {
        _state.Value = nextState;

        if (nextState == GameLifecycleState.Concluding)
        {
            Debug.Log("Concluding timer started.", this);
            _concludingTimer.StartTimer(_concludingDurationSeconds);
            return;
        }

        if (_concludingTimer.Remaining > 0f)
        {
            _concludingTimer.StopTimer();
        }
    }

    private static bool IsTransitionAllowed(GameLifecycleState currentState, GameLifecycleState nextState)
    {
        if (currentState == nextState)
        {
            return false;
        }

        return currentState switch
        {
            GameLifecycleState.Offline => nextState == GameLifecycleState.InLobby,
            GameLifecycleState.InLobby => nextState == GameLifecycleState.Offline || nextState == GameLifecycleState.InProgress,
            GameLifecycleState.InProgress => nextState == GameLifecycleState.Offline || nextState == GameLifecycleState.DoorOpens || nextState == GameLifecycleState.Concluding || nextState == GameLifecycleState.GameOver,
            GameLifecycleState.DoorOpens => nextState == GameLifecycleState.Offline || nextState == GameLifecycleState.Concluding || nextState == GameLifecycleState.GameOver,
            GameLifecycleState.Concluding => nextState == GameLifecycleState.Offline || nextState == GameLifecycleState.GameOver,
            GameLifecycleState.GameOver => nextState == GameLifecycleState.Offline || nextState == GameLifecycleState.InLobby,
            _ => false
        };
    }

    private GameLifecycleState GetInitialClientState()
    {
        return _state.Value == GameLifecycleState.Offline
            ? GameLifecycleState.InLobby
            : _state.Value;
    }
}