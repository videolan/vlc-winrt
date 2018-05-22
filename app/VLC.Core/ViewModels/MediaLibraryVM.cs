using System.Linq;
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

        public string AddMediaHelpString => string.IsNullOrEmpty(Locator.FileCopyService.XboxIp)
            ? Strings.AddMediaHelp
            : string.Format(Strings.AddMediaHelpWithIP, Locator.FileCopyService.XboxIp);
    }
}
