using System.Diagnostics;
using Windows.Graphics.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VLC_WINRT_APP.Model;
using VLC_WINRT_APP.Model.Video;
using VLC_WINRT_APP.ViewModels;

namespace VLC_WINRT_APP.Views.MainPages
{
    public sealed partial class MainPageHome : Page
    {
        public MainPageHome()
        {
            InitializeComponent();
            this.SizeChanged += OnSizeChanged;
            Loaded += OnLoaded;
        }


        protected override void OnNavigatedTo(Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
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
            var width = Window.Current.Bounds.Width;
            Debug.WriteLine(width);
        }

        private void ItemsWrapGrid_Loaded(object sender, RoutedEventArgs e)
        {
            VideoTemplateSizer.ComputeCompactVideo(sender as ItemsWrapGrid);
        }
    }
}