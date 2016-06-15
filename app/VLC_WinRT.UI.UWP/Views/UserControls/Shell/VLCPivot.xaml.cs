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
    public sealed partial class VLCPivot
    {
        public VLCPivot()
        {
            this.InitializeComponent();
            this.SizeChanged += VLCPivot_SizeChanged;
        }

        private void VLCPivot_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Responsive();
        }

        private void TitleBar_Loaded(object sender, RoutedEventArgs e)
        {
            AppViewHelper.SetTitleBar(TitleBar);
            Responsive();
        }

        private void Responsive()
        {
            if (Window.Current.Bounds.Width < 850)
            {
                VLCIcon.Visibility = Visibility.Collapsed;
            }
            else
            {
                VLCIcon.Visibility = Visibility.Visible;
            }


            if (AppViewHelper.TitleBarRightOffset == 0)
                return;

            var pivotHeader = WinRTXamlToolkit.Controls.Extensions.VisualTreeHelperExtensions.GetFirstDescendantOfType<PivotHeaderPanel>(Pivot);
            if (pivotHeader == null)
                return;

            if (Window.Current.Bounds.Width < 850)
            {
                pivotHeader.Margin = new Thickness(0, 16, 0, 0);
            }
            else
            {
                pivotHeader.Margin = new Thickness();
            }
        }
    }
}
