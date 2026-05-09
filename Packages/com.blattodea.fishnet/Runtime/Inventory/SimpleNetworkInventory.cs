using System;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

#if UNITY_INPUT_SYSTEM_ENABLED
using UnityEngine.InputSystem;
#endif

namespace Blattodea.FishNet.Inventory
{
    [DisallowMultipleComponent]
    public class SimpleNetworkInventory : NetworkBehaviour
    {
        [Header("Inventory")]
        [SerializeField] private GameObject[] slotObjects = Array.Empty<GameObject>();
        [SerializeField] private int defaultSelectedSlot = -1;

#if UNITY_INPUT_SYSTEM_ENABLED
        [Header("Input")]
        [SerializeField] private InputActionReference selectSlotInputAction;

        private bool _inputSubscriptionActive;
#endif

        private readonly SyncVar<int> _selectedSlot = new();

        public int SelectedSlot => _selectedSlot.Value;

        protected virtual void Awake()
        {
            _selectedSlot.SetInitialValues(defaultSelectedSlot);
            _selectedSlot.UpdateSendRate(0f);
            _selectedSlot.OnChange += OnSelectedSlotChanged;
            ApplySelectedSlot(_selectedSlot.Value);
        }

        protected virtual void OnEnable()
        {
#if UNITY_INPUT_SYSTEM_ENABLED
            RefreshInputSubscription();
#endif
        }

        protected virtual void OnDisable()
        {
#if UNITY_INPUT_SYSTEM_ENABLED
            DisableInputSubscription();
#endif
        }

        protected virtual void OnDestroy()
        {
            _selectedSlot.OnChange -= OnSelectedSlotChanged;

#if UNITY_INPUT_SYSTEM_ENABLED
            DisableInputSubscription();
#endif
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
#if UNITY_INPUT_SYSTEM_ENABLED
            RefreshInputSubscription();
#endif
            ApplySelectedSlot(_selectedSlot.Value);
        }

        public override void OnStopClient()
        {
#if UNITY_INPUT_SYSTEM_ENABLED
            DisableInputSubscription();
#endif
            base.OnStopClient();
        }

        public override void OnOwnershipClient(NetworkConnection prevOwner)
        {
            base.OnOwnershipClient(prevOwner);

#if UNITY_INPUT_SYSTEM_ENABLED
            RefreshInputSubscription();
#endif
        }

        public virtual void RequestSelectedSlot(int slotIndex)
        {
            if (!IsOwner)
            {
                return;
            }

            ServerSetSelectedSlot(slotIndex);
        }

        [ServerRpc(RequireOwnership = false)]
        protected virtual void ServerSetSelectedSlot(int slotIndex, NetworkConnection caller = null)
        {
            if (Owner.IsValid && caller != Owner)
            {
                return;
            }

            if (!IsValidSlotIndex(slotIndex))
            {
                return;
            }

            _selectedSlot.Value = slotIndex;
        }

        protected virtual void OnSelectedSlotChanged(int previousSlot, int nextSlot, bool asServer)
        {
            ApplySelectedSlot(nextSlot);
        }

        protected virtual void ApplySelectedSlot(int slotIndex)
        {
            if (slotObjects == null)
            {
                return;
            }

            for (int index = 0; index < slotObjects.Length; index++)
            {
                GameObject slotObject = slotObjects[index];
                if (slotObject == null)
                {
                    continue;
                }

                slotObject.SetActive(index == slotIndex);
            }
        }

        protected virtual bool IsValidSlotIndex(int slotIndex)
        {
            if (slotIndex < 0)
            {
                return slotIndex == -1;
            }

            return slotIndex < slotObjects.Length;
        }

#if UNITY_INPUT_SYSTEM_ENABLED
        protected virtual void RefreshInputSubscription()
        {
            if (!IsClientInitialized || !isActiveAndEnabled || !IsOwner)
            {
                DisableInputSubscription();
                return;
            }

            if (_inputSubscriptionActive || selectSlotInputAction == null || selectSlotInputAction.action == null)
            {
                return;
            }

            selectSlotInputAction.action.Enable();
            selectSlotInputAction.action.performed += OnSelectSlotPerformed;
            _inputSubscriptionActive = true;
        }

        protected virtual void DisableInputSubscription()
        {
            if (!_inputSubscriptionActive || selectSlotInputAction == null || selectSlotInputAction.action == null)
            {
                return;
            }

            selectSlotInputAction.action.performed -= OnSelectSlotPerformed;
            _inputSubscriptionActive = false;
        }

        protected virtual void OnSelectSlotPerformed(InputAction.CallbackContext context)
        {
            RequestSelectedSlot(ReadSlotIndex(context));
        }

        protected virtual int ReadSlotIndex(InputAction.CallbackContext context)
        {
            Type valueType = context.valueType;
            if (valueType == typeof(int))
            {
                return context.ReadValue<int>();
            }

            if (valueType == typeof(float))
            {
                return Mathf.RoundToInt(context.ReadValue<float>());
            }

            return Convert.ToInt32(context.ReadValueAsObject());
        }
#endif
    }
}