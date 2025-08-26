using UnityEngine;
using UnityEngine.InputSystem;

namespace LocalMultiplayerUITest
{
    public class Player : MonoBehaviour
    {
        [SerializeField]
        private PlayerInput _playerInput;
        [SerializeField]
        private Canvas _playerRootCanvas;

        public PlayerInput PlayerInput => _playerInput;
        public Canvas PlayerRootCanvas => _playerRootCanvas;
    }
}