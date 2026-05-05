using FishNet.Managing;
using FishNet.Transporting;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace FishNet.Example
{
    [DisallowMultipleComponent]
    public class RuntimeNetworkHud : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private NetworkManager _networkManager;

        [Header("Connection")]
        [SerializeField]
        private string _address = "127.0.0.1";
        [SerializeField]
        private ushort _port = 7770;

        [Header("Visibility")]
        [SerializeField]
        private bool _startVisible = true;
    #if ENABLE_INPUT_SYSTEM
        [SerializeField]
        private InputActionReference _toggleHudAction;
    #endif

        [Header("Appearance")]
        [SerializeField]
        private Rect _hudArea = new Rect(12f, 12f, 340f, 260f);
        [SerializeField]
        private Color _startedColor = new Color(0.3f, 0.85f, 0.4f, 1f);
        [SerializeField]
        private Color _stoppedColor = new Color(0.85f, 0.3f, 0.3f, 1f);
        [SerializeField]
        private Color _changingColor = new Color(0.95f, 0.7f, 0.2f, 1f);

        private LocalConnectionState _clientState = LocalConnectionState.Stopped;
        private LocalConnectionState _serverState = LocalConnectionState.Stopped;
        private string _portText = "7770";
        private bool _isSubscribed;
        private bool _isVisible;
    #if ENABLE_INPUT_SYSTEM
        private bool _inputSubscribed;
        private bool _ownsEnabledToggleAction;
    #endif

        private bool IsLinuxServerEnvironment => Application.platform == RuntimePlatform.LinuxServer;

        private void Awake()
        {
            _portText = _port.ToString();
            _isVisible = _startVisible;

            if (IsLinuxServerEnvironment)
                enabled = false;
        }

        private void OnEnable()
        {
            if (IsLinuxServerEnvironment)
                return;

            TryInitialize();
            RefreshInputSubscription();
        }

        private void Start()
        {
            if (IsLinuxServerEnvironment)
                return;

            TryInitialize();
        }

        private void OnDisable()
        {
            Unsubscribe();
            UnsubscribeFromToggleAction();
        }

        private void OnDestroy()
        {
            Unsubscribe();
            UnsubscribeFromToggleAction();
        }

        private void OnGUI()
        {
            if (IsLinuxServerEnvironment)
                return;

            if (!TryInitialize(logIfMissing: false))
                return;

            Vector2 referenceResolution = new Vector2(1920f, 1080f);
            Matrix4x4 previousMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity,
                new Vector3(Screen.width / referenceResolution.x, Screen.height / referenceResolution.y, 1f));

            if (!_isVisible)
            {
                GUI.matrix = previousMatrix;
                return;
            }

            GUILayout.BeginArea(_hudArea, GUI.skin.box);
            GUILayout.Label("Runtime Network HUD");
            GUILayout.Space(6f);

            DrawStatusRow("Server", _serverState);
            DrawStatusRow("Client", _clientState);

            GUILayout.Space(8f);
            GUILayout.Label("Address");
            _address = GUILayout.TextField(_address ?? string.Empty);

            GUILayout.Space(4f);
            GUILayout.Label("Port");
            _portText = GUILayout.TextField(_portText ?? string.Empty);

            GUILayout.Space(10f);
            using (new GUILayout.HorizontalScope())
            {
                GUI.enabled = CanToggle(_serverState);
                if (GUILayout.Button(GetToggleLabel("Server", _serverState), GUILayout.Height(32f)))
                    ToggleServer();

                GUI.enabled = CanToggle(_clientState);
                if (GUILayout.Button(GetToggleLabel("Client", _clientState), GUILayout.Height(32f)))
                    ToggleClient();
            }

            GUILayout.Space(6f);

            GUI.enabled = CanToggleHost();
            if (GUILayout.Button(GetHostLabel(), GUILayout.Height(32f)))
                ToggleHost();

            GUI.enabled = true;
            GUILayout.EndArea();
            GUI.matrix = previousMatrix;
        }

        private bool TryInitialize(bool logIfMissing = true)
        {
            if (_networkManager == null)
                _networkManager = FindFirstObjectByType<NetworkManager>();

            if (_networkManager == null)
            {
                if (logIfMissing)
                    Debug.LogError($"{nameof(RuntimeNetworkHud)} could not find a {nameof(NetworkManager)}.", this);

                return false;
            }

            if (_isSubscribed)
                return true;

            _networkManager.ServerManager.OnServerConnectionState += OnServerConnectionState;
            _networkManager.ClientManager.OnClientConnectionState += OnClientConnectionState;
            _serverState = _networkManager.ServerManager.Started ? LocalConnectionState.Started : LocalConnectionState.Stopped;
            _clientState = _networkManager.ClientManager.Started ? LocalConnectionState.Started : LocalConnectionState.Stopped;
            _isSubscribed = true;
            return true;
        }

        private void Unsubscribe()
        {
            if (!_isSubscribed || _networkManager == null)
                return;

            _networkManager.ServerManager.OnServerConnectionState -= OnServerConnectionState;
            _networkManager.ClientManager.OnClientConnectionState -= OnClientConnectionState;
            _isSubscribed = false;
        }

        private void RefreshInputSubscription()
        {
#if ENABLE_INPUT_SYSTEM
            if (!isActiveAndEnabled || IsLinuxServerEnvironment)
            {
                UnsubscribeFromToggleAction();
                return;
            }

            if (_toggleHudAction == null || _toggleHudAction.action == null)
            {
                UnsubscribeFromToggleAction();
                return;
            }

            if (_inputSubscribed)
                return;

            if (!_toggleHudAction.action.enabled)
            {
                _toggleHudAction.action.Enable();
                _ownsEnabledToggleAction = true;
            }

            _toggleHudAction.action.performed += OnToggleHudPerformed;
            _inputSubscribed = true;
#endif
        }

        private void UnsubscribeFromToggleAction()
        {
#if ENABLE_INPUT_SYSTEM
            if (_toggleHudAction != null && _toggleHudAction.action != null && _inputSubscribed)
                _toggleHudAction.action.performed -= OnToggleHudPerformed;

            if (_toggleHudAction != null && _toggleHudAction.action != null && _ownsEnabledToggleAction)
                _toggleHudAction.action.Disable();

            _inputSubscribed = false;
            _ownsEnabledToggleAction = false;
#endif
        }

#if ENABLE_INPUT_SYSTEM
        private void OnToggleHudPerformed(InputAction.CallbackContext context)
        {
            _isVisible = !_isVisible;
        }
#endif

        private void OnServerConnectionState(ServerConnectionStateArgs args)
        {
            _serverState = args.ConnectionState;
        }

        private void OnClientConnectionState(ClientConnectionStateArgs args)
        {
            _clientState = args.ConnectionState;
        }

        private void ToggleServer()
        {
            if (!TryInitialize())
                return;

            if (_serverState == LocalConnectionState.Started)
            {
                _networkManager.ServerManager.StopConnection(true);
                return;
            }

            if (!TryGetPort(out ushort port))
                return;

            _port = port;
            _networkManager.ServerManager.StartConnection(port);
        }

        private void ToggleClient()
        {
            if (!TryInitialize())
                return;

            if (_clientState == LocalConnectionState.Started)
            {
                _networkManager.ClientManager.StopConnection();
                return;
            }

            if (!TryGetPort(out ushort port))
                return;

            _port = port;
            _networkManager.ClientManager.StartConnection(_address, port);
        }

        private void ToggleHost()
        {
            if (!TryInitialize())
                return;

            bool hostRunning = _serverState == LocalConnectionState.Started && _clientState == LocalConnectionState.Started;
            if (hostRunning)
            {
                _networkManager.ClientManager.StopConnection();
                _networkManager.ServerManager.StopConnection(true);
                return;
            }

            if (!TryGetPort(out ushort port))
                return;

            _port = port;
            _networkManager.ServerManager.StartConnection(port);
            _networkManager.ClientManager.StartConnection(_address, port);
        }

        private bool TryGetPort(out ushort port)
        {
            if (ushort.TryParse(_portText, out port))
                return true;

            Debug.LogWarning($"{nameof(RuntimeNetworkHud)} requires a valid port value.", this);
            return false;
        }

        private bool CanToggle(LocalConnectionState state)
        {
            return state == LocalConnectionState.Stopped || state == LocalConnectionState.Started;
        }

        private bool CanToggleHost()
        {
            bool serverReady = CanToggle(_serverState);
            bool clientReady = CanToggle(_clientState);
            return serverReady && clientReady;
        }

        private string GetToggleLabel(string connectionType, LocalConnectionState state)
        {
            if (state == LocalConnectionState.Started)
                return $"Stop {connectionType}";

            if (state == LocalConnectionState.Stopped)
                return $"Start {connectionType}";

            return $"{state} {connectionType}";
        }

        private string GetHostLabel()
        {
            bool hostRunning = _serverState == LocalConnectionState.Started && _clientState == LocalConnectionState.Started;
            if (hostRunning)
                return "Stop Host";

            if (_serverState == LocalConnectionState.Stopped && _clientState == LocalConnectionState.Stopped)
                return "Start Host";

            return "Host Unavailable";
        }

        private void DrawStatusRow(string label, LocalConnectionState state)
        {
            Color previousColor = GUI.contentColor;
            GUI.contentColor = GetStateColor(state);
            GUILayout.Label($"{label}: {state}");
            GUI.contentColor = previousColor;
        }

        private Color GetStateColor(LocalConnectionState state)
        {
            if (state == LocalConnectionState.Started)
                return _startedColor;

            if (state == LocalConnectionState.Stopped)
                return _stoppedColor;

            return _changingColor;
        }
    }
}