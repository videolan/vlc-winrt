using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VLC.Model.Video;
using VLC.ViewModels;

namespace VLC.Views.MainPages.MainVideoControls
{
    public sealed partial class AllVideosPivotItem : Page
    {
        public AllVideosPivotItem()
        {
            this.InitializeComponent();
            this.Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            this.Unloaded += AllVideosPivotItem_Unloaded;
            Locator.VideoLibraryVM.OnNavigatedToAllVideos();
        }

        private async void AllVideosPivotItem_Unloaded(object sender, RoutedEventArgs e)
        {
            await Locator.VideoLibraryVM.OnNavigatedFromAllVideos();
        }

        private void VideosWrapGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            TemplateSizer.ComputeCompactVideo(sender as ItemsWrapGrid, this.ActualWidth);
        }
    }
}
