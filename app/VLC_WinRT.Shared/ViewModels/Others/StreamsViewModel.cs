using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Core;
using VLC_WinRT.Database;
using VLC_WinRT.Model.Stream;
using VLC_WinRT.Utils;
using VLC_WinRT.Commands;

namespace VLC_WinRT.ViewModels.Others
{
    public class StreamsViewModel : BindableBase, IDisposable
    {
        public IEnumerable<IGrouping<string, StreamMedia>> StreamsHistoryAndFavoritesGrouped
        {
            get { return Locator.MediaLibrary.Streams?.GroupBy(x => x.Id.ToString()); }
        }

        public bool IsCollectionEmpty
        {
            get { return !Locator.MediaLibrary.Streams.Any(); }
        }

        public PlayNetworkMRLCommand PlayStreamCommand { get; } = new PlayNetworkMRLCommand();

        public void OnNavigatedTo()
        {
            Task.Run(() => Initialize());
        }

        public void OnNavigatedFrom()
        {
            Dispose();
        }

        public async Task Initialize()
        {
            Locator.MediaLibrary.Streams.CollectionChanged += Streams_CollectionChanged;
            await Locator.MediaLibrary.LoadStreamsFromDatabase();
        }

        private async void Streams_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Low, () =>
            {
                OnPropertyChanged(nameof(StreamsHistoryAndFavoritesGrouped));
                OnPropertyChanged(nameof(IsCollectionEmpty));
            });
        }

        public void Dispose()
        {
            Locator.MediaLibrary.Streams.CollectionChanged -= Streams_CollectionChanged;
        }
    }
}
