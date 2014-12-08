using System;
using Windows.Phone.UI.Input;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using VLC_WINRT_APP.Helpers;
using VLC_WINRT_APP.Helpers.MusicLibrary;
using VLC_WINRT_APP.Model;
using VLC_WINRT_APP.Model.Music;
using VLC_WINRT_APP.ViewModels;
using VLC_WINRT_APP.Views.MainPages.MainMusicControls;


namespace VLC_WINRT_APP.Views.MainPages
{
    public sealed partial class MainPageMusic : UserControl
    {
        public MainPageMusic()
        {
            this.InitializeComponent();
            this.Loaded += OnLoaded;
            this.SizeChanged += OnSizeChanged;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            this.Unloaded += OnUnloaded;
            HardwareButtons.BackPressed += HardwareButtonsOnBackPressed;
        }

        private async void HardwareButtonsOnBackPressed(object sender, BackPressedEventArgs backPressedEventArgs)
        {
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (App.ApplicationFrame.CanGoBack)
                {
                    backPressedEventArgs.Handled = true;
                    App.ApplicationFrame.GoBack();
                }
            });
        }

        private void OnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            HardwareButtons.BackPressed -= HardwareButtonsOnBackPressed;
        }

        //protected override void OnNavigatedTo(NavigationEventArgs e)
        //{
        //    base.OnNavigatedTo(e);
        //    if (e.NavigationMode == NavigationMode.New && Locator.MusicLibraryVM.LoadingState == LoadingState.NotLoaded)
        //    {
        //        Locator.MusicLibraryVM.Initialize();
        //    }
        //}

        private void OnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
            //if (DisplayHelper.IsPortrait())
            //{
            //    CommandBar.Visibility = Visibility.Visible;
            //}
            //else
            //{
            //    CommandBar.Visibility = Visibility.Collapsed;
            //}
        }

        private void MainPageMusicContentPresenter_OnLoaded(object sender, RoutedEventArgs e)
        {
            MainPageMusicContentPresenter.Navigate(typeof (AlbumsPivotItem));
        }
    }
}
