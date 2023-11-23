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
        float f_currentLeftVibration = 0;
        float f_currentRightVibration = 0;
        float f_currentVibrationTime = 0;

        private void Awake()
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

        private void Update()
        {
            if (f_currentVibrationTime <= 0)
            {
                f_currentVibrationTime = 0;
                StopControllerVibration();
                return;
            }
            f_currentVibrationTime -= Time.deltaTime;
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
                            Cursor.visible = false;

                            return;
                        //correct name
                        case "Nintendo Co., Ltd.":
                            e_currentControlDevice = ControllerType.Switch;
                            Cursor.visible = false;

                            return;
                        default:
                            //xbox manufacturer returns empty but so do keyboard and mice
                            if (InputSystem.devices[0].description.product.Contains("XBox"))
                            {
                                e_currentControlDevice = ControllerType.Xbox;
                                Cursor.visible = false;
                            }
                            else
                            {
                                e_currentControlDevice = ControllerType.KeyboardMouse;
                                Cursor.visible = true;
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

        public static void VibrateController(float leftMotorSpeed, float rightMotorSpeed, float time)
        {
            switch (controlManager.e_currentControlDevice)
            {
                case ControllerType.KeyboardMouse:
                    return;
                default:
                    if (controlManager.f_currentLeftVibration <= leftMotorSpeed)
                    {
                        controlManager.f_currentLeftVibration = leftMotorSpeed;
                    }
                    if (controlManager.f_currentRightVibration <= rightMotorSpeed)
                    {
                        controlManager.f_currentRightVibration = rightMotorSpeed;
                    }
                    if (controlManager.f_currentVibrationTime < time)
                    {
                        controlManager.f_currentVibrationTime = time;
                    }
                    Gamepad.current.SetMotorSpeeds(Mathf.Clamp01(controlManager.f_currentLeftVibration), Mathf.Clamp01(controlManager.f_currentRightVibration));                    
                    break;
            }
        }
        public void StopControllerVibration()
        {
            InputSystem.ResetHaptics();
            f_currentLeftVibration = 0;
            f_currentRightVibration = 0;
        }
    }
}
