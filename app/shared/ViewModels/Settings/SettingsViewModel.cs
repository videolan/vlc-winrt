/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System.Collections.ObjectModel;
#if WINDOWS_PHONE_APP
using VLC_WINPRT;
#endif
using Windows.Storage;
using Windows.UI.Xaml;
using VLC_WINRT.Utility.Helpers;

namespace VLC_WINRT.ViewModels.Settings
{
    public class SettingsViewModel : NavigateableViewModel
    {
        private ObservableCollection<CustomFolder> _musicFolders;
        private ObservableCollection<CustomFolder> _videoFolders;
 
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
        
        public SettingsViewModel()
        {
            _musicFolders = new ObservableCollection<CustomFolder>();
            _videoFolders = new ObservableCollection<CustomFolder>();
        }

        public void PopulateCustomFolders()
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
                    Mru = KnownFolders.VideosLibrary.Path,
                });
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
                    Mru = KnownFolders.MusicLibrary.Path,
                });
            }
        }
    }

    public class CustomFolder
    {
        public string DisplayName { get; set; }
        public string Mru { get; set; }
    }
}
