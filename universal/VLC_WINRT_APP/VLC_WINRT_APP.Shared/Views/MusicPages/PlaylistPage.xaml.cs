using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using VLC_WINRT_APP.Helpers;
using VLC_WINRT_APP.Helpers.MusicLibrary;
using VLC_WINRT_APP.Model.Music;
using VLC_WINRT_APP.ViewModels;
using VLC_WINRT_APP.ViewModels.MusicVM;
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
            await AppBarHelper.UpdateAppBar(typeof(PlaylistPage));
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
