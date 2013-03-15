using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using VLC_WINRT.Model;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Streams;
using Windows.System.Threading;

namespace VLC_WINRT.Utility.Services.RunTime
{
    public class HistoryService
    {
        private const string HistoryFileName = "histories.xml";
        private static List<MediaHistory> _histories;

        public HistoryService()
        {
            if (_histories == null)
            {
                ThreadPool.RunAsync(GetHistory);
            }
        }

        public void Add(IStorageItem file)
        {
            string mru = StorageApplicationPermissions.MostRecentlyUsedList.Add(file);
            MediaHistory history = CreateHistory(file, mru);
            _histories.Insert(0, history);
            SaveHistory();
        }

        public void Clear()
        {
            StorageApplicationPermissions.MostRecentlyUsedList.Clear();
            _histories = new List<MediaHistory>();
            SaveHistory();
        }

        public int FileCount()
        {
            return StorageApplicationPermissions.MostRecentlyUsedList.Entries.Count();
        }

        public async Task<StorageFile> RetrieveFileAt(int index)
        {
            if (index < _histories.Count)
            {
                MediaHistory history = _histories[index];
                StorageFile file = await StorageApplicationPermissions.MostRecentlyUsedList.GetFileAsync(history.Token);
                return file;
            }
            else
                return null;
        }

        private async void GetHistory(IAsyncAction operation)
        {
            StorageFile historyFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(HistoryFileName,
                                                                                                CreationCollisionOption
                                                                                                    .OpenIfExists);

            using (IRandomAccessStreamWithContentType stream = await historyFile.OpenReadAsync())
            {
                if (stream.Size > 0)
                {
                    var serializer = new XmlSerializer(typeof(List<MediaHistory>));
                    XmlReader reader = XmlReader.Create(stream.AsStreamForRead());
                    _histories = (List<MediaHistory>)serializer.Deserialize(reader);
                }
                else
                {
                    //if the file is empty must be a first run, create new history
                    _histories = new List<MediaHistory>();
                }
            }
        }

        public async void SaveHistory()
        {
            StorageFile historyFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(HistoryFileName,
                                                                                                CreationCollisionOption
                                                                                                    .OpenIfExists);
            using (Stream stream = await historyFile.OpenStreamForWriteAsync())
            {
                var serializer = new XmlSerializer(_histories.GetType());

                //only save 50 most recent files for performance reasons
                _histories = _histories.Take(50).ToList();
                serializer.Serialize(stream, _histories);
            }
        }

        private MediaHistory CreateHistory(IStorageItem item, string mru)
        {
            var history = new MediaHistory
                              {
                                  Token = mru,
                                  Filename = item.Name,
                                  LastPlayed = DateTime.Now
                              };

            return history;
        }
    }
}