using Windows.Graphics.Display;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using VLC_WINRT_APP.Helpers;
using VLC_WINRT_APP.Helpers.MusicLibrary.EchoNest;
using VLC_WINRT_APP.Model;
using VLC_WINRT_APP.Model.Video;
using VLC_WINRT_APP.ViewModels;
using VLC_WINRT_APP.Views.VariousPages.DEBUG;

namespace VLC_WINRT_APP.Views.MainPages
{
    public sealed partial class MainPageHome : Page
    {
        public MainPageHome()
        {
            InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
            this.SizeChanged += OnSizeChanged;
            Loaded += OnLoaded;
        }


        protected override void OnNavigatedTo(Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Locator.MainVM.UpdateSecondaryAppBarButtons();
            if (Locator.VideoLibraryVM.LoadingState == LoadingState.NotLoaded)
            {
                Locator.VideoLibraryVM.Initialize();
            }
            if (Locator.MusicLibraryVM.LoadingState == LoadingState.NotLoaded)
            {
                Locator.MusicLibraryVM.Initialize();
            }
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
            Responsive();
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Responsive();
        }

        private void Responsive()
        {
            if (DisplayHelper.IsPortrait())
            {
                CommandBar.Visibility = Visibility.Visible;
                MainPivot.Margin = new Thickness(-7, 15, -15, 0);
            }
            else
            {
                CommandBar.Visibility = Visibility.Collapsed;
                MainPivot.Margin = new Thickness(-7, -15, -25, 0);
            }
        }


        private void VideoWrapGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            TemplateSizer.ComputeCompactVideo(sender as ItemsWrapGrid);
        }

        private void AlbumsWrapGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            TemplateSizer.ComputeAlbums(sender as ItemsWrapGrid);
        }

        private void DEBUGHISTORY(object sender, RoutedEventArgs e)
        {
            App.ApplicationFrame.Navigate(typeof(historyvideo));
        }

        private void MainPivot_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Locator.MainVM.UpdateAppBar(MainPivot.SelectedIndex);
        }
    }
}