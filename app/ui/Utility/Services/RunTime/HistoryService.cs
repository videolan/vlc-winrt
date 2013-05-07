using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
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
        private readonly ManualResetEvent _fileReadEvent = new ManualResetEvent(false);
        private static readonly object HistoryFileLock = new object();

        public HistoryService()
        {
            lock (HistoryFileLock)
            {
                //only allow one instance to read file
                if (_histories == null)
                {
                    ThreadPool.RunAsync(GetHistory);

                    //Wait for our file to be read
                    _fileReadEvent.WaitOne(int.MaxValue);
                } 
            }
        }

        public void Add(IStorageItem file)
        {
            MediaHistory previouslySavedMedia = _histories.FirstOrDefault(h => h.Filename == file.Name);

            if (previouslySavedMedia == null)
            {
                string mru = StorageApplicationPermissions.MostRecentlyUsedList.Add(file);
                MediaHistory history = CreateHistory(file, mru);
                _histories.Insert(0, history);
            }
            else
            {
                _histories.Remove(previouslySavedMedia);
                _histories.Insert(0, previouslySavedMedia);
            }

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
                try
                {
                    StorageFile file = await StorageApplicationPermissions.MostRecentlyUsedList.GetFileAsync(history.Token);
                    return file;
                }
                catch (FileNotFoundException)
                {
                    return null;
                }
            }
            else
                return null;
        }

        private async void GetHistory(IAsyncAction operation)
        {
            try
            {
                StorageFile historyFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(HistoryFileName,
                                                                                                    CreationCollisionOption
                                                                                                        .OpenIfExists);

                using (IRandomAccessStreamWithContentType stream = await historyFile.OpenReadAsync())
                {
                    if (stream.Size > 0)
                    {
                        try
                        {
                            var serializer = new XmlSerializer(typeof (List<MediaHistory>));
                            XmlReader reader = XmlReader.Create(stream.AsStreamForRead());
                            _histories = (List<MediaHistory>) serializer.Deserialize(reader);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("Error retrieving history file");
                            Debug.WriteLine(ex.ToString());
                            _histories = new List<MediaHistory>();
                        }
                       
                    }
                    else
                    {
                        //if the file is empty must be a first run, create new history
                        _histories = new List<MediaHistory>();
                    }
                }
            }
            finally
            {
                _fileReadEvent.Set();
            }
        }

        public async void SaveHistory()
        {
            StorageFile historyFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(HistoryFileName,
                                                                                                CreationCollisionOption
                                                                                                    .ReplaceExisting);
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