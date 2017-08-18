using System;
using Windows.Networking.Connectivity;
using VLC.Model.Events;

namespace VLC.Services.RunTime
{
    public class NetworkListenerService
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

        public bool IsConnected => NetworkInformation.GetInternetConnectionProfile()?.GetNetworkConnectivityLevel() ==
                                    NetworkConnectivityLevel.InternetAccess;
    }
}
