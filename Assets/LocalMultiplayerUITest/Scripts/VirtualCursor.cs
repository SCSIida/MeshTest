using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace LocalMultiplayerUITest
{
    public class VirtualCursor : MonoBehaviour
    {
        [SerializeField]
        private PlayerInput _playerInput;
        [SerializeField]
        private string _virtualCursorMoveActionName = "VirtualCursorMove";
        [SerializeField]
        private string _virtualCursorLeftClickActionName = "VirtualCursorLeftClick";

        [SerializeField]
        private float _cursorSpeed = 100f;

        private PlayerInput _assignedPlayerInput;
        private Mouse _virtualMouse;
        private InputAction _virtualCursorMoveAction;
        private InputAction _virtualCursorLeftClickAction;

        private Mouse _pairedMouse;

        private void OnEnable()
        {
            AddVirtualCursor(_playerInput);
        }

        private void OnDisable()
        {
            RemoveVirtualCursor();
        }

        private Mouse AddVirtualCursor(PlayerInput playerInput)
        {
            Mouse virtualMouse = (Mouse)InputSystem.AddDevice("VirtualMouse");
            foreach (InputDevice device in playerInput.devices)
            {
                if (device is Gamepad)
                {
                    playerInput.SwitchCurrentControlScheme(device, virtualMouse);
                    break;
                }
            }

            CachePairedMouse(playerInput);

            _virtualMouse = virtualMouse;
            _virtualCursorMoveAction = playerInput.actions.FindAction(_virtualCursorMoveActionName, true);
            _virtualCursorLeftClickAction = playerInput.actions.FindAction(_virtualCursorLeftClickActionName, true);
            _virtualCursorLeftClickAction.started += OnVirtualCursorLeftClickActionCallback;
            _virtualCursorLeftClickAction.canceled += OnVirtualCursorLeftClickActionCallback;
            _assignedPlayerInput = playerInput;
            _assignedPlayerInput.onControlsChanged += OnControlsChanged;

            InputSystem.onAfterUpdate += OnAfterUpdate;

            return virtualMouse;
        }

        private void RemoveVirtualCursor()
        {
            InputSystem.onAfterUpdate -= OnAfterUpdate;

            InputSystem.RemoveDevice(_virtualMouse);

            _virtualMouse = null;
            _virtualCursorMoveAction = null;
            _virtualCursorLeftClickAction.started -= OnVirtualCursorLeftClickActionCallback;
            _virtualCursorLeftClickAction.canceled -= OnVirtualCursorLeftClickActionCallback;
            _virtualCursorLeftClickAction = null;
            _assignedPlayerInput.onControlsChanged -= OnControlsChanged;
            _assignedPlayerInput = null;
        }

        private void OnAfterUpdate()
        {
            Vector2 currentPosition = _virtualMouse.position.ReadValue();
            Vector2 delta = _virtualCursorMoveAction.ReadValue<Vector2>() * _cursorSpeed;
            Vector2 newPosition = currentPosition + delta;
            //Debug.Log($"{nameof(OnAfterUpdate)}: Current = {currentPosition}, Delta = {delta}, New = {newPosition}");
            InputState.Change(_virtualMouse.position, newPosition);
        }

        private void OnVirtualCursorLeftClickActionCallback(InputAction.CallbackContext context)
        {
            _virtualMouse.CopyState(out MouseState mouseState);
            mouseState.WithButton(MouseButton.Left, context.control.IsPressed());
            InputState.Change(_virtualMouse, mouseState);
        }

        /// <summary>
        /// Called when the control scheme or devices change for the PlayerInput.
        /// </summary>
        /// <remarks>
        /// This method will be triggered automatically when <see cref="PlayerInput.notificationBehavior"/> is set to <see cref="PlayerNotifications.SendMessages"/>.
        /// </remarks>
        /// <param name="playerInput"></param>
        private void OnControlsChanged(PlayerInput playerInput)
        {
            Mouse systemMouse = MouseUtils.FindSystemMouse();
            if (playerInput.IsPlayerUsingDevice(systemMouse))
            {
                // Move the system cursor to the virtual cursor position.
                systemMouse.WarpCursorPosition(_pairedMouse.position.ReadValue());
            }
            else
            if (playerInput.IsPlayerUsingDevice(_virtualMouse))
            {
                // Move the virtual cursor to the current software cursor position.
                InputState.Change(_virtualMouse.position, _pairedMouse.position.ReadValue());
            }

            CachePairedMouse(playerInput);
        }

        private void CachePairedMouse(PlayerInput playerInput)
        {
            foreach (InputDevice device in playerInput.devices)
            {
                if (device is Mouse)
                {
                    _pairedMouse = device as Mouse;
                    break;
                }
            }
        }
    }
}
