using Microsoft.Xaml.Interactivity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using VLC_WINRT_APP.Model.Video;
using VLC_WINRT_APP.ViewModels;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
namespace VLC_WINRT_APP.Views.MainPages.MainVideoControls
{
    public sealed partial class SearchVideoPage : UserControl
    {
        public SearchVideoPage()
        {
            this.InitializeComponent();
            this.Loaded += SearchVideoPage_Loaded;
        }

        void SearchVideoPage_Loaded(object sender, RoutedEventArgs e)
        {
            Responsive(Window.Current.Bounds.Width);
            Window.Current.SizeChanged += Current_SizeChanged;
            this.Unloaded += SearchVideoPage_Unloaded;
        }

        void Current_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            Responsive(e.Size.Width);
        }

        void SearchVideoPage_Unloaded(object sender, RoutedEventArgs e)
        {
            Window.Current.SizeChanged -= Current_SizeChanged;
        }

        void Responsive(double width)
        {
            if (width <= 550)
                VisualStateUtilities.GoToState(this, "Minimal", false);
            else
                VisualStateUtilities.GoToState(this, "Normal", false);
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Locator.VideoLibraryVM.SearchTag = VideoSearchBox.Text;
        }

        private void VideosWrapGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            TemplateSizer.ComputeCompactVideo(sender as ItemsWrapGrid, this.ActualWidth);
        }
    }
}
