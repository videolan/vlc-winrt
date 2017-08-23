using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using VLC.Helpers;
using VLC.ViewModels;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace VLC.Universal8._1.Views.UserControls
{
    public sealed partial class CommandBarBottom : CommandBar
    {
        public CommandBarBottom()
        {
            this.InitializeComponent();

            //if (!DynamicApisAvailable) return;

            //RootCommandBar.IsDynamicOverflowEnabled = true;
            //PlayPreviousButton.DynamicOverflowOrder = 2;
            //PlayPauseButton.DynamicOverflowOrder = 3;
            //PlayNextButton.DynamicOverflowOrder = 2;
            //ShuffleButton.DynamicOverflowOrder = 1;
            //MiniWindowButton.DynamicOverflowOrder = 1;
        }

        #region interactions
        private void RootMiniPlayer_Clicked(object sender, RoutedEventArgs e)
        {
            Locator.MusicPlayerVM.GoToMusicPlayerPage.Execute(null);
        }
        #endregion

        private void PlayButton_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            StopPlayback();
        }

        private void PauseButton_Holding(object sender, HoldingRoutedEventArgs e)
        {
            StopPlayback();
        }

        public void StopPlayback()
        {
            Locator.PlaybackService.Stop();
            Locator.PlaybackService.ClearPlaylist();
        }

        //private bool DynamicApisAvailable => ApiInformation.IsApiContractPresent(
        //    "Windows.Foundation.UniversalApiContract", 3);
    }
}
