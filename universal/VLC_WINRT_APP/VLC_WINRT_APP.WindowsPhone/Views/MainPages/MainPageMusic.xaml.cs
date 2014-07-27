using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using VLC_WINRT_APP.ViewModels;


namespace VLC_WINRT_APP.Views.MainPages
{
    public sealed partial class MainPageMusic : UserControl
    {
        public MainPageMusic()
        {
            this.InitializeComponent();
        }
        private void SemanticZoom_OnViewChangeCompletedArtistByName(object sender, SemanticZoomViewChangedEventArgs e)
        {
            Locator.MusicLibraryVM.ExecuteSemanticZoom(SemanticZoom, ArtistsGroupedByName);
        }

        private void Header_Tapped(object sender, TappedRoutedEventArgs e)
        {
            SemanticZoom.IsZoomedInViewActive = false;
        }

        private void VariableSizedWrapGrid_Loaded(object sender, RoutedEventArgs e)
        {
            DisplayInformation displayInformation = DisplayInformation.GetForCurrentView();
            ((VariableSizedWrapGrid)sender).ItemHeight = (displayInformation.LogicalDpi < 200) ? 150 : 200;
            ((VariableSizedWrapGrid)sender).ItemWidth = (displayInformation.LogicalDpi < 200) ? 150 : 200;
            //Debug.WriteLine(displayInformation.LogicalDpi);
        }
    }
}
