using System;
using VLC_WinRT.Helpers;
using Windows.Graphics.Display;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using VLC_WinRT.ViewModels;

namespace VLC_WinRT.UI.Legacy.Views.UserControls
{
    public sealed partial class TitleBar : UserControl
    {
        public TitleBar()
        {
            this.InitializeComponent();
            this.Loaded += TitleBar_Loaded;
            Locator.SettingsVM.PropertyChanged += SettingsVM_PropertyChanged;
        }

        private void TitleBar_Loaded(object sender, RoutedEventArgs e)
        {
            Responsive();
        }

        private void SettingsVM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Locator.SettingsVM.AccentColor) 
                || e.PropertyName == nameof(Locator.SettingsVM.AccentColorTitleBar)
                || e.PropertyName == nameof(Locator.SettingsVM.ApplicationTheme))
            {
                Responsive();
            }
        }

        void Responsive()
        {
            if (Locator.SettingsVM.AccentColorTitleBar)
            {
                VLCLogo.Fill = Title.Foreground = ProgressRing.Foreground = InformationText.Foreground = new SolidColorBrush(Colors.White);
                if (Locator.SettingsVM.ApplicationTheme == ApplicationTheme.Light)
                {
                    RootGrid.Background = new SolidColorBrush(Locator.SettingsVM.AccentColor);
                }
                else
                {
                    var darkColor = new SolidColorBrush(Locator.SettingsVM.AccentColor);
                    darkColor.Opacity = 0.6;
                    RootGrid.Background = darkColor;
                }
            }
            else
            {
                VLCLogo.Fill = Title.Foreground = ProgressRing.Foreground = InformationText.Foreground = new SolidColorBrush(Locator.SettingsVM.AccentColor);
                if (Locator.SettingsVM.ApplicationTheme == ApplicationTheme.Light)
                {
                    RootGrid.Background = new SolidColorBrush(Color.FromArgb(0xf2, 0xe8, 0xe8, 0xe8));
                }
                else
                {
                    RootGrid.Background = new SolidColorBrush(Color.FromArgb(0xd9, 0x05, 0x05, 0x05));
                }
            }
        }
    }
}
