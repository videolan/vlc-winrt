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
        public bool PreventAppExit { get; set; } = false;

        public NavigationService()
        {
#if WINDOWS_PHONE_APP
            HardwareButtons.BackPressed += HardwareButtons_BackPressed;
#endif    
        }

#if WINDOWS_PHONE_APP
        private void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        {
            e.Handled = true;
            switch (CurrentPage)
            {
                case VLCPage.MainPageHome:
                    e.Handled = false;
                    break;
                case VLCPage.MainPageVideo:
                    break;
                case VLCPage.MainPageMusic:
                    break;
                case VLCPage.MainPageFileExplorer:
                    break;
                case VLCPage.AlbumPage:
                    GoBack_HideFlyout();
                    break;
                case VLCPage.ArtistPage:
                    GoBack_HideFlyout();
                    break;
                case VLCPage.PlaylistPage:
                    GoBack_Default();
                    break;
                case VLCPage.CurrentPlaylistPage:
                    GoBack_Default();
                    break;
                case VLCPage.VideoPlayerPage:
                    GoBack_Default();
                    break;
                case VLCPage.MusicPlayerPage:
                    GoBack_Default();
                    break;
                case VLCPage.SettingsPage:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
#endif

        void GoBack_Default()
        {
            App.ApplicationFrame.GoBack();
        }

        void GoBack_HideFlyout()
        {
            App.RootPage.SplitShell.HideFlyout();
        }
    }
}
