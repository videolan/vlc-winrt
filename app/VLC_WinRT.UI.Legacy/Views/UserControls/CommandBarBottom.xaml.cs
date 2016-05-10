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
                SliderHorizontal.Visibility =
                PlayPreviousButton.Visibility =
                PlayButton.Visibility =
                PauseButton.Visibility =
                PlayNextButton.Visibility =
                ShuffleButton.Visibility =
                RepeatButton.Visibility =
                ShuffleMiniButton.Visibility =
                MiniWindowButton.Visibility =
                MiniPlayerVisibility;

            this.ClosedDisplayMode = 
                MiniPlayerVisibility == Visibility.Visible ? 
                    AppBarClosedDisplayMode.Compact : AppBarClosedDisplayMode.Minimal;
        }

        #endregion

        #region interactions
        private void RootMiniPlayer_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Locator.MusicPlayerVM.GoToMusicPlayerPage.Execute(null);
        }
        #endregion
    }
}
