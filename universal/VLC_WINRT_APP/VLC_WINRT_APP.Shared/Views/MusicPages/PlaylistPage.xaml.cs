using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Microsoft.Xaml.Interactivity;
using VLC_WINRT_APP.Helpers;
using VLC_WINRT_APP.Model.Music;
using VLC_WINRT_APP.ViewModels;
#if WINDOWS_PHONE_APP
using Windows.Phone.UI.Input;
#endif


namespace VLC_WINRT_APP.Views.MusicPages
{
    public sealed partial class PlaylistPage : Page
    {
        public PlaylistPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
#if WINDOWS_PHONE_APP
            HardwareButtons.BackPressed += HardwareButtonsOnBackPressed;
#endif
            Responsive();
            Window.Current.SizeChanged += CurrentOnSizeChanged;
            this.Unloaded += OnUnloaded;
        }

        private void CurrentOnSizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            Responsive();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            Window.Current.SizeChanged -= CurrentOnSizeChanged;
        }

        void Responsive()
        {
            if (Window.Current.Bounds.Width > 700)
            {
                VisualStateUtilities.GoToState(this, "Horizontal", false);
            }
            else
            {
                VisualStateUtilities.GoToState(this, "Vertical", false);
            }
        }

#if WINDOWS_PHONE_APP
        private void HardwareButtonsOnBackPressed(object sender, BackPressedEventArgs backPressedEventArgs)
        {
            if (App.ApplicationFrame.CanGoBack)
                App.ApplicationFrame.GoBack();
            backPressedEventArgs.Handled = true;
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            HardwareButtons.BackPressed -= HardwareButtonsOnBackPressed;
        }
#endif
        private void PlayListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems != null && e.AddedItems.Any())
            {
                foreach (var addedItem in e.AddedItems)
                {
                    Locator.MusicLibraryVM.CurrentTrackCollection.SelectedTracks.Add(addedItem as TrackItem);
                }
            }
            if (e.RemovedItems != null && e.RemovedItems.Any())
            {
                foreach (var removedItem in e.RemovedItems)
                {
                    Locator.MusicLibraryVM.CurrentTrackCollection.SelectedTracks.Remove(removedItem as TrackItem);
                }
            }
        }
    }
}
