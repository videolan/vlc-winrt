/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using VLC.ViewModels;
using Windows.Devices.Haptics;
using Windows.UI.Input;

namespace VLC.Services.RunTime
{
    public class RadialControllerService
    {
        RadialController controller;
        RadialControllerConfiguration config;
        RadialControllerMenuItem volumeItem;
        RadialControllerMenuItem playbackItem;

        MediaPlaybackViewModel vm => Locator.MediaPlaybackViewModel;

        enum Mode { Volume, Playback };
        public enum MediaMode { Music, Video };
        Mode currentMode;
        bool doNotProcessClick = false;

        const string volumeTag = "volume";
        const string videoPlaybackTag = "playback";

        public void Start(MediaMode mediaMode)
        {
            controller = RadialController.CreateForCurrentView();
            controller.RotationResolutionInDegrees = 5;
            controller.UseAutomaticHapticFeedback = false;

            volumeItem = RadialControllerMenuItem.CreateFromFontGlyph("Volume", "\xE767", "Segoe MDL2 Assets");
            volumeItem.Tag = volumeTag;
            playbackItem = RadialControllerMenuItem.CreateFromFontGlyph("Playback Speed", "\xE714", "Segoe MDL2 Assets");
            playbackItem.Tag = videoPlaybackTag;

            volumeItem.Invoked += Item_Invoked;
            playbackItem.Invoked += Item_Invoked;

            controller.Menu.Items.Add(volumeItem);
            controller.Menu.Items.Add(playbackItem);

            controller.RotationChanged += controller_RotationChanged;
            controller.ButtonClicked += controller_ButtonClicked;

            // Remove default set of items.
            config = RadialControllerConfiguration.GetForCurrentView();
            config.SetDefaultMenuItems(new RadialControllerSystemMenuItemKind[] { });

            config.ActiveControllerWhenMenuIsSuppressed = controller;
            controller.ButtonHolding += controller_ButtonHolding;
        }

        public void Stop()
        {

        }

        private void controller_ButtonHolding(RadialController sender, RadialControllerButtonHoldingEventArgs args)
        {
            switch (currentMode)
            {
                case Mode.Volume:
                    break;
                case Mode.Playback:
                    break;
            }
        }

        private void controller_ButtonClicked(RadialController sender, RadialControllerButtonClickedEventArgs args)
        {
            switch (currentMode)
            {
                case Mode.Volume:
                    //vm.Volume = vm.VOLUME_MIN;
                    break;
                case Mode.Playback:
                    vm.PlayOrPauseCommand.Execute(null);
                    break;
            }
        }

        private void controller_RotationChanged(RadialController sender, RadialControllerRotationChangedEventArgs args)
        {
            switch (currentMode)
            {
                case Mode.Volume:
                    var volume = vm.Volume + ((args.RotationDeltaInDegrees) / 5);
                    Debug.WriteLine($"Current Volume: {vm.Volume}, new volume {volume} plus {((args.RotationDeltaInDegrees) / 5)}");
                    if (volume > vm.VOLUME_MAX)
                        vm.Volume = vm.VOLUME_MAX;
                    else if (volume < vm.VOLUME_MIN)
                        vm.Volume = vm.VOLUME_MIN;
                    else
                        Debug.WriteLine($"Volume: {(int)volume}");
                        vm.Volume = (int)volume;
                    break;
                case Mode.Playback:
                    var speedRate = vm.SpeedRate + (int)((args.RotationDeltaInDegrees) / 5);
                    if (speedRate <= 0)
                        vm.SpeedRate = 0;
                    else
                        vm.SpeedRate = speedRate;
                    break;
            }
        }

        private void Item_Invoked(RadialControllerMenuItem sender, object args)
        {
            switch (sender.Tag)
            {
                case volumeTag:
                    currentMode = Mode.Volume;
                    break;
                case videoPlaybackTag:
                    currentMode = Mode.Playback;
                    break;
            }
        }

        private void SendBuzzFeedback(SimpleHapticsController hapticController)
        {
            var feedbacks = hapticController.SupportedFeedback;

            foreach (SimpleHapticsControllerFeedback feedback in feedbacks)
            {
                if (feedback.Waveform == KnownSimpleHapticsControllerWaveforms.Click)
                {
                    //Click the RadialController 3 times, with a duration of 250ms between each click
                    hapticController.SendHapticFeedbackForPlayCount(feedback, 1, 3, TimeSpan.FromMilliseconds(250));
                    return;
                }
            }
        }

    }
}
