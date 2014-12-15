using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Graphics.Display;
using Windows.Phone.UI.Input;
using Windows.Storage;
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

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            AppBarHelper.UpdateAppBar(typeof(MainPageHome), MainPivot.SelectedIndex);
            if (Locator.VideoLibraryVM.LoadingState == LoadingState.NotLoaded)
            {
                Locator.VideoLibraryVM.Initialize();
            }
            if (Locator.MusicLibraryVM.LoadingState == LoadingState.NotLoaded)
            {
                Locator.MusicLibraryVM.Initialize();
            }
            SdCardTest();
        }

        private async Task SdCardTest()
        {
            var extDev = KnownFolders.RemovableDevices;
            var sdCard = (await extDev.GetFoldersAsync()).FirstOrDefault();
            if (sdCard != null)
            {
                var sdCardPivot = new PivotItem()
                {
                    Header = ResourceLoader.GetForCurrentView("Resources").GetString("SDCardHeader"),
                    Content = new MainPageSDCard(),
                };
                if (MainPivot != null && MainPivot.Items != null)
                {
                    MainPivot.Items.Add(sdCardPivot);
                }
            }
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
            Responsive();
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Responsive();
            this.Unloaded += OnUnloaded;
            HardwareButtons.BackPressed += HardwareButtonsOnBackPressed;
        }

        private void OnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            HardwareButtons.BackPressed -= HardwareButtonsOnBackPressed;
        }

        private void HardwareButtonsOnBackPressed(object sender, BackPressedEventArgs backPressedEventArgs)
        {
            if (MainPivot.SelectedIndex != 3 || (MainPivot.SelectedIndex == 3 && !Locator.ExternalStorageVM.CurrentStorageVM.CanGoBack))
            {
                backPressedEventArgs.Handled = false;
            }
        }


        private void Responsive()
        {
            if (DisplayHelper.IsPortrait())
            {
                //CommandBar.Visibility = Visibility.Visible;
                MainPivot.Margin = new Thickness(-7, 0, -15, 0);
                PivotTitle.Visibility = Visibility.Visible;
                HeaderBackgroundGrid.Height = 110;
                HeaderGradientGrid.Margin = new Thickness(0, 110, 0, 0);
                HeaderBackgroundImage.Margin = new Thickness(0, 110, 0, 0);
            }
            else
            {
                //CommandBar.Visibility = Visibility.Collapsed;
                MainPivot.Margin = new Thickness(-7, -15, -25, 0);
                PivotTitle.Visibility = Visibility.Collapsed;
                HeaderBackgroundGrid.Height = 50;
                HeaderGradientGrid.Margin = new Thickness(0, 50, 0, 0);
                HeaderBackgroundImage.Margin = new Thickness(0, 50, 0, 0);
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
            AppBarHelper.UpdateAppBar(typeof(MainPageHome), MainPivot.SelectedIndex);
        }
    }
}