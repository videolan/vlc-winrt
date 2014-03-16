/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Streams;
using Windows.System.Threading;
using VLC_WINRT.Model;

namespace VLC_WINRT.Utility.Services.RunTime
{
    public class HistoryService
    {
        private const string HistoryFileName = "histories.xml";
        private static List<MediaHistory> _histories;
        private static readonly object HistoryFileLock = new object();
        public event EventHandler HistoryUpdated;
        private bool _computing;

        public HistoryService()
        {
        }

        public void Dispose()
        {
        }

        public Task Clear()
        {
            StorageApplicationPermissions.MostRecentlyUsedList.Clear();
            _histories = new List<MediaHistory>();
            return SaveHistory();
        }

        public async Task<string> Add(StorageFile file)
        {
            bool isAudio = file.ContentType.Contains("audio");

            string token;
            MediaHistory previouslySavedMedia = _histories.FirstOrDefault(h => h.Filename == file.Name);

            if (previouslySavedMedia == null)
            {
                string mru = StorageApplicationPermissions.FutureAccessList.Add(file);
                MediaHistory history = CreateHistory(file, mru, isAudio);
                _histories.Insert(0, history);
                token = history.Token;
            }
            else
            {
                _histories.Remove(previouslySavedMedia);
                _histories.Insert(0, previouslySavedMedia);
                token = previouslySavedMedia.Token;
            }

            await SaveHistory();
            return token;
        }

        public int FileCount()
        {
            return StorageApplicationPermissions.FutureAccessList.Entries.Count();
        }

        public async Task<StorageFile> RetrieveFileAt(int index)
        {
            if (index < _histories.Count)
            {
                MediaHistory history = _histories[index];
                try
                {
                    StorageFile file = await StorageApplicationPermissions.FutureAccessList.GetFileAsync(history.Token);
                    return file;
                }
                catch (FileNotFoundException)
                {
                    return null;
                }
            }
            return null;
        }

        public MediaHistory GetHistory(string token)
        {
            return _histories.FirstOrDefault(h => h.Token == token);
        }

        public string GetTokenAtPosition(int index)
        {
            return index < _histories.Count ? _histories[index].Token : null;
        }
        public bool IsAudioAtPosition(int index)
        {
            return index < _histories.Count ? _histories[index].IsAudio : false;
        }
        public IAsyncOperation<StorageFile> RetrieveFile(string token)
        {
            if (string.IsNullOrEmpty(token)) return null;
            return StorageApplicationPermissions.FutureAccessList.GetFileAsync(token);
        }

        public async Task RestoreHistory()
        {
            lock (HistoryFileLock)
            {
                //only allow one instance to read file
                if (_histories != null || _computing == true)
                    return;
                _computing = true;
            }
            try
            {
                StorageFile historyFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(HistoryFileName,
                    CreationCollisionOption.OpenIfExists);

                using (IRandomAccessStreamWithContentType stream = await historyFile.OpenReadAsync())
                {
                    if (stream.Size > 0)
                    {
                        try
                        {
                            var serializer = new XmlSerializer(typeof(List<MediaHistory>));
                            XmlReader reader = XmlReader.Create(stream.AsStreamForRead());
                            _histories = (List<MediaHistory>)serializer.Deserialize(reader);
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
                _computing = false;
            }
        }

        public async Task SaveHistory()
        {
            StorageFile historyFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(HistoryFileName,
                CreationCollisionOption.ReplaceExisting);
            using (Stream stream = await historyFile.OpenStreamForWriteAsync())
            {
                var serializer = new XmlSerializer(_histories.GetType());

                //only save 50 most recent files for performance reasons
                _histories = _histories.Take(50).ToList();
                serializer.Serialize(stream, _histories);
            }

            PublishUpdate();
        }

        private MediaHistory CreateHistory(IStorageItem item, string token, bool isAudio)
        {
            var history = new MediaHistory
            {
                Token = token,
                Filename = item.Name,
                LastPlayed = DateTime.Now,
                TotalWatchedMilliseconds = 0,
                IsAudio = isAudio
            };

            return history;
        }

        public void UpdateMediaHistory(string fileToken, TimeSpan totalWatched)
        {
            MediaHistory mediaHistory = _histories.FirstOrDefault(h => h.Token == fileToken);
            if (mediaHistory != null)
                mediaHistory.TotalWatchedMilliseconds = totalWatched.TotalMilliseconds;
            PublishUpdate();
        }

        private void PublishUpdate()
        {
            if (HistoryUpdated != null)
            {
                HistoryUpdated(this, new EventArgs());
            }
        }
    }
}
