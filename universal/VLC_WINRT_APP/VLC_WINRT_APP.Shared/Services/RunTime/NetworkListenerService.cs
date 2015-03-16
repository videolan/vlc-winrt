using System;
using Windows.Networking.Connectivity;
using VLC_WINRT_APP.Model.Events;

namespace VLC_WINRT_APP.Services.RunTime
{
    class NetworkListenerService
    {
        public event EventHandler<InternetConnectionChangedEventArgs> InternetConnectionChanged;

        public NetworkListenerService()
        {
            NetworkInformation.NetworkStatusChanged += NetworkInformation_NetworkStatusChanged;
        }

        void NetworkInformation_NetworkStatusChanged(object sender)
        {
            if (InternetConnectionChanged == null) return;
            var arg = new InternetConnectionChangedEventArgs(IsConnected);
            InternetConnectionChanged(null, arg);
        }

        public static bool IsConnected
        {
            get
            {
                var profile = NetworkInformation.GetInternetConnectionProfile();
                var isConnected = (profile != null && profile.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess);
                return isConnected;
            }
        }
    }
}
