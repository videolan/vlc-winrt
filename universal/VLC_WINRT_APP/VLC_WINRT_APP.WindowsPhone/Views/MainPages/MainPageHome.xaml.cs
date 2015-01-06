using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Resources;
using Windows.Phone.UI.Input;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using VLC_WINRT_APP.Helpers;
using VLC_WINRT_APP.Model;
using VLC_WINRT_APP.Model.Video;
using VLC_WINRT_APP.ViewModels;
using VLC_WINRT_APP.ViewModels.MusicVM;
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
        private bool NeedsToDrop()
        {
            Package thisPackage = Package.Current;
            PackageVersion version = thisPackage.Id.Version;
            string appVersion = string.Format("{0}.{1}.{2}.{3}",
                version.Major, version.Minor, version.Build, version.Revision);
            if (ApplicationSettingsHelper.Contains("CurrentVersion") && ApplicationSettingsHelper.ReadSettingsValue("CurrentVersion").ToString() == appVersion)
            {
                return false;
            }
            else
            {
                ApplicationSettingsHelper.SaveSettingsValue("CurrentVersion", appVersion);
                return true;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (NeedsToDrop())
            {
                MusicLibraryVM.TrackCollectionRepository.Drop();
                MusicLibraryVM.TracklistItemRepository.Drop();
                MusicLibraryVM._albumDataRepository.Drop();
                MusicLibraryVM._artistDataRepository.Drop();
                MusicLibraryVM._trackDataRepository.Drop();
                Locator.VideoLibraryVM.VideoRepository.Drop();
                MusicLibraryVM.TrackCollectionRepository.Initialize();
                MusicLibraryVM.TracklistItemRepository.Initialize();
                MusicLibraryVM._albumDataRepository.Initialize();
                MusicLibraryVM._artistDataRepository.Initialize();
                MusicLibraryVM._trackDataRepository.Initialize();
                Locator.VideoLibraryVM.VideoRepository.Initialize();
            }
            HardwareButtons.BackPressed += HardwareButtonsOnBackPressed;
            AppBarHelper.UpdateAppBar(typeof(MainPageHome), MainPivot.SelectedIndex);
            if (Locator.VideoLibraryVM.LoadingState == LoadingState.NotLoaded)
            {
                Task.Run(async () => await Locator.VideoLibraryVM.Initialize());
            }
            if (Locator.MusicLibraryVM.LoadingState == LoadingState.NotLoaded)
            {
                Task.Run(async () => await Locator.MusicLibraryVM.Initialize());
            }
            SdCardTest();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            HardwareButtons.BackPressed -= HardwareButtonsOnBackPressed;
        }

        private async Task SdCardTest()
        {
            if (MainPivot.Items != null && MainPivot != null && MainPivot.Items.Count == 4) return;
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
                PivotTitle.Margin = new Thickness(0, 16, 0, 0);
                MainPivot.Margin = new Thickness(-7, 0, -15, 0);
                HeaderBackgroundGrid.Height = 110;
            }
            else
            {
                PivotTitle.Margin = new Thickness(0, 2, 0, 0);
                MainPivot.Margin = new Thickness(-7, -15, -25, 0);
                HeaderBackgroundGrid.Height = 50;
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