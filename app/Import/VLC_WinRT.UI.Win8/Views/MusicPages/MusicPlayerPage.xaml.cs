using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using Microsoft.Xaml.Interactivity;
using VLC;
using VLC.ViewModels;

namespace VLC_WinRT.Views.MusicPages
{
    public sealed partial class MusicPlayerPage : Page
    {
        public MusicPlayerPage()
        {
            this.InitializeComponent();
            this.Loaded += MusicPlayerPage_Loaded;
        }

        void MusicPlayerPage_Loaded(object sender, RoutedEventArgs e)
        {
            Responsive();
            this.SizeChanged += OnSizeChanged;
            this.Unloaded += OnUnloaded;
        }

        #region layout
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
#if WINDOWS_PHONE_APP
#else
            Locator.MediaPlaybackViewModel.MouseService.OnHidden += MouseStateChanged;
            Locator.MediaPlaybackViewModel.MouseService.OnMoved += MouseMoved;
#endif
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            MouseMoved();
#if WINDOWS_PHONE_APP
#else
            Locator.MediaPlaybackViewModel.MouseService.OnHidden -= MouseStateChanged;
            Locator.MediaPlaybackViewModel.MouseService.OnMoved -= MouseMoved;
#endif
        }

        private void MouseStateChanged()
        {
            App.SplitShell.HideTopBar();
            FadeOut.Begin();
        }

        private void MouseMoved()
        {
            App.SplitShell.ShowTopBar();
      //      FadeIn.Begin();
        }

        private void PlaceholderInteractionGrid_OnTapped(object sender, TappedRoutedEventArgs args)
        {
            if (args.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch) { }
          //      Locator.MediaPlaybackViewModel.MouseService.Content_Tapped(sender, args);
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
            Responsive();
        }

        private void OnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            this.SizeChanged -= OnSizeChanged;
        }

        void Responsive()
        {
            if (Window.Current.Bounds.Width < 640)
            {
                VisualStateUtilities.GoToState(this, "Narrow", false);
            }
            else
            {
                VisualStateUtilities.GoToState(this, "Wide", false);
            }
        }

        private void GridView_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
        }
        #endregion

        #region interactions
        private void GoBack(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }
        #endregion
    }
}
