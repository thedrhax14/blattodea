using FishNet.Object;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class PullmanSequenceNetwork : NetworkBehaviour
{
    public static PullmanSequenceNetwork Instance { get; private set; }
    public static bool IsTrainStopCutsceneActive { get; private set; }

    [SerializeField]
    private NetworkGameLifecycle _gameLifecycle;
    [SerializeField]
    private Transform _carriage;

    private bool _stopLeverActivated;
    private bool _trainStopCutsceneActive;
    private bool _mainDoorPanelLeverPulled;
    private bool _mainDoorOpeningStarted;
    private bool _mainDoorOpeningEffectsTriggered;
    private bool _mainDoorSmashEffectsTriggered;
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

        IsTrainStopCutsceneActive = false;
    }

    private void OnEnable()
    {
        GameEvents.Instance.CarriageStopped += OnCarriageStopped;
    }

    private void OnDisable()
    {
        GameEvents.Instance.CarriageStopped -= OnCarriageStopped;
    }

    public static bool TryGetInstance(out PullmanSequenceNetwork instance)
    {
        instance = Instance;
        return instance != null;
    }

    public bool TryGetCarriage(out Transform carriage)
    {
        carriage = _carriage;
        return carriage != null;
    }

    public void RequestStopLeverActivation()
    {
        if (!IsNetworkRunning())
        {
            StartTrainStopCutsceneLocally();
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

    public void RequestMainDoorPanelLeverPull()
    {
        if (!IsNetworkRunning())
        {
            PullMainDoorPanelLeverLocally();
            return;
        }

        if (IsServerStarted)
        {
            PullMainDoorPanelLeverOnServer();
            return;
        }

        ServerRequestMainDoorPanelLeverPull();
    }

    public void ReportMainDoorOpeningEffects()
    {
        if (!IsNetworkRunning())
        {
            TriggerMainDoorOpeningEffectsLocally();
            return;
        }

        if (IsServerStarted)
        {
            TriggerMainDoorOpeningEffectsOnServer();
            return;
        }

        ServerReportMainDoorOpeningEffects();
    }

    public void ReportMainDoorSmashEffects()
    {
        if (!IsNetworkRunning())
        {
            TriggerMainDoorSmashEffectsLocally();
            return;
        }

        if (IsServerStarted)
        {
            TriggerMainDoorSmashEffectsOnServer();
            return;
        }

        ServerReportMainDoorSmashEffects();
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
    private void ServerRequestMainDoorPanelLeverPull()
    {
        PullMainDoorPanelLeverOnServer();
    }

    [ServerRpc(RequireOwnership = false)]
    private void ServerReportMainDoorOpeningEffects()
    {
        TriggerMainDoorOpeningEffectsOnServer();
    }

    [ServerRpc(RequireOwnership = false)]
    private void ServerReportMainDoorSmashEffects()
    {
        TriggerMainDoorSmashEffectsOnServer();
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
    private void ObserversStartTrainStopCutscene()
    {
        StartTrainStopCutsceneLocally();
    }

    [ObserversRpc(ExcludeServer = true)]
    private void ObserversEndTrainStopCutscene()
    {
        EndTrainStopCutsceneLocally();
    }

    [ObserversRpc(ExcludeServer = true)]
    private void ObserversPullMainDoorPanelLever()
    {
        PullMainDoorPanelLeverLocally();
    }

    [ObserversRpc(ExcludeServer = true)]
    private void ObserversStartMainDoorOpening()
    {
        StartMainDoorOpeningLocally();
    }

    [ObserversRpc(ExcludeServer = true)]
    private void ObserversTriggerMainDoorOpeningEffects()
    {
        TriggerMainDoorOpeningEffectsLocally();
    }

    [ObserversRpc(ExcludeServer = true)]
    private void ObserversTriggerMainDoorSmashEffects()
    {
        TriggerMainDoorSmashEffectsLocally();
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
        StartTrainStopCutsceneOnServer();
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
        _mainDoorOpeningEffectsTriggered = false;
        _mainDoorSmashEffectsTriggered = false;
        _gameLifecycle?.TryBeginDoorOpens();
        StartMainDoorOpeningLocally();
        ObserversStartMainDoorOpening();
    }

    [Server]
    private void PullMainDoorPanelLeverOnServer()
    {
        if (_mainDoorPanelLeverPulled)
        {
            return;
        }

        _mainDoorPanelLeverPulled = true;
        PullMainDoorPanelLeverLocally();
        ObserversPullMainDoorPanelLever();
    }

    [Server]
    private void TriggerMainDoorOpeningEffectsOnServer()
    {
        if (_mainDoorOpeningEffectsTriggered)
        {
            return;
        }

        if (!_mainDoorOpeningStarted)
        {
            return;
        }

        _mainDoorOpeningEffectsTriggered = true;
        TriggerMainDoorOpeningEffectsLocally();
        ObserversTriggerMainDoorOpeningEffects();
    }

    [Server]
    private void TriggerMainDoorSmashEffectsOnServer()
    {
        if (_mainDoorSmashEffectsTriggered)
        {
            return;
        }

        if (!_mainDoorOpeningStarted)
        {
            return;
        }

        _mainDoorSmashEffectsTriggered = true;
        TriggerMainDoorSmashEffectsLocally();
        ObserversTriggerMainDoorSmashEffects();
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

    private void OnCarriageStopped()
    {
        if (!IsNetworkRunning())
        {
            EndTrainStopCutsceneLocally();
            return;
        }

        if (!IsServerStarted)
        {
            return;
        }

        EndTrainStopCutsceneOnServer();
    }

    [Server]
    private void StartTrainStopCutsceneOnServer()
    {
        if (_trainStopCutsceneActive)
        {
            return;
        }

        _trainStopCutsceneActive = true;
        StartTrainStopCutsceneLocally();
        ObserversStartTrainStopCutscene();
    }

    [Server]
    private void EndTrainStopCutsceneOnServer()
    {
        if (!_trainStopCutsceneActive)
        {
            return;
        }

        _trainStopCutsceneActive = false;
        EndTrainStopCutsceneLocally();
        ObserversEndTrainStopCutscene();
    }

    private static void StartTrainStopCutsceneLocally()
    {
        IsTrainStopCutsceneActive = true;
        GameEvents.Instance.RaiseTrainStopCutsceneStarted();
    }

    private static void EndTrainStopCutsceneLocally()
    {
        IsTrainStopCutsceneActive = false;
        GameEvents.Instance.RaiseTrainStopCutsceneEnded();
    }

    private static void StartMainDoorOpeningLocally()
    {
        GameEvents.Instance.RaiseMainDoorOpeningStarted();
    }

    private static void PullMainDoorPanelLeverLocally()
    {
        GameEvents.Instance.RaiseMainDoorPanelLeverPulled();
    }

    private static void TriggerMainDoorOpeningEffectsLocally()
    {
        GameEvents.Instance.RaiseMainDoorOpeningEffectsTriggered();
    }

    private static void TriggerMainDoorSmashEffectsLocally()
    {
        GameEvents.Instance.RaiseMainDoorSmashEffectsTriggered();
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