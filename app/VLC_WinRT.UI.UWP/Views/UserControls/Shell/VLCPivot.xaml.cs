using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using VLC_WinRT.Helpers;
using VLC_WinRT.ViewModels;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace VLC_WinRT.Views.UserControls
{
    public sealed partial class VLCPivot : UserControl
    {
        public VLCPivot()
        {
            this.InitializeComponent();
        }
        
        private void TitleBar_Loaded(object sender, RoutedEventArgs e)
        {
            AppViewHelper.SetTitleBar(TitleBar);
            App.SplitShell.ContentSizeChanged += SplitShell_ContentSizeChanged;
            SplitShell_ContentSizeChanged(Window.Current.Bounds.Width);
        }

        private void SplitShell_ContentSizeChanged(double newWidth)
        {
            var pivotHeader = WinRTXamlToolkit.Controls.Extensions.VisualTreeHelperExtensions.GetFirstDescendantOfType<PivotHeaderPanel>(Pivot);

            var rightOffset = CoreApplication.GetCurrentView().TitleBar.SystemOverlayRightInset;
            var w = newWidth - MenuDropdown.ActualWidth - pivotHeader.ActualWidth - rightOffset;
            TitleBar.Width = w < 0 ? 0 : w;
            TitleBar.Margin = new Thickness(0, 0, rightOffset,0);
        }
    }
}
