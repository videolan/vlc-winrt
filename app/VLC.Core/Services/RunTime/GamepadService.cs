using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VLC.ViewModels;
using Windows.Gaming.Input;

namespace VLC.Services.RunTime
{
    public class GamepadService : IDisposable
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
        
        public void Dispose()
        {
            Gamepad.GamepadAdded -= Gamepad_GamepadUpdated;
            Gamepad.GamepadRemoved -= Gamepad_GamepadUpdated;
        }
    }
}
