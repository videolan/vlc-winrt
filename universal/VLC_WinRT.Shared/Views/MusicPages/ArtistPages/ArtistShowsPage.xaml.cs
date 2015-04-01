using System;
using Windows.System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using VLC_WinRT.Model.Music;
#if WINDOWS_PHONE_APP
using Windows.Phone.UI.Input;
#endif

namespace VLC_WinRT.Views.MusicPages.ArtistPages
{
    public sealed partial class ArtistShowsPage : Page
    {
        public ArtistShowsPage()
        {
            this.InitializeComponent();
        }
    }
}
