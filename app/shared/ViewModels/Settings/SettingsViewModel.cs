/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System.Collections.Generic;
using System.Collections.ObjectModel;
#if WINDOWS_PHONE_APP
using VLC_WINPRT;
#endif
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using VLC_WINRT.Utility.Commands;
using VLC_WINRT.Utility.Helpers;

namespace VLC_WINRT.ViewModels.Settings
{
    public class SettingsViewModel : NavigateableViewModel
    {
        private ObservableCollection<CustomFolder> _musicFolders;
        private ObservableCollection<CustomFolder> _videoFolders;

        private AddCustomVideoFolder _addCustomVideoFolder;
        private AddCustomAudioFolder _addCustomAudioFolder;

        public ObservableCollection<CustomFolder> MusicFolders
        {
            get { return _musicFolders; }
            set { SetProperty(ref _musicFolders, value); }
        }

        public ObservableCollection<CustomFolder> VideoFolders
        {
            get { return _videoFolders; }
            set { SetProperty(ref _videoFolders, value); }
        }

        public AddCustomVideoFolder AddCustomVideoFolder
        {
            get { return _addCustomVideoFolder; }
            set { SetProperty(ref _addCustomVideoFolder, value); }
        }

        public AddCustomAudioFolder AddCustomAudioFolder
        {
            get { return _addCustomAudioFolder; }
            set { SetProperty(ref _addCustomAudioFolder, value); }
        }

        #region constructors
        public SettingsViewModel()
        {
            _musicFolders = new ObservableCollection<CustomFolder>();
            _videoFolders = new ObservableCollection<CustomFolder>();

            _addCustomAudioFolder = new AddCustomAudioFolder();
            _addCustomVideoFolder = new AddCustomVideoFolder();
        }
        #endregion

        public async Task PopulateCustomFolders()
        {
            if (ApplicationData.Current.LocalSettings.Containers.ContainsKey("customVideoFolders"))
            {
                foreach (var folder in ApplicationData.Current.LocalSettings.Containers["customVideoFolders"].Values)
                {
                    VideoFolders.Add(new CustomFolder()
                    {
                        DisplayName = folder.Key,
                        Mru = folder.Value.ToString(),
                    });
                }
            }
            else
            {
                VideoFolders.Add(new CustomFolder()
                {
                    DisplayName = "Video Library",
                    Mru = "Video Library",
                });
                ApplicationData.Current.LocalSettings.CreateContainer("customVideoFolders", ApplicationDataCreateDisposition.Always);
                ApplicationData.Current.LocalSettings.Containers["customVideoFolders"].Values.Add(new KeyValuePair<string, object>("Video Library", "Video Library"));
            }

            if (ApplicationData.Current.LocalSettings.Containers.ContainsKey("customAudioFolders"))
            {
                foreach (var folder in ApplicationData.Current.LocalSettings.Containers["customAudioFolders"].Values)
                {
                    MusicFolders.Add(new CustomFolder()
                    {
                        DisplayName = folder.Key,
                        Mru = folder.Value.ToString(),
                    });
                }
            }
            else
            {
                MusicFolders.Add(new CustomFolder()
                {
                    DisplayName = "Music Library",
                    Mru = "Music Library",
                });
                ApplicationData.Current.LocalSettings.CreateContainer("customAudioFolders", ApplicationDataCreateDisposition.Always);
                ApplicationData.Current.LocalSettings.Containers["customAudioFolders"].Values.Add(new KeyValuePair<string, object>("Music Library", "Music Library"));
            }
        }

        public void RemoveMusicFolder(CustomFolder folderToRemove)
        {
            MusicFolders.Remove(folderToRemove);
            ApplicationData.Current.LocalSettings.Containers["customAudioFolders"].Values.Remove(new KeyValuePair<string, object>(folderToRemove.DisplayName, folderToRemove.Mru));
        }
        public void RemoveVideoFolder(CustomFolder folderToRemove)
        {
            VideoFolders.Remove(folderToRemove);
            ApplicationData.Current.LocalSettings.Containers["customVideoFolders"].Values.Remove(new KeyValuePair<string, object>(folderToRemove.DisplayName, folderToRemove.Mru));
        }

        public void AddMusicFolder(CustomFolder folderToAdd)
        {
            MusicFolders.Add(folderToAdd);
            ApplicationData.Current.LocalSettings.Containers["customAudioFolders"].Values.Add(new KeyValuePair<string, object>(folderToAdd.DisplayName, folderToAdd.Mru));
        }
        public void AddVideoFolder(CustomFolder folderToAdd)
        {
            VideoFolders.Add(folderToAdd);
            ApplicationData.Current.LocalSettings.Containers["customVideoFolders"].Values.Add(new KeyValuePair<string, object>(folderToAdd.DisplayName, folderToAdd.Mru));
        }
    }

    public class CustomFolder
    {
        public string DisplayName { get; set; }
        public string Mru { get; set; }
    }
}
