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

namespace VLC_WinRT.UI.Legacy.Views.MusicPages
{
    public sealed partial class MiniPlayerWindow : Page
    {
        public MiniPlayerWindow()
        {
            this.InitializeComponent();
            Locator.MusicPlayerVM.PropertyChanged += MusicPlayerVM_PropertyChanged;
            Locator.MediaPlaybackViewModel.TrackCollection.PropertyChanged += TrackCollection_PropertyChanged;
            Locator.MediaPlaybackViewModel.PropertyChanged += MediaPlaybackViewModel_PropertyChanged;
            this.Loaded += MiniPlayerWindow_Loaded;
        }

        private void MiniPlayerWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.SizeChanged += MiniPlayerWindow_SizeChanged;
            Initialize();
            Responsive();
            AppViewHelper.SetAppView(Colors.WhiteSmoke);
        }

        private void MiniPlayerWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Responsive();
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
                await SetArtistPicture();
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
                    case nameof(Locator.MediaPlaybackViewModel.TrackCollection.CanGoPrevious):
                        SetPreviousButton();
                        break;
                    case nameof(Locator.MediaPlaybackViewModel.TrackCollection.CanGoNext):
                        SetNextButton();
                        break;
                    case nameof(Locator.MediaPlaybackViewModel.TrackCollection.Playlist):
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
                        await SetArtistPicture();
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
                    var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(Locator.MusicPlayerVM.CurrentAlbum.AlbumCoverUri));
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

        async Task SetArtistPicture()
        {
            bool fileExists = Locator.MusicPlayerVM.CurrentArtist.IsPictureLoaded;
            try
            {
                if (fileExists)
                {
                    var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(Locator.MusicPlayerVM.CurrentArtist.Picture));
                    using (var stream = await file.OpenAsync(FileAccessMode.Read))
                    {
                        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            var image = new BitmapImage();
                            image.SetSource(stream);
                            ArtistPic.Source = image;
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log("Error getting artist picture : " + Locator.MusicPlayerVM.CurrentArtist.Name);
            }
        }

        void SetAlbumName()
        {
            //AlbumName.Text = Locator.MusicPlayerVM.CurrentAlbum.Name;
        }

        void SetArtistName()
        {
            ArtistName.Text = Locator.MusicPlayerVM.CurrentArtist.Name;
        }

        private void SetTrackName()
        {
            TrackName.Text = Locator.MusicPlayerVM.CurrentTrack.Name;
        }

        void SetPreviousButton()
        {
            PreviousButton.Visibility = (Locator.MediaPlaybackViewModel.TrackCollection.CanGoPrevious)
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
            NextButton.Visibility = (Locator.MediaPlaybackViewModel.TrackCollection.CanGoNext)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        void SetTrackList()
        {
            TracksListView.ItemsSource = Locator.MediaPlaybackViewModel.TrackCollection.Playlist;
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
            TracksListView.Visibility = (Window.Current.Bounds.Height > 150) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ListViewBase_OnItemClick(object sender, ItemClickEventArgs e)
        {
            Locator.MusicLibraryVM.TrackClickedCommand.Execute(e);
        }

        private async void OpenMainWindow_Click(object sender, RoutedEventArgs e)
        {
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () => CoreApplication.MainView.CoreWindow.Activate());
        }

        private async void ExpandWindow_Click(object sender, RoutedEventArgs e)
        {
            Locator.NavigationService.GoBack_Specific();
        }
    }
}
