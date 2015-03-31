using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Xaml.Interactivity;
using VLC_WinRT.ViewModels;
#if WINDOWS_PHONE_APP
using Windows.Phone.UI.Input;
#endif

namespace VLC_WinRT.Views.MainPages
{
    public sealed partial class MainPageRemovables : Page
    {
        public MainPageRemovables()
        {
            this.InitializeComponent();
            SizeChanged += OnSizeChanged;
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Responsive();
            this.Unloaded += OnUnloaded;
#if WINDOWS_PHONE_APP
            HardwareButtons.BackPressed += HardwareButtonsOnBackPressed;
#endif
        }

        private void OnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
#if WINDOWS_PHONE_APP
            HardwareButtons.BackPressed -= HardwareButtonsOnBackPressed;
#endif
        }
#if WINDOWS_PHONE_APP
        private void HardwareButtonsOnBackPressed(object sender, BackPressedEventArgs backPressedEventArgs)
        {
            if (Locator.ExternalStorageVM.CurrentStorageVM.CanGoBack)
            {
                backPressedEventArgs.Handled = true;
                Locator.ExternalStorageVM.CurrentStorageVM.GoBackCommand.Execute(null);
            }
            else
            {
            }
        }
#endif
        private void OnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
            Responsive();
        }

        void Responsive()
        {
            if (Window.Current.Bounds.Width > 700)
            {
                VisualStateUtilities.GoToState(this, "Horizontal", false);
            }
            else
            {
                VisualStateUtilities.GoToState(this, "Vertical", false);
            }
        }
    }
}
