/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VLC_WINRT.Utility.Helpers;
using VLC_WINRT.Utility.Services.RunTime;
using VLC_WINRT.ViewModels;
using VLC_WINRT.ViewModels.MainPage;
using VLC_WINRT.Views.Controls.MainPage;
using VLC_WINRT_APP;

namespace VLC_WINRT.Views.Controls
{
    public sealed partial class SearchFlyout : UserControl
    {
        private readonly List<string> _comboBoxStrings = new List<string>
        {
            "Artists",
            "Tracks",
            "Videos",
        };

        public SearchFlyout()
        {
            this.InitializeComponent();
            Window.Current.CoreWindow.KeyDown += (sender, arg) =>
            {
                if (arg.VirtualKey == Windows.System.VirtualKey.Enter)
                {
                    Search();
                }

            };
            UIAnimationHelper.FadeOut(this);
            ComboBox.ItemsSource = _comboBoxStrings;
        }

        public void Show()
        {
            UIAnimationHelper.FadeIn(this);
            FadeInAnimation.Begin();
        }

        public void Hide()
        {
            FadeOutAnimation.Begin();
            UIAnimationHelper.FadeOut(this);
        }

        private void LaunchSearch(object sender, RoutedEventArgs e)
        {
            Search();
        }

        private void Search()
        {
            if (ComboBox.SelectedIndex == -1)
            {
                ErrorTextBlock.Visibility = Visibility.Visible;
                return;
            }

            ErrorTextBlock.Visibility = Visibility.Collapsed;

            List<GenericElement> results = new List<GenericElement>();
            if (ComboBox.SelectedIndex == 0)
            {
                var music = Locator.MusicLibraryVM.Artist.Where(x => x.Name.ToLower().Contains(SearchBox.Text.ToLower()));
                results = music.Select(track => new GenericElement()
                {
                    Title = track.Name,
                    SubTitle = track.Albums.Count + " albums",
                    Object = track,
                }).ToList();
            }
            else if (ComboBox.SelectedIndex == 1)
            {
                var music = Locator.MusicLibraryVM.Track.Where(x => x.Name.ToLower().Contains(SearchBox.Text.ToLower()));
                results = music.Select(track => new GenericElement()
                {
                    Title = track.Name,
                    SubTitle = track.ArtistName,
                    Object = track,
                }).ToList();
            }
            else if (ComboBox.SelectedIndex == 2)
            {
                var videos =
                    Locator.VideoLibraryVM.Media.Where(y => y.Title.ToLower().Contains(SearchBox.Text.ToLower()));
                results.AddRange(videos.Select(video => new GenericElement()
                {
                    Title = video.Title,
                    SubTitle = video.Subtitle,
                    Object = video,
                }));
            }
            SearchListView.ItemsSource = results;
        }

        public class GenericElement
        {
            public string Title { get; set; }
            public string SubTitle { get; set; }
            public object Object { get; set; }
        }

        private void SearchFlyoutCloseButton_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        private void SearchListView_OnItemClick(object sender, ItemClickEventArgs e)
        {
            var objet = e.ClickedItem as GenericElement;
            NavigationService.NavigateTo(typeof(Views.MainPage));
            var page = App.ApplicationFrame.Content as Views.MainPage;
            if (page != null)
            {
                Hide();
                var musicColumn = page.GetFirstDescendantOfType<MusicColumn>() as MusicColumn;
                var videoColumn = page.GetFirstDescendantOfType<VideoColumn>();
                if (objet.Object.GetType() == typeof(MusicLibraryViewModel.ArtistItem))
                {
                    var gV = musicColumn.FindName("AlbumsByArtistListView") as GridView;
                    var lV = musicColumn.FindName("AlbumsByArtistListViewSnap") as ListView;
                    musicColumn.ChangedSectionsHeadersState(0);
                    //page.ChangedSectionsHeadersState(2);
                    
                    gV.ScrollIntoView(objet.Object);
                    lV.ScrollIntoView(objet.Object);
                }
                else if (objet.Object.GetType() == typeof(MusicLibraryViewModel.TrackItem))
                {
                    var track = objet.Object as MusicLibraryViewModel.TrackItem;
                    track.PlayTrack.Execute(track);
                }
                else if (objet.Object.GetType() == typeof (MediaViewModel))
                {
                    //page.ChangedSectionsHeadersState(1);
                    (videoColumn.FindName("ZoomedInGridViewFull") as GridView).ScrollIntoView(objet.Object);
                    (videoColumn.FindName("FirstPanelListView") as ListView).ScrollIntoView(objet.Object);
                }
            }
        }
    }
}
