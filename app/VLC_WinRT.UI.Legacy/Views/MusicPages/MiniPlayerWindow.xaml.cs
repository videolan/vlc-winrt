using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media.Imaging;
using VLC_WinRT.Controls;
using VLC_WinRT.Helpers;
using VLC_WinRT.ViewModels;
using VLC_WinRT.Views.MainPages;
using VLC_WinRT.Utils;

namespace VLC_WinRT.UI.Legacy.Views.MusicPages
{
    public sealed partial class MiniPlayerWindow : Page
    {
        public MiniPlayerWindow()
        {
            this.InitializeComponent();
            Locator.MusicPlayerVM.PropertyChanged += MusicPlayerVM_PropertyChanged;
            Locator.MediaPlaybackViewModel.PlaybackService.PropertyChanged += TrackCollection_PropertyChanged;
            Locator.MediaPlaybackViewModel.PropertyChanged += MediaPlaybackViewModel_PropertyChanged;
            this.Loaded += MiniPlayerWindow_Loaded;
        }

        private void MiniPlayerWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.SizeChanged += MiniPlayerWindow_SizeChanged;
            this.Unloaded += MiniPlayerWindow_Unloaded;
            Initialize();
            Responsive();
#if WINDOWS_UWP
            AppViewHelper.ResizeWindow(false, 400, 80);
            AppViewHelper.SetAppView(true);
            AppViewHelper.SetTitleBar(DraggableGrid);
#endif
            App.SplitShell.FooterVisibility = AppBarClosedDisplayMode.Hidden;
        }

        private void MiniPlayerWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            App.SplitShell.FooterVisibility = AppBarClosedDisplayMode.Compact;
        }

        private void MiniPlayerWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Responsive();
            //TitleBarRowDefinition.Height = new GridLength(AppViewHelper.TitleBarHeight, GridUnitType.Pixel);
        }

        async void Initialize()
        {
            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Low, async () =>
            {
                SetArtistName();
                SetAlbumName();
                SetTrackName();
                SetPreviousButton();
                SetNextButton();
                SetPlayPauseButtons();
                SetTrackList();
                await SetImgCover();
            });
        }
        
        private async void MediaPlaybackViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {
                switch (e.PropertyName)
                {
                    case nameof(Locator.MediaPlaybackViewModel.IsPlaying):
                        SetPlayPauseButtons();
                        break;
                }
            });
        }

        private async void TrackCollection_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {
                switch (e.PropertyName)
                {
                    case nameof(Locator.MediaPlaybackViewModel.PlaybackService.CanGoPrevious):
                        SetPreviousButton();
                        break;
                    case nameof(Locator.MediaPlaybackViewModel.PlaybackService.CanGoNext):
                        SetNextButton();
                        break;
                    case nameof(Locator.MediaPlaybackViewModel.PlaybackService.Playlist):
                        SetTrackList();
                        break;
                }
            });
        }

        private async void MusicPlayerVM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Low, async () =>
            {
                switch (e.PropertyName)
                {
                    case nameof(Locator.MusicPlayerVM.CurrentTrack):
                        SetTrackName();
                        break;
                    case nameof(Locator.MusicPlayerVM.CurrentAlbum):
                        await SetImgCover();
                        SetAlbumName();
                        break;
                    case nameof(Locator.MusicPlayerVM.CurrentArtist):
                        SetArtistName();
                        break;
                }
            });
        }

        async Task SetImgCover()
        {
            bool fileExists = Locator.MusicPlayerVM.CurrentAlbum.IsPictureLoaded;
            try
            {
                if (fileExists)
                {
                    var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(Locator.MusicPlayerVM.CurrentAlbum.AlbumCoverFullUri));
                    using (var stream = await file.OpenAsync(FileAccessMode.Read))
                    {
                        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            var image = new BitmapImage();
                            image.SetSource(stream);
                            ImgCover.Source = image;
                        });
                    }
                }
            }
            catch (Exception)
            {
                LogHelper.Log("Error getting album picture : " + Locator.MusicPlayerVM.CurrentAlbum.Name);
            }
        }
        
        void SetAlbumName()
        {
            //AlbumName.Text = Locator.MusicPlayerVM.CurrentAlbum?.Name;
        }

        void SetArtistName()
        {
            ArtistName.Text = Locator.MusicPlayerVM.CurrentArtist?.Name;
        }

        private void SetTrackName()
        {
            TrackName.Text = Locator.MusicPlayerVM.CurrentTrack?.Name;
        }

        void SetPreviousButton()
        {
            PreviousButton.Visibility = (Locator.MediaPlaybackViewModel.PlaybackService.CanGoPrevious)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        void SetPlayPauseButtons()
        {
            PauseIcon.Visibility = (Locator.MediaPlaybackViewModel.IsPlaying)
                ? Visibility.Visible
                : Visibility.Collapsed;

            PlayIcon.Visibility = (Locator.MediaPlaybackViewModel.IsPlaying)
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

        void SetNextButton()
        {
            NextButton.Visibility = (Locator.MediaPlaybackViewModel.PlaybackService.CanGoNext)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        void SetTrackList()
        {
        }

        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            Locator.MediaPlaybackViewModel.PlayPreviousCommand.Execute(null);
        }

        private void PlayPause_Click(object sender, RoutedEventArgs e)
        {
            Locator.MediaPlaybackViewModel.PlayOrPauseCommand.Execute(null);
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            Locator.MediaPlaybackViewModel.PlayNextCommand.Execute(null);
        }

        void Responsive()
        {
        }

        private void ListViewBase_OnItemClick(object sender, ItemClickEventArgs e)
        {
            Locator.MusicLibraryVM.TrackClickedCommand.Execute(e);
        }

        private async void OpenMainWindow_Click(object sender, RoutedEventArgs e)
        {
            await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Low, () => CoreApplication.MainView.CoreWindow.Activate());
        }

        private void ExpandWindow_Click(object sender, RoutedEventArgs e)
        {
            Locator.NavigationService.GoBack_Specific();
        }
    }
}
