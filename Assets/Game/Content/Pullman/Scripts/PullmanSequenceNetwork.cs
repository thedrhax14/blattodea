using FishNet.Object;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class PullmanSequenceNetwork : NetworkBehaviour
{
    public static PullmanSequenceNetwork Instance { get; private set; }

    [SerializeField]
    private NetworkGameLifecycle _gameLifecycle;

    private bool _stopLeverActivated;
    private bool _mainDoorOpeningStarted;
    private bool _mainDoorOpened;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"Duplicate {nameof(PullmanSequenceNetwork)} detected.", this);
            return;
        }

        Instance = this;

        if (_gameLifecycle == null)
        {
            _gameLifecycle = GetComponent<NetworkGameLifecycle>();
        }

        if (_gameLifecycle == null)
        {
            _gameLifecycle = FindFirstObjectByType<NetworkGameLifecycle>();
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public static bool TryGetInstance(out PullmanSequenceNetwork instance)
    {
        instance = Instance;
        return instance != null;
    }

    public void RequestStopLeverActivation()
    {
        if (!IsNetworkRunning())
        {
            ActivateStopLeverLocally();
            _gameLifecycle?.TryStartGame();
            return;
        }

        if (IsServerStarted)
        {
            ActivateStopLeverOnServer();
            return;
        }

        ServerRequestStopLeverActivation();
    }

    public void RequestMainDoorOpening()
    {
        if (!IsNetworkRunning())
        {
            _gameLifecycle?.TryBeginDoorOpens();
            StartMainDoorOpeningLocally();
            return;
        }

        if (IsServerStarted)
        {
            StartMainDoorOpeningOnServer();
            return;
        }

        ServerRequestMainDoorOpening();
    }

    public void ReportMainDoorOpened()
    {
        if (!IsNetworkRunning())
        {
            MarkMainDoorOpenedLocally();
            _gameLifecycle?.TryBeginConcluding();
            return;
        }

        if (IsServerStarted)
        {
            MarkMainDoorOpenedOnServer();
            return;
        }

        ServerReportMainDoorOpened();
    }

    [ServerRpc(RequireOwnership = false)]
    private void ServerRequestStopLeverActivation()
    {
        ActivateStopLeverOnServer();
    }

    [ServerRpc(RequireOwnership = false)]
    private void ServerRequestMainDoorOpening()
    {
        StartMainDoorOpeningOnServer();
    }

    [ServerRpc(RequireOwnership = false)]
    private void ServerReportMainDoorOpened()
    {
        MarkMainDoorOpenedOnServer();
    }

    [ObserversRpc(ExcludeServer = true)]
    private void ObserversActivateStopLever()
    {
        ActivateStopLeverLocally();
    }

    [ObserversRpc(ExcludeServer = true)]
    private void ObserversStartMainDoorOpening()
    {
        StartMainDoorOpeningLocally();
    }

    [ObserversRpc(ExcludeServer = true)]
    private void ObserversMarkMainDoorOpened()
    {
        MarkMainDoorOpenedLocally();
    }

    [Server]
    private void ActivateStopLeverOnServer()
    {
        if (_stopLeverActivated)
        {
            return;
        }

        _stopLeverActivated = true;
        _gameLifecycle?.TryStartGame();
        ActivateStopLeverLocally();
        ObserversActivateStopLever();
    }

    [Server]
    private void StartMainDoorOpeningOnServer()
    {
        if (_mainDoorOpeningStarted || _mainDoorOpened)
        {
            return;
        }

        if (!_stopLeverActivated)
        {
            return;
        }

        if (!GameStates.Instance.CarriageStopped)
        {
            return;
        }

        _mainDoorOpeningStarted = true;
        _gameLifecycle?.TryBeginDoorOpens();
        StartMainDoorOpeningLocally();
        ObserversStartMainDoorOpening();
    }

    [Server]
    private void MarkMainDoorOpenedOnServer()
    {
        if (!_mainDoorOpeningStarted || _mainDoorOpened)
        {
            return;
        }

        _mainDoorOpened = true;
        MarkMainDoorOpenedLocally();
        ObserversMarkMainDoorOpened();
        _gameLifecycle?.TryBeginConcluding();
    }

    private static void ActivateStopLeverLocally()
    {
        GameEvents.Instance.ActivateStopLever();
    }

    private static void StartMainDoorOpeningLocally()
    {
        GameEvents.Instance.RaiseMainDoorOpeningStarted();
    }

    private static void MarkMainDoorOpenedLocally()
    {
        GameEvents.Instance.RaiseMainDoorOpened();
    }

    private bool IsNetworkRunning()
    {
        return IsServerStarted || IsClientStarted;
    }
}