/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Navigation;
using VLC_WINRT.Utility.Helpers;
using VLC_WINRT.Utility.Services.RunTime;

namespace VLC_WINRT.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SpecialThanks : Page
    {
        private int itemsLoaded = -250;
        private bool isLoading = false;
        private LoadBackers loadBackers;
        public SpecialThanks()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            loadBackers = new LoadBackers();
            LoadMore();
        }

        private void GoBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.NavigateTo(typeof(MainPage));
        }

        private void BackersGridView_OnLoaded(object sender, RoutedEventArgs e)
        {
            var scrollbars = MainScrollViewer.GetDescendantsOfType<ScrollBar>().ToList();

            var bar = scrollbars.FirstOrDefault(x => x.Orientation == Orientation.Horizontal); 
            if (bar != null)
                bar.Scroll += BarScroll;

            MainScrollViewer.ViewChanged += MainScrollViewerOnViewChanged;
        }

        private void MainScrollViewerOnViewChanged(object sender, ScrollViewerViewChangedEventArgs scrollViewerViewChangedEventArgs)
        {
            if (MainScrollViewer.HorizontalOffset >= MainScrollViewer.ScrollableWidth - 100 && !isLoading)
            {
                LoadMore();
            }
        }

        async void LoadMore()
        {
            itemsLoaded += 250;
            isLoading = true;
            await loadBackers.Get(itemsLoaded);
            isLoading = false;
        }

        private void BarScroll(object sender, ScrollEventArgs e)
        {
            if (e.ScrollEventType != ScrollEventType.EndScroll) 
                return;

            var bar = sender as ScrollBar;
            if (bar == null)
                return;

            System.Diagnostics.Debug.WriteLine("Scrolling ended");

            if (e.NewValue >= bar.Maximum - 100 && !isLoading)
            {
                LoadMore();
            }
        }
    }
}
