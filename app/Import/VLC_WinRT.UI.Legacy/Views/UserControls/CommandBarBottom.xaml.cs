using System;
using System.ComponentModel;
using VLC.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using VLC.MusicMetaFetcher.Models.MusicEntities;

namespace VLC_WinRT.UI.Legacy.Views.UserControls
{
    public sealed partial class CommandBarBottom : CommandBar
    {
        public CommandBarBottom()
        {
            InitializeComponent();

            //MediaPlaybackViewModel.MiniPlayerVisibility
            Locator.MediaPlaybackViewModel.PropertyChanged += MediaPlaybackViewModelOnPropertyChanged;
        }

        void MediaPlaybackViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName != nameof(Locator.MediaPlaybackViewModel.MiniPlayerVisibility)) return;

            if (Locator.MediaPlaybackViewModel.MiniPlayerVisibility == Visibility.Visible)
                Show();
            else Hide();
        }

        void Show()
        {
            IsOpen = true;
            IsSticky = true;
            Visibility = Visibility.Visible;
        }

        void Hide()
        {
            IsOpen = false;
            IsSticky = false;
            Visibility = Visibility.Collapsed;
        }

        #region interactions
        private void RootMiniPlayer_Clicked(object sender, RoutedEventArgs e)
        {
            Locator.MusicPlayerVM.GoToMusicPlayerPage.Execute(null);
        }
        #endregion

        private void PlayButton_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            StopPlayback();
        }

        private void PauseButton_Holding(object sender, HoldingRoutedEventArgs e)
        {
            StopPlayback();
        }

        public void StopPlayback()
        {
            Locator.PlaybackService.Stop();
            Locator.PlaybackService.ClearPlaylist();
        }
    }
}
