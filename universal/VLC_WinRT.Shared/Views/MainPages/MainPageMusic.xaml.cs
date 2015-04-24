/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using VLC_WinRT.ViewModels;
using Microsoft.Xaml.Interactivity;

namespace VLC_WinRT.Views.MainPages
{
    public sealed partial class MainPageMusic : Page
    {
        public MainPageMusic()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        private void MusicPanesFrame_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (MainPageMusicContentPresenter.Content == null)
            {
                Locator.MainVM.ChangeMainPageMusicViewCommand.Execute((int)Locator.SettingsVM.MusicView);
            }
            Responsive(Window.Current.Bounds.Width);
            Window.Current.SizeChanged += Current_SizeChanged;
            this.Unloaded += MusicPaneButtons_Unloaded;
        }

        void Current_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            Responsive(e.Size.Width);
        }

        void MusicPaneButtons_Unloaded(object sender, RoutedEventArgs e)
        {
            Window.Current.SizeChanged -= Current_SizeChanged;
        }

        void Responsive(double width)
        {
            if (width <= 650)
                VisualStateUtilities.GoToState(this, "Minimal", false);
            else
                VisualStateUtilities.GoToState(this, "Normal", false);
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(MusicSearchBox.Text) && !string.IsNullOrEmpty(Locator.MusicLibraryVM.SearchTag))
                Locator.MainVM.ChangeMainPageMusicViewCommand.Execute((int)Locator.SettingsVM.MusicView);
            Locator.MusicLibraryVM.SearchTag = MusicSearchBox.Text;
        }
    }
}
