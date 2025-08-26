using UnityEngine;
using UnityEngine.InputSystem;

namespace LocalMultiplayerUITest
{
    public class CursorUI : MonoBehaviour
    {
        [SerializeField]
        private PlayerInput _playerInput;
        [SerializeField]
        private string _actionName = "Point";

        [SerializeField]
        private RectTransform _cursorTransform;

        private InputAction _cursorPointAction;

        private void OnEnable()
        {
            _cursorPointAction = _playerInput.actions.FindAction(_actionName, true);
            _cursorPointAction.started += CursorPointActionCallback;
            _cursorPointAction.performed += CursorPointActionCallback;
            _cursorPointAction.canceled += CursorPointActionCallback;
        }

        private void OnDisable()
        {
            _cursorPointAction.started -= CursorPointActionCallback;
            _cursorPointAction.performed -= CursorPointActionCallback;
            _cursorPointAction.canceled -= CursorPointActionCallback;
            _cursorPointAction = null;
        }

        private void CursorPointActionCallback(InputAction.CallbackContext context)
        {
            _cursorTransform.anchoredPosition = context.ReadValue<Vector2>() / ((Vector2)_cursorTransform.parent.lossyScale);
        }

        public void SetCursorVisible(bool visible)
        {
            _cursorTransform.gameObject.SetActive(visible);
        }
    }
}