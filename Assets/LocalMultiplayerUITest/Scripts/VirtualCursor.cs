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
        private Mouse _mouse;
        private InputAction _virtualCursorMoveAction;
        private InputAction _virtualCursorLeftClickAction;

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

            _mouse = virtualMouse;
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

            InputSystem.RemoveDevice(_mouse);

            _mouse = null;
            _virtualCursorMoveAction = null;
            _virtualCursorLeftClickAction.started -= OnVirtualCursorLeftClickActionCallback;
            _virtualCursorLeftClickAction.canceled -= OnVirtualCursorLeftClickActionCallback;
            _virtualCursorLeftClickAction = null;
            _assignedPlayerInput.onControlsChanged -= OnControlsChanged;
            _assignedPlayerInput = null;
        }

        private void OnAfterUpdate()
        {
            Vector2 currentPosition = _mouse.position.ReadValue();
            Vector2 delta = _virtualCursorMoveAction.ReadValue<Vector2>() * _cursorSpeed;
            Vector2 newPosition = currentPosition + delta;
            //Debug.Log($"{nameof(OnAfterUpdate)}: Current = {currentPosition}, Delta = {delta}, New = {newPosition}");
            InputState.Change(_mouse.position, newPosition);
        }

        private void OnVirtualCursorLeftClickActionCallback(InputAction.CallbackContext context)
        {
            _mouse.CopyState(out MouseState mouseState);
            mouseState.WithButton(MouseButton.Left, context.control.IsPressed());
            InputState.Change(_mouse, mouseState);
        }

        /// <summary>
        /// Called when the control scheme or devices change for the PlayerInput.
        /// </summary>
        /// <param name="playerInput"></param>
        private void OnControlsChanged(PlayerInput playerInput)
        {
            string controls = "";
            InputAction action = playerInput.actions.FindAction("Point");
            for(var i = 0; i < action.controls.Count; i++)
            {
                controls += $"{{{i}: {action.controls[i].device?.name}}}, ";
            }
            Debug.Log($"{nameof(VirtualCursor)}: {nameof(OnControlsChanged)}: {playerInput.currentControlScheme}: {(playerInput.devices.Count > 0 ? playerInput.devices[0].name : "")} ({playerInput.devices.Count}), Controls = {controls}, Point.ActiveControl = {action.activeControl?.device?.name}, Mouse.crrent = {Mouse.current?.name}, VirtualMouse = {_mouse?.position.ReadValue()}");
            Cursor.visible = playerInput.currentControlScheme == "Keyboard&Mouse";
            if (playerInput.currentControlScheme == "Keyboard&Mouse")
            {
                //Cursor.SetCursor(null, _mouse.position.ReadValue(), CursorMode.Auto);
                if (_mouse?.added ?? false)
                {
                    Mouse.current.WarpCursorPosition(_mouse.position.ReadValue());
                }
            }
            else
            // TODO: Can we check if _mouse device is used or not?
            if (playerInput.currentControlScheme == "Gamepad")
            {
                if (_mouse?.added ?? false)
                {
                    InputState.Change(_mouse.position, Mouse.current.position.ReadValue());
                    //InputState.Change(_mouse.position, playerInput.actions.FindAction("Point").ReadValue<Vector2>());
                }
            }
            // TODO: Virtual cursor stop moving after switching from keyboard/mouse to gamepad.

        }
    }
}
