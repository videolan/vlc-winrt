using VLC.Model;
using VLC.Utils;
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
            await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () => OnPropertyChanged(nameof(IndexingLibraryVisibility)));
        }

        public Visibility IndexingLibraryVisibility
        {
            get { return Locator.MediaLibrary.MediaLibraryIndexingState == LoadingState.Loading ? Visibility.Visible : Visibility.Collapsed; }
        }
    }
}
