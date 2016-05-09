using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using VLC_WinRT.ViewModels;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
namespace VLC_WinRT.UI.UWP.Views.UserControls.Shell
{
    public sealed partial class VLCDialog : ContentDialog
    {
        public VLCDialog()
        {
            this.InitializeComponent();
        }

        private void PasswordBox1_GotFocus(object sender, RoutedEventArgs e)
        {
            Locator.MainVM.KeyboardListenerService.CanListen = false;
        }

        private void PasswordBox1_LostFocus(object sender, RoutedEventArgs e)
        {
            Locator.MainVM.KeyboardListenerService.CanListen = true;
        }
    }
}
