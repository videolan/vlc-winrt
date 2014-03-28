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

        }
    }

    public class CustomFolder
    {
        public string DisplayName { get; set; }
        public string Mru { get; set; }
    }
}
