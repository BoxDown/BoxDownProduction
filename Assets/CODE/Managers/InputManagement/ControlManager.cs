using UnityEngine;
using UnityEngine.InputSystem;

namespace Managers
{
    public class ControlManager : MonoBehaviour
    {
        public static ControlManager controlManager
        {
            get;
            private set;
        }
        public enum ControllerType
        {
            KeyboardMouse,
            Xbox,
            Playstation,
            Switch,
            Count
        }


        PlayerInput C_playerInputs;
        ControllerType e_currentControlDevice = ControllerType.KeyboardMouse;

        private void OnEnable()
        {
            C_playerInputs = FindObjectOfType<PlayerInput>();
            if (controlManager != null && controlManager != this)
            {
                Destroy(this);
            }
            else
            {
                controlManager = this;
            }
        }

        private static void EnableListeners()
        {
            InputSystem.onDeviceChange += controlManager.InputDeviceChanged;
        }

        private void InputDeviceChanged(InputDevice device, InputDeviceChange change)
        {
            switch (change)
            {
                case InputDeviceChange.Added:
                    string deviceType = device.description.manufacturer;
                    switch (deviceType)
                    {
                        //correct name
                        case "Sony Interactive Entertainement":
                            e_currentControlDevice = ControllerType.Playstation;
                            return;
                        //correct name
                        case "Nintendo Co., Ltd.":
                            e_currentControlDevice = ControllerType.Switch;
                            return;
                        default:
                            //xbox manufacturer returns empty but so do keyboard and mice
                            if (InputSystem.devices[0].description.product.Contains("XBox"))
                            {
                                e_currentControlDevice = ControllerType.Xbox;
                            }
                            else
                            {
                                e_currentControlDevice = ControllerType.KeyboardMouse;
                            }
                            return;
                    }

                case InputDeviceChange.Disconnected:
                    return;

                case InputDeviceChange.Reconnected:
                    return;
                default:

                    break;
            }
        }

        public void ResetDevices()
        {
            for (int i = 0; i < InputSystem.devices.Count - 1; i++)
            {
                InputSystem.RemoveDevice(InputSystem.devices[0]);
            }
        }
        public void CheckFirstInputDevice()
        {
            if (InputSystem.devices[0] == null)
                return;

            switch (InputSystem.devices[0].description.manufacturer)
            {
                //correct name
                case "Sony Interactive Entertainement":
                    e_currentControlDevice = ControllerType.Playstation;
                    return;
                //correct name
                case "Nintendo Co., Ltd.":
                    e_currentControlDevice = ControllerType.Switch;
                    return;
                default:
                    //xbox manufacturer returns empty but so do keyboard and mice
                    if (InputSystem.devices[0].description.product.Contains("XBox"))
                    {
                        e_currentControlDevice = ControllerType.Xbox;
                    }
                    else
                    {
                        e_currentControlDevice = ControllerType.KeyboardMouse;
                    }
                    return;
            }
        }
        public static ControllerType GetControllerType()
        {
            return controlManager.e_currentControlDevice;
        }

        public static void ChangeInputDevice(string controlScheme)
        {

            switch (controlScheme)
            {
                //correct name
                case "Playstation":
                    controlManager.e_currentControlDevice = ControllerType.Playstation;
                    return;
                //correct name
                case "Nintendo":
                    controlManager.e_currentControlDevice = ControllerType.Switch;
                    return;
                case "Xbox":
                    controlManager.e_currentControlDevice = ControllerType.Xbox;
                    return;
                case "KeyboardMouse":
                    controlManager.e_currentControlDevice = ControllerType.KeyboardMouse;
                    return;
            }
        }

        private void ChangeControlImages(ControllerType type)
        {

        }
    }
}
