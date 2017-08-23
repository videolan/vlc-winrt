//using libVLCX;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Runtime.InteropServices.WindowsRuntime;
//using System.Threading;
//using System.Threading.Tasks;
//using VLC.Utils;
//using VLC.ViewModels;
//using Windows.Foundation;
//using Windows.Foundation.Collections;
//using Windows.System;
//using Windows.UI.Xaml;
//using Windows.UI.Xaml.Controls;
//namespace VLC.Universal8._1.Views.UserControls.Shell
//{
//    public sealed partial class VLCDialog : ContentDialog
//    {
//        static SemaphoreSlim SingleDialogSem = new SemaphoreSlim(1);
//        Dialog Dialog;

//        private void Dialog_Closed(Windows.UI.Xaml.Controls.ContentDialog sender, Windows.UI.Xaml.Controls.ContentDialogClosedEventArgs args)
//        {
//            Dialog?.dismiss();
//            SingleDialogSem.Release();
//        }

//        static public async Task WaitForDialogLock()
//        {
//            await SingleDialogSem.WaitAsync();
//        }

//        private void CommonInit(string title, string desc, Dialog dialog)
//        {
//            this.InitializeComponent();
//            TitleTextBlock.Text = title;
//            DescriptionTextBlock.Text = desc;
//            this.Closed += Dialog_Closed;
//            Dialog = dialog;
//        }

//        public VLCDialog(string title, string desc)
//        {
//            CommonInit(title, desc, null);
//            this.PrimaryButtonText = "OK";
//        }

//        public VLCDialog(string title, string desc, Dialog dialog, string username, bool askStore)
//        {
//            CommonInit(title, desc, dialog);
//            TextBox1.Visibility = Visibility.Visible;
//            TextBox1.PlaceholderText = Strings.Username;

//            PasswordBox1.Visibility = Visibility.Visible;
//            PasswordBox1.PlaceholderText = Strings.Password;

//            if (askStore)
//            {
//                StoreToggleSwitch.Header = "Store credentials for next time";
//                StoreToggleSwitch.Visibility = Visibility.Visible;
//            }

//            this.PrimaryButtonText = "Submit";
//            this.PrimaryButtonClick += (d, eventArgs) =>
//            {
//                Dialog?.postLogin(TextBox1.Text, PasswordBox1.Password, StoreToggleSwitch.IsOn);
//                Dialog = null;
//            };

//            this.SecondaryButtonText = "Cancel";
//            this.SecondaryButtonClick += (d, eventArgs) =>
//            {
//                Dialog?.dismiss();
//                Dialog = null;
//            };
//        }

//        public VLCDialog(string title, string desc, Dialog dialog, Question questionType, string cancel, string action1, string action2)
//        {
//            CommonInit(title, desc, dialog);

//            this.PrimaryButtonText = action1;
//            this.PrimaryButtonClick += (d, eventArgs) =>
//            {
//                Dialog?.postAction(1);
//                Dialog = null;
//            };
//            this.SecondaryButtonText = action2;
//            this.SecondaryButtonClick += (d, eventArgs) =>
//            {
//                Dialog?.postAction(2);
//                Dialog = null;
//            };
//        }

//        public void Cancel()
//        {
//            Dialog?.dismiss();
//            Dialog = null;
//            Hide();
//        }

//        private void PasswordBox1_GotFocus(object sender, RoutedEventArgs e)
//        {
//            Locator.MainVM.KeyboardListenerService.CanListen = false;
//        }

//        private void PasswordBox1_LostFocus(object sender, RoutedEventArgs e)
//        {
//            Locator.MainVM.KeyboardListenerService.CanListen = true;
//        }

//        private void UsernameBox_KeyUp(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
//        {
//            if (e.Key == VirtualKey.Enter)
//                PasswordBox1.Focus(FocusState.Programmatic);
//        }
//        private void PasswordBox_KeyUp(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
//        {
//            if (e.Key == VirtualKey.Enter)
//                StoreToggleSwitch.Focus(FocusState.Programmatic);
//        }
//    }
//}
