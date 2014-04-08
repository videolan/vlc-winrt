/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using Windows.Storage.AccessCache;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using VLC_WINRT.Utility.Helpers;
using VLC_WINRT.Utility.Services.RunTime;
using VLC_WINRT.ViewModels;
using VLC_WINRT.ViewModels.MainPage;
using VLC_WINRT.Common;
using VLC_WINRT.ViewModels.MainPage.VlcExplorer;

namespace VLC_WINRT.Views
{
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class RemovableStoragePage : Page
    {
        public RemovableStoragePage()
        {
            InitializeComponent();
            SizeChanged += OnSizeChanged;
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            FadeInPage.Begin();
        }
        private async void GoBack_Click(object sender, RoutedEventArgs e)
        {
            if (Window.Current.Bounds.Width == 320 && 
                FirstPanelGridView.Visibility == Visibility.Collapsed)
            {
                FirstPanelGridView.Visibility = Visibility.Visible;
            }
            else
            {
                await FadeOutPage.BeginAsync();
                NavigationService.NavigateTo(typeof(MainPage));
            }
        }

        private void FirstPanelGridView_SelectionChanged(object sender, ItemClickEventArgs e)
        {
            if (Window.Current.Bounds.Width == 320)
            {
                FirstPanelGridView.Visibility = Visibility.Collapsed;
            }
            var removableLibraryViewModel = e.ClickedItem as FileExplorerViewModel;
            if (removableLibraryViewModel == null) return;

            SecondPanelGridView.ItemsSource = removableLibraryViewModel.StorageItems;
            SecondPanelListView.ItemsSource = removableLibraryViewModel.StorageItems;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
            DispatchHelper.Invoke(() =>
            {
                if (sizeChangedEventArgs.NewSize.Width == 320)
                {
                    FirstPanelGridView.Margin = new Thickness(10, 0, 0, 0);
                    SecondPanelListView.Visibility = Visibility.Visible;
                    SecondPanelGridView.Visibility = Visibility.Collapsed;
                }
                else
                {
                    FirstPanelGridView.Margin = new Thickness(100, 0, 0, 0);
                    SecondPanelListView.Visibility = Visibility.Collapsed;
                    SecondPanelGridView.Visibility = Visibility.Visible;
                }
            });
        }

        private async void SecondPanelGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as IVlcStorageItem;
            if (item is FileExplorerViewModel)
            {
                await ((FileExplorerViewModel)item).GetFiles();
                SecondPanelGridView.ItemsSource = ((FileExplorerViewModel)item).StorageItems;
                SecondPanelListView.ItemsSource = ((FileExplorerViewModel)item).StorageItems;
            }
            else if (item is VlcStorageFile)
            {
                string FileName = ((VlcStorageFile)item).Name;
                string MRL = StorageApplicationPermissions.FutureAccessList.Add(((VlcStorageFile)item).StorageFile);
                Locator.PlayVideoVM.SetActiveVideoInfo(MRL, FileName);
                NavigationService.NavigateTo(typeof(PlayVideo));
            }
        }
    }
}
