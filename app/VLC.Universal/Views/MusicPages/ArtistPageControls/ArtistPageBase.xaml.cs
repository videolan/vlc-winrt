using System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Xaml.Interactivity;
using VLC.Model.Video;
using VLC.ViewModels;
using Windows.UI.Xaml.Navigation;
using VLC.Model;

namespace VLC.UI.Legacy.Views.MusicPages.ArtistPageControls
{
    public sealed partial class ArtistPageBase : Page
    {
        public ArtistPageBase()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if ((VLCPage)e.Parameter == VLCPage.ArtistInfoView)
            {
                MainPivot.SelectedIndex = 1;
            }
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
        }
    }
}