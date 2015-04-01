using System;
using VLC_WinRT.Model;
#if WINDOWS_PHONE_APP
using Windows.Phone.UI.Input;
#endif

namespace VLC_WinRT.Services.RunTime
{
    public class NavigationService
    {
        public VLCPage CurrentPage;

        public NavigationService()
        {
#if WINDOWS_PHONE_APP
            HardwareButtons.BackPressed += HardwareButtons_BackPressed;
#endif    
        }

#if WINDOWS_PHONE_APP
        private void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        {
            switch (CurrentPage)
            {
                case VLCPage.MainPageHome:
                    break;
                case VLCPage.MainPageVideo:
                    break;
                case VLCPage.MainPageMusic:
                    break;
                case VLCPage.MainPageFileExplorer:
                    break;
                case VLCPage.AlbumPage:
                    break;
                case VLCPage.ArtistPage:
                    break;
                case VLCPage.PlaylistPage:
                    break;
                case VLCPage.CurrentPlaylistPage:
                    break;
                case VLCPage.VideoPlayerPage:
                    break;
                case VLCPage.MusicPlayerPage:
                    break;
                case VLCPage.SettingsPage:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
#endif
    }
}
