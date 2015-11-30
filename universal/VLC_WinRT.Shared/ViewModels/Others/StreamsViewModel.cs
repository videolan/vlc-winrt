using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Core;
using VLC_WinRT.Database;
using VLC_WinRT.Model.Stream;
using VLC_WinRT.Utils;

namespace VLC_WinRT.ViewModels.Others
{
    public class StreamsViewModel : BindableBase, IDisposable
    {
        private List<StreamMedia> streams;
        public StreamsDatabase StreamsDatabase = new StreamsDatabase();
        public IEnumerable<IGrouping<string, StreamMedia>> StreamsHistoryAndFavoritesGrouped
        {
            get { return streams?.GroupBy(x => x.Id.ToString()); }
        }

        public void OnNavigatedTo()
        {
            Task.Run(() => Initialize());
        }

        public async Task Initialize()
        {
            streams = await StreamsDatabase.Load();
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {
                OnPropertyChanged(nameof(StreamsHistoryAndFavoritesGrouped));
            });
        }

        public void Dispose()
        {
            streams?.Clear();
            streams = null;
        }
    }
}
