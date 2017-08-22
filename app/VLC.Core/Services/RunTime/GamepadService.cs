using System;
using Windows.Gaming.Input;

namespace VLC.Services.RunTime
{
    public class GamepadService
    {
        public event EventHandler<Gamepad> GamepadUpdated;
        
        public void StartListening()
        {
            Gamepad.GamepadAdded += Gamepad_GamepadUpdated;
            Gamepad.GamepadRemoved += Gamepad_GamepadUpdated;
        }
        
        private void Gamepad_GamepadUpdated(object sender, Gamepad e)
        {
            GamepadUpdated?.Invoke(this, e);
        }

        public void StopListening()
        {
            Gamepad.GamepadAdded -= Gamepad_GamepadUpdated;
            Gamepad.GamepadRemoved -= Gamepad_GamepadUpdated;
        }
    }
}
