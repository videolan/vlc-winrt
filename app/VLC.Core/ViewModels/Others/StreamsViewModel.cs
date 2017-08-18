using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Windows.UI.Xaml;
using Windows.UI.Core;

using VLC.Model.Stream;
using VLC.Utils;
using VLC.Services.RunTime;
using VLC.Commands.StreamsLibrary;
using VLC.Model.Library;

namespace VLC.ViewModels.Others
{
    public class StreamsViewModel : ViewModelBase
    {
        private MediaLibrary _mediaLibrary;
        private NetworkListenerService _networkListenerService;

        public StreamsViewModel(MediaLibrary mediaLibrary, NetworkListenerService networkListenerService)
        {
            _mediaLibrary = mediaLibrary;
            _networkListenerService = networkListenerService;
        }
        
        public PlayNetworkMRLCommand PlayStreamCommand { get; private set; } = new PlayNetworkMRLCommand();
        public IEnumerable<StreamMedia> StreamsHistoryAndFavoritesGrouped => _mediaLibrary.Streams?.OrderBy(x => x.Order);        
        public bool IsCollectionEmpty => !_mediaLibrary.Streams.Any();
        public Visibility NoInternetPlaceholderEnabled => _networkListenerService.IsConnected ? Visibility.Collapsed : Visibility.Visible;

        public override void Initialize()
        {
            _networkListenerService.InternetConnectionChanged += StreamsViewModel_InternetConnectionChanged;
            _mediaLibrary.Streams.CollectionChanged += Streams_CollectionChanged;

            Task.Run(async () => await _mediaLibrary.LoadStreamsFromDatabase());
        }

        public override void Stop()
        {
            _mediaLibrary.Streams.CollectionChanged -= Streams_CollectionChanged;
            _networkListenerService.InternetConnectionChanged -= StreamsViewModel_InternetConnectionChanged;

            _mediaLibrary = null;
            _networkListenerService = null;
            PlayStreamCommand = null;
        }
        
        private async void StreamsViewModel_InternetConnectionChanged(object sender, Model.Events.InternetConnectionChangedEventArgs e)
        {
            await DispatchHelper.InvokeInUIThread(CoreDispatcherPriority.Normal, () => OnPropertyChanged(nameof(NoInternetPlaceholderEnabled)));
        }

        private void Streams_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(StreamsHistoryAndFavoritesGrouped));
            OnPropertyChanged(nameof(IsCollectionEmpty));
        }
    }
}