using System;
using Windows.Networking.Connectivity;
using VLC.Model.Events;

namespace VLC.Services.RunTime
{
    class NetworkListenerService
    {
        public event EventHandler<InternetConnectionChangedEventArgs> InternetConnectionChanged;

        public NetworkListenerService()
        {
            NetworkInformation.NetworkStatusChanged += NetworkInformation_NetworkStatusChanged;
            NotifyNetworkChanged();
        }

        void NetworkInformation_NetworkStatusChanged(object sender)
        {
            NotifyNetworkChanged();
        }

        void NotifyNetworkChanged()
        {
            if (InternetConnectionChanged == null)
                return;
            var arg = new InternetConnectionChangedEventArgs(IsConnected);
            InternetConnectionChanged(null, arg);
        }
        
        public static bool IsConnected
        {
            get
            {
                var profile = NetworkInformation.GetInternetConnectionProfile();
                return (profile != null && profile.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess);
            }
        }
    }
}
