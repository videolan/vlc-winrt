using libVLCX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using VLC_WinRT.Utils;
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

        public void Initialize(string title, string desc)
        {
            FillText(title, desc);
            this.PrimaryButtonText = "OK";
        }

        public void Initialize(string title, string desc, Dialog dialog, string username, bool askStore)
        {
            FillText(title, desc);
            TextBox1.Visibility = Visibility.Visible;
            TextBox1.PlaceholderText = Strings.Username;

            PasswordBox1.Visibility = Visibility.Visible;
            PasswordBox1.PlaceholderText = Strings.Password;

            if (askStore)
            {
                StoreToggleSwitch.Header = "Store credentials for next time";
                StoreToggleSwitch.Visibility = Visibility.Visible;
            }

            this.PrimaryButtonText = "Submit";
            this.PrimaryButtonClick += (d, eventArgs) =>
            {
                dialog.postLogin(TextBox1.Text, PasswordBox1.Password, StoreToggleSwitch.IsOn);
            };

            this.SecondaryButtonText = "Cancel";
            this.SecondaryButtonClick += (d, eventArgs) =>
            {
                dialog.dismiss();
            };
        }

        private void FillText(string title, string desc)
        {
            TitleTextBlock.Text = title;
            DescriptionTextBlock.Text = desc;
        }

        private void SetButtons()
        {

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
