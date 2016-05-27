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
using Windows.UI.Xaml.Input;

namespace VLC_WinRT.UI.Legacy.Views.UserControls
{
    public sealed partial class CommandBarBottom : CommandBar
    {
        public CommandBarBottom()
        {
            this.InitializeComponent();
            this.Loaded += CommandBarBottom_Loaded;
        }

        #region init
        private void CommandBarBottom_Loaded(object sender, RoutedEventArgs e)
        {
            UpdatePlayerVisibility();
            Locator.MusicPlayerVM.PropertyChanged += MusicPlayerVM_PropertyChanged;
            App.SplitShell.ContentSizeChanged += SplitShell_ContentSizeChanged;
        }

        private void SplitShell_ContentSizeChanged(double newWidth)
        {
            Responsive();
        }

        private void MusicPlayerVM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Locator.MusicPlayerVM.IsMiniPlayerVisible))
            {
                this.MiniPlayerVisibility = Locator.MusicPlayerVM.IsMiniPlayerVisible;
            }
        }
        #endregion

        #region properties
        public Visibility MiniPlayerVisibility
        {
            get { return (Visibility)GetValue(MiniPlayerVisibilityProperty); }
            set { SetValue(MiniPlayerVisibilityProperty, value); }
        }

        public static readonly DependencyProperty MiniPlayerVisibilityProperty =
            DependencyProperty.Register(nameof(MiniPlayerVisibility), typeof(Visibility), typeof(CommandBarBottom), new PropertyMetadata(Visibility.Collapsed, PlayerVisibilityChanged));

        private static void PlayerVisibilityChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var that = (CommandBarBottom)obj;
            that.UpdatePlayerVisibility();
        }

        public void UpdatePlayerVisibility()
        {
            NowPlayingArtistGrid.Visibility =
                PlayPreviousButton.Visibility =
                PlayNextButton.Visibility =
                MiniPlayerVisibility;

            var shuffleButton = FindName(nameof(ShuffleButton)) as FrameworkElement;
            if (shuffleButton != null)
                shuffleButton.Visibility = MiniPlayerVisibility;

            var repeatButton = FindName(nameof(RepeatButton)) as FrameworkElement;
            if (repeatButton != null)
                repeatButton.Visibility = MiniPlayerVisibility;

            var miniWindowButton = FindName(nameof(MiniWindowButton)) as FrameworkElement;
            if (miniWindowButton != null)
                miniWindowButton.Visibility = MiniPlayerVisibility;

            if (App.SplitShell.FooterVisibility != AppBarClosedDisplayMode.Hidden)
                App.SplitShell.FooterVisibility = MiniPlayerVisibility == Visibility.Visible ? AppBarClosedDisplayMode.Compact : AppBarClosedDisplayMode.Minimal;
        }

        #endregion

        #region interactions
        private void RootMiniPlayer_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Locator.MusicPlayerVM.GoToMusicPlayerPage.Execute(null);
        }
        #endregion

        void Responsive()
        {
            if (this.ActualWidth < 500)
            {
                TrackNameTextBlock.Visibility = ArtistNameTextBlock.Visibility = Visibility.Collapsed;
            }
            else
            {
                TrackNameTextBlock.Visibility = ArtistNameTextBlock.Visibility = Visibility.Visible;
            }

            if (this.ActualWidth < 700)
            {
                bool addedSeparator = false;
                foreach (var item in this.PrimaryCommands.ToList())
                {
                    if (((FrameworkElement)item).Tag?.ToString() == "sec")
                    {
                        if (!addedSeparator)
                        {
                            this.SecondaryCommands.Add(new AppBarSeparator());
                            addedSeparator = true;
                        }

                        this.PrimaryCommands.Remove(item);
                        if (item is AppBarButton)
                        {
                            var newButton = new AppBarButton();
                            newButton.Tag = ((FrameworkElement)item).Tag;
                            newButton.Name = ((FrameworkElement)item).Name;
                            newButton.Label = ((AppBarButton)item).Label;
                            newButton.Icon = ((AppBarButton)item).Icon;
                            newButton.Style = App.Current.Resources["AppBarTextButtonStyle"] as Style;
                            newButton.Command = ((AppBarButton)item).Command;
                            newButton.CommandParameter = ((AppBarButton)item).CommandParameter;

                            newButton.Width = 160;

                            this.SecondaryCommands.Add(newButton);
                        }
                        else if (item is AppBarToggleButton)
                        {
                            var newButton = new AppBarToggleButton();
                            newButton.Tag = ((FrameworkElement)item).Tag;
                            newButton.Name = ((FrameworkElement)item).Name;
                            newButton.Label = ((AppBarToggleButton)item).Label;
                            newButton.Icon = ((AppBarToggleButton)item).Icon;
                            newButton.Style = App.Current.Resources["AppBarToggleTextButtonStyle"] as Style;
                            newButton.Command = ((AppBarToggleButton)item).Command;
                            newButton.CommandParameter = ((AppBarToggleButton)item).CommandParameter;

                            newButton.Width = 160;

                            this.SecondaryCommands.Add(newButton);
                        }
                    }
                }
            }
            else
            {
                bool removedSeparator = false;
                foreach (var item in this.SecondaryCommands.ToList())
                {
                    if (((FrameworkElement)item).Tag?.ToString() == "sec")
                    {
                        if (!removedSeparator)
                        {
                            this.SecondaryCommands.Remove(this.SecondaryCommands.FirstOrDefault(x => x is AppBarSeparator));
                            removedSeparator = true;
                        }

                        this.SecondaryCommands.Remove(item);
                        if (item is AppBarButton)
                        {
                            var newButton = new AppBarButton();
                            newButton.Tag = ((FrameworkElement)item).Tag;
                            newButton.Name = ((FrameworkElement)item).Name;
                            newButton.Label = ((AppBarButton)item).Label;
                            newButton.Icon = ((AppBarButton)item).Icon;
                            newButton.Style = null;
                            newButton.Command = ((AppBarButton)item).Command;
                            newButton.CommandParameter = ((AppBarButton)item).CommandParameter;

                            this.PrimaryCommands.Add(newButton);
                        }
                        else if (item is AppBarToggleButton)
                        {
                            var newButton = new AppBarToggleButton();
                            newButton.Tag = ((FrameworkElement)item).Tag;
                            newButton.Name = ((FrameworkElement)item).Name;
                            newButton.Label = ((AppBarToggleButton)item).Label;
                            newButton.Icon = ((AppBarToggleButton)item).Icon;
                            newButton.Style = null;
                            newButton.Command = ((AppBarToggleButton)item).Command;
                            newButton.CommandParameter = ((AppBarToggleButton)item).CommandParameter;

                            this.PrimaryCommands.Add(newButton);
                        }
                    }
                }
            }
        }

        private void PlayButton_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            Locator.MediaPlaybackViewModel.Stop();
        }
    }
}
