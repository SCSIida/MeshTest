using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

namespace LocalMultiplayerUITest
{
    public class PlayerManager : MonoBehaviour
    {
        [SerializeField]
        private InputAction _togglePlayerManagementInputAction;
        [SerializeField]
        private InputAction _joinInputAction;
        [SerializeField]
        private GameObject _playerPrefab;

        [SerializeField]
        private GameObject _publicUIRoot;

        private bool _isPlayerManagementActive = false;

        private List<Player> _players = new();

        private void OnEnable()
        {
            _togglePlayerManagementInputAction.Enable();
            //_togglePlayerManagementInputAction.started += OnTogglePlayerManagementCallback;
            _togglePlayerManagementInputAction.performed += OnTogglePlayerManagementCallback;
            //_togglePlayerManagementInputAction.canceled += OnTogglePlayerManagementCallback;

            TogglePlayerManagement();
        }

        private void OnDisable()
        {
            _togglePlayerManagementInputAction.Disable();
            //_togglePlayerManagementInputAction.started -= OnTogglePlayerManagementCallback;
            _togglePlayerManagementInputAction.performed -= OnTogglePlayerManagementCallback;
            //_togglePlayerManagementInputAction.canceled -= OnTogglePlayerManagementCallback;
        }

        private void TogglePlayerManagement()
        {
            _isPlayerManagementActive = !_isPlayerManagementActive;

            if (_isPlayerManagementActive)
            {
                _joinInputAction.Enable();
                //_joinInputAction.started += OnJoinCallback;
                _joinInputAction.performed += OnJoinCallback;
                //_joinInputAction.canceled += OnJoinCallback;

                while (_players.Count > 0)
                {
                    int index = _players.Count - 1;
                    Player player = _players[index];
                    player.PlayerInput.onControlsChanged -= OnControlsChanged;
                    Destroy(player.gameObject);
                    _players.RemoveAt(index);
                }
            }
            else
            {
                _joinInputAction.Disable();
                //_joinInputAction.started -= OnJoinCallback;
                _joinInputAction.performed -= OnJoinCallback;
                //_joinInputAction.canceled -= OnJoinCallback;

                if (_players.Count == 1)
                {
                    _players[0].PlayerInput.neverAutoSwitchControlSchemes = false;
                    _players[0].PlayerInput.onControlsChanged += OnControlsChanged;
                    Cursor.visible = _players[0].PlayerInput.IsPlayerUsingDevice(MouseUtils.FindSystemMouse());
                }
                foreach (Player player in _players)
                {
                    player.PlayerInput.ActivateInput();
                    player.GetComponent<CursorUI>()?.SetCursorVisible(true);
                }
            }
            Debug.Log($"{nameof(TogglePlayerManagement)}: {_isPlayerManagementActive}");
        }

        private void OnTogglePlayerManagementCallback(InputAction.CallbackContext context)
        {
            Debug.Log($"{nameof(OnTogglePlayerManagementCallback)}: {context.phase}: ");
            if (context.ReadValueAsButton())
            {
                TogglePlayerManagement();
            }
        }

        private void OnJoinCallback(InputAction.CallbackContext context)
        {
            Debug.Log($"{nameof(OnJoinCallback)}: {context.phase}: ");
            if (context.ReadValueAsButton())
            {
                if (_players.Exists((p) => p.PlayerInput.devices.Contains(context.control.device)))
                {
                    Debug.Log($"Device {context.control.device} is already paired to a player");
                    return;
                }

                PlayerInput playerInput = SpawnPlayer(_playerPrefab, context.control.device);
                _players.Add(playerInput.GetComponent<Player>());

                for (int i = 0; i < _players.Count; i++)
                {
                    Player player = _players[i];
                    player.PlayerRootCanvas.GetComponent<RectTransform>().anchoredPosition = new Vector2((i - ((_players.Count - 1) / 2f)) * (Screen.width / (_players.Count + 1f)), 0);
                }
            }
        }

        private PlayerInput SpawnPlayer(GameObject prefab, InputDevice device)
        {
            Debug.Log($"{nameof(SpawnPlayer)}: Spawn player for device({device})");
            PlayerInput playerInput = PlayerInput.Instantiate(prefab, pairWithDevice: device);
            playerInput.neverAutoSwitchControlSchemes = true;
            playerInput.DeactivateInput();
            playerInput.GetComponent<CursorUI>()?.SetCursorVisible(false);
            if (playerInput.uiInputModule is InputSystemUIIgnorablePointerInputModule inputModule)
            {
                inputModule.dontIgnoreChildren = _publicUIRoot;
            }
            return playerInput;
        }

        private void OnControlsChanged(PlayerInput playerInput)
        {
            Cursor.visible = playerInput.IsPlayerUsingDevice(MouseUtils.FindSystemMouse());
        }
    }
}
