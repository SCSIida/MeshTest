using UnityEngine.InputSystem;

namespace LocalMultiplayerUITest
{
    internal static class PlayerInputExtensions
    {
        public static bool IsPlayerUsingDevice(this PlayerInput playerInput, InputDevice device)
        {
            if (device == null || !device.added)
            {
                return false;
            }

            foreach (InputDevice d in playerInput.devices)
            {
                if (d == device)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
