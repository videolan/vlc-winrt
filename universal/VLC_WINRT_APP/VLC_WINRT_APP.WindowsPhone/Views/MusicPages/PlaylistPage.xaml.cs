using System;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.Phone.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using VLC_WINRT_APP.Model.Music;
using VLC_WINRT_APP.ViewModels;
using VLC_WINRT_APP.ViewModels.MusicVM;

namespace VLC_WINRT_APP.Views.MusicPages
{
    public sealed partial class PlaylistPage : Page
    {
        public PlaylistPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            HardwareButtons.BackPressed += HardwareButtonsOnBackPressed;
        }

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

        private void PlayListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItems = (sender as ListView).SelectedItems;
            if (selectedItems != null && selectedItems.Count > 0)
            {
                DeleteButton.Visibility = Visibility.Visible;
                PlayButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                PlayButton.Visibility = Visibility.Visible;
                DeleteButton.Visibility = Visibility.Collapsed;
            }
        }

        private async void DeleteButton_OnClick(object sender, RoutedEventArgs e)
        {
            foreach (var selectedItem in PlayListView.SelectedItems)
            {
                var trackItem = selectedItem as TrackItem;
                if (trackItem == null) continue;
                await
                    MusicLibraryVM.TracklistItemRepository.Remove(trackItem.Id,
                        Locator.MusicLibraryVM.CurrentTrackCollection.Id);

                Locator.MusicLibraryVM.CurrentTrackCollection.Playlist.Remove(trackItem);
            }
            DeleteButton.Visibility = Visibility.Collapsed;
            PlayButton.Visibility = Visibility.Visible;
        }
    }
}
