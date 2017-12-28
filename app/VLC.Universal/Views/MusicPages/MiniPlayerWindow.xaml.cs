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
using VLC.Controls;
using VLC.Helpers;
using VLC.ViewModels;
using VLC.UI.Views.MainPages;
using VLC.Utils;
using VLC.Model;

namespace VLC.UI.Views.MusicPages
{
    public sealed partial class MiniPlayerWindow : Page
    {
        public MiniPlayerWindow()
        {
            this.InitializeComponent();
            Locator.MusicPlayerVM.PropertyChanged += MusicPlayerVM_PropertyChanged;
            Locator.MediaPlaybackViewModel.PlaybackService.Playback_MediaSet += Playback_MediaSet;
            Locator.MediaPlaybackViewModel.PropertyChanged += MediaPlaybackViewModel_PropertyChanged;
            this.Loaded += MiniPlayerWindow_Loaded;
        }

        private async void MiniPlayerWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();
            AppViewHelper.ResizeWindow(false, 400, 80);
            await AppViewHelper.SetAppView(true);
            AppViewHelper.SetTitleBar(DraggableGrid);
            Locator.MediaPlaybackViewModel.SliderBindingEnabled = true;
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

        private async void Playback_MediaSet(IMediaItem media)
        {
            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {
                SetPreviousButton();
                SetNextButton();
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
            if (Locator.MusicPlayerVM.CurrentAlbum == null)
                return;

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
                else
                {
                    await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => ImgCover.Source = null);
                }
            }
            catch (Exception)
            {
                LogHelper.Log("Error getting album picture : " + Locator.MusicPlayerVM.CurrentAlbum.Name);
            }
        }

        void SetAlbumName()
        {
        }

        void SetArtistName()
        {
            ArtistName.Text = (Locator.MusicPlayerVM.CurrentAlbum == null) ? Strings.NowPlaying : Locator.MusicPlayerVM.CurrentArtist?.Name;
        }

        private void SetTrackName()
        {
            TrackName.Text = string.IsNullOrEmpty(Locator.MusicPlayerVM.CurrentMediaTitle) ? string.Empty : Locator.MusicPlayerVM.CurrentMediaTitle;
        }

        void SetPreviousButton()
        {
            PreviousButton.IsEnabled = Locator.PlaybackService.CanGoPrevious;
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
            NextButton.IsEnabled = Locator.PlaybackService.CanGoNext;
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

        private void ListViewBase_OnItemClick(object sender, ItemClickEventArgs e)
        {
            Locator.MusicLibraryVM.PlayTrackCommand.Execute(e);
        }

        private async void OpenMainWindow_Click(object sender, RoutedEventArgs e)
        {
            await DispatchHelper.InvokeInUIThread(CoreDispatcherPriority.Low, () => CoreApplication.MainView.CoreWindow.Activate());
        }

        private void ExpandWindow_Click(object sender, RoutedEventArgs e)
        {
            Locator.NavigationService.GoBack_Specific();
        }
    }
}
