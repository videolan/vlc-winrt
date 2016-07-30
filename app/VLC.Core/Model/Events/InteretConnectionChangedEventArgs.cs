using System;

namespace VLC.Model.Events
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
