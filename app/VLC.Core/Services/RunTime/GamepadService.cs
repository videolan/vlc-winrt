using System;
using Windows.Gaming.Input;

using VLC.Helpers;

namespace VLC.Services.RunTime
{
    public class GamepadService
    {
        private const string GamepadAdded = "Gamedpad Added";
        private const string GamepadRemoved = "Gamedpad Removed";
        // Workaround to prevent event firing on app start which makes the notification annoying and unnecessary.
        private bool _appStartEvent;

        public void StartListening()
        {
            _appStartEvent = true;
            Gamepad.GamepadAdded += Gamepad_Added;
            Gamepad.GamepadRemoved += Gamepad_Removed;
        }

        public void StopListening()
        {
            Gamepad.GamepadAdded -= Gamepad_Added;
            Gamepad.GamepadRemoved -= Gamepad_Removed;
        }

        private void Gamepad_Added(object sender, Gamepad e)
        {
            if (_appStartEvent)
            {
                _appStartEvent = false;
                return;
            }

            Notify(GamepadAdded);   
        }

        private void Gamepad_Removed(object sender, Gamepad e)
        {
            Notify(GamepadRemoved);
        }

        private void Notify(string gamepadState)
        {
            ToastHelper.Basic(gamepadState);
        }
    }
}