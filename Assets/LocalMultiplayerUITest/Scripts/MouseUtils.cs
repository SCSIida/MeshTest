using UnityEngine.InputSystem;

internal static class MouseUtils
{
    public static Mouse FindSystemMouse()
    {
        Mouse systemMouse = null;

        foreach (InputDevice device in InputSystem.devices)
        {
            if (device.native && device is Mouse mouse)
            {
                systemMouse = mouse;
                break;
            }
        }
        return systemMouse;
    }
}
