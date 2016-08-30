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
            if (e.Parameter == null)
                SetView(isArtistInfoView: false);
            else
                SetView((VLCPage)e.Parameter == VLCPage.ArtistInfoView);
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
        }

        public void SetView(bool isArtistInfoView)
        {
            if (isArtistInfoView)
            {
                (FindName(nameof(ExtraInfoGrid)) as FrameworkElement).Visibility = Visibility.Visible;
                ArtistAlbumsSemanticView.Visibility = Visibility.Collapsed;
            }
            else
            {
                (FindName(nameof(ExtraInfoGrid)) as FrameworkElement).Visibility = Visibility.Collapsed;
                ArtistAlbumsSemanticView.Visibility = Visibility.Visible;
            }
        }
    }
}