using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace VLC_WINRT_APP.Views.UserControls
{
    public sealed partial class CompactVideoItemControl : UserControl
    {
        public CompactVideoItemControl()
        {
            this.InitializeComponent();
        }
        private void RootAlbumItem_Holding(object sender, HoldingRoutedEventArgs e)
        {
#if WINDOWS_PHONE_APP
            Flyout.ShowAttachedFlyout((Border)sender);
#endif
        }

        private void Grid_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
#if WINDOWS_APP
            Flyout.ShowAttachedFlyout((Border)sender);
#endif
        }
    }
}
