using System;

namespace VLC_WINRT_APP.Model.Events
{
    public class InternetConnectionChangedEventArgs : EventArgs
    {
        public InternetConnectionChangedEventArgs(bool isConnected)
        {
            this.isConnected = isConnected;
        }

        private bool isConnected;
        public bool IsConnected
        {
            get { return isConnected; }
        }
    }
}
