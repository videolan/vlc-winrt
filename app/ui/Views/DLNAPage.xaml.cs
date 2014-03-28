/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using VLC_WINRT.Utility.Helpers;
using VLC_WINRT.Utility.Services.RunTime;
using VLC_WINRT.ViewModels.MainPage;
using VLC_WINRT.Common;


namespace VLC_WINRT.Views
{
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class DLNAPage : Page
    {
        public DLNAPage()
        {
            this.InitializeComponent();
            this.SizeChanged += OnSizeChanged;
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            FadeInPage.Begin();
        }
        private async void GoBack_Click(object sender, RoutedEventArgs e)
        {
            if (Window.Current.Bounds.Width == 320 && FirstPanelGridView.Visibility == Windows.UI.Xaml.Visibility.Collapsed)
            {
                FirstPanelGridView.Visibility = Windows.UI.Xaml.Visibility.Visible;
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
                FirstPanelGridView.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
            SecondPanelGridView.ItemsSource = (e.ClickedItem as VideoLibraryViewModel).Media;
            SecondPanelListView.ItemsSource = (e.ClickedItem as VideoLibraryViewModel).Media;
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
    }
}
