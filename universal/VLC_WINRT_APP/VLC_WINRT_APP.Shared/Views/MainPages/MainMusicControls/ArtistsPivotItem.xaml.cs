using System;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using VLC_WINRT_APP.Views.MainPages.MusicPanes;

namespace VLC_WINRT_APP.Views.MainPages.MainMusicControls
{
    public sealed partial class ArtistsPivotItem : Page
    {
        public ArtistsPivotItem()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            this.Content = new ArtistCollectionBase();
        }
    }
}
