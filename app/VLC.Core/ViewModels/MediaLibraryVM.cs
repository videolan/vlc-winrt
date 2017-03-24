using VLC.Model;
using VLC.Utils;
using Windows.Networking;
using Windows.Networking.Connectivity;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace VLC.ViewModels
{
    public class MediaLibraryVM : BindableBase
    {
        public MediaLibraryVM()
        {
            Locator.MediaLibrary.OnIndexing += MediaLibrary_OnIndexing;
        }

        ~MediaLibraryVM()
        {
            Locator.MediaLibrary.OnIndexing -= MediaLibrary_OnIndexing;
        }

        private async void MediaLibrary_OnIndexing(LoadingState obj)
        {
            await DispatchHelper.InvokeInUIThread(CoreDispatcherPriority.Normal, () => OnPropertyChanged(nameof(IndexingLibraryVisibility)));
        }

        public Visibility IndexingLibraryVisibility
        {
            get { return Locator.MediaLibrary.MediaLibraryIndexingState == LoadingState.Loading ? Visibility.Visible : Visibility.Collapsed; }
        }

        public string AddMediaHelpString
        {
            get
            {
                foreach (HostName localHostName in NetworkInformation.GetHostNames())
                {
                    if (localHostName.IPInformation != null)
                    {
                        if (localHostName.Type == HostNameType.Ipv4)
                        {
                            var str = string.Format(Strings.AddMediaHelpWithIP, localHostName.ToString());
                            return str;
                        }
                    }
                }
                return string.Format(Strings.AddMediaHelp);
            }
        }
    }
}
