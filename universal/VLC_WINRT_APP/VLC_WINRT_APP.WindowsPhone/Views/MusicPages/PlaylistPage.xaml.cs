using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Windows.Phone.UI.Input;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using VLC_WINRT_APP.Helpers;
using VLC_WINRT_APP.Helpers.MusicLibrary;
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
                await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    try
                    {
                        await
                            MusicLibraryVM.TracklistItemRepository.Remove(trackItem.Id,
                                Locator.MusicLibraryVM.CurrentTrackCollection.Id);

                        Locator.MusicLibraryVM.CurrentTrackCollection.Playlist.Remove(trackItem);
                    }
                    catch (Exception exception)
                    {
                        LogHelper.Log(exception);
                    }
                });
            }
            DeleteButton.Visibility = Visibility.Collapsed;
            PlayButton.Visibility = Visibility.Visible;
        }

        private async void DeletePlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            await MusicLibraryManagement.DeletePlaylist(Locator.MusicLibraryVM.CurrentTrackCollection);
            App.ApplicationFrame.GoBack();
        }
    }
}
