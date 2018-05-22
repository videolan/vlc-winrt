using System.Collections.Generic;
using System.Diagnostics;
using Windows.System;
using Windows.UI.Core;
using VLC.Model;
using VLC.ViewModels;
using libVLCX;
using VLC.Helpers;
using System.Linq;
using Windows.Foundation;

namespace VLC.Services.RunTime
{
    public class KeyboardListenerService
    {
        private const uint MaxVirtualKeys = 3;
        private VirtualKey[] _virtualKeys = new VirtualKey[MaxVirtualKeys];
        
        public KeyboardListenerService()
        {
            CanListen = true;
            CoreWindow.GetForCurrentThread().KeyUp += KeyboardListenerService_KeyUp;
            CoreWindow.GetForCurrentThread().KeyDown += KeyboardListenerService_KeyDown;
            InitializeDefaultShortcuts();
        }

        public List<KeyboardAction> Shortcuts = new List<KeyboardAction>();

        public bool CanListen { get; set; }

        public event TypedEventHandler<CoreWindow, KeyEventArgs> KeyDownPressed;

        void InitializeDefaultShortcuts()
        {
            Shortcuts = new List<KeyboardAction>
            {
                new KeyboardAction
                {
                    Action = VLCAction.FullscreenToggle,
                    MainKey = VirtualKey.F
                },
                new KeyboardAction
                {
                    Action = VLCAction.LeaveFullscreen,
                    MainKey = VirtualKey.Escape
                },
                new KeyboardAction
                {
                    Action = VLCAction.PauseToggle,
                    MainKey = VirtualKey.Space
                },
                new KeyboardAction
                {
                    Action = VLCAction.Faster,
                    MainKey = VirtualKey.Add ,
                    KeyCode = 0xBB //OEM plus
                },
                new KeyboardAction
                {
                    Action = VLCAction.Slow,
                    MainKey = VirtualKey.Subtract,
                    KeyCode = 0xBD //OEM minus
                },
                new KeyboardAction
                {
                    Action = VLCAction.NormalRate,
                    MainKey = VirtualKey.Execute
                },
                new KeyboardAction
                {
                    Action = VLCAction.Next,
                    MainKey = VirtualKey.N
                },
                new KeyboardAction
                {
                    Action = VLCAction.Previous,
                    MainKey = VirtualKey.P
                },
                new KeyboardAction
                {
                    Action = VLCAction.Stop,
                    MainKey = VirtualKey.S
                },
                new KeyboardAction
                {
                    Action = VLCAction.Quit,
                    MainKey = VirtualKey.Q
                },
                new KeyboardAction
                {
                    Action = VLCAction.Back,
                    MainKey = VirtualKey.Back
                },
                new KeyboardAction
                {
                    Action = VLCAction.VolumeUp,
                    MainKey = VirtualKey.Control,
                    SecondKey = VirtualKey.Add,
                    KeyCode = 0xBB //OEM plus
                },
                new KeyboardAction
                {
                    Action = VLCAction.VolumeDown,
                    MainKey = VirtualKey.Control,
                    SecondKey = VirtualKey.Subtract,
                    KeyCode = 0xBD //OEM minus
                },
                new KeyboardAction
                {
                    Action = VLCAction.Mute,
                    MainKey = VirtualKey.M
                },
                new KeyboardAction
                {
                    Action = VLCAction.ChangeAudioTrack,
                    MainKey = VirtualKey.B
                },
                new KeyboardAction
                {
                    Action = VLCAction.ChangeSubtitle,
                    MainKey = VirtualKey.V
                },
                new KeyboardAction
                {
                    Action = VLCAction.OpenFile,
                    MainKey = VirtualKey.Control,
                    SecondKey = VirtualKey.O
                },
                new KeyboardAction
                {
                    Action = VLCAction.OpenNetwork,
                    MainKey = VirtualKey.Control,
                    SecondKey = VirtualKey.N
                },
                new KeyboardAction
                {
                    Action = VLCAction.TabNext,
                    MainKey = VirtualKey.Control,
                    SecondKey = VirtualKey.Tab
                }
            };
        }

        void KeyboardListenerService_KeyUp(CoreWindow sender, KeyEventArgs args)
        {
            if (!CanListen) return;
            for (int i = 0; i < MaxVirtualKeys; i++)
            {
                if (_virtualKeys[i] == args.VirtualKey)
                {
                    _virtualKeys[i] = VirtualKey.None;
                }
            }
            switch (args.VirtualKey)
            {
                case VirtualKey.GamepadMenu:
                case VirtualKey.F1:
                    if (Locator.SettingsVM.MediaCenterMode)
                    {
                        if (Locator.NavigationService.CurrentPage != VLCPage.MainPageXBOX)
                            Locator.NavigationService.Go(VLCPage.MainPageXBOX);
                        else
                            Locator.NavigationService.GoBack_HideFlyout();
                    }
                    break;
            }
        }

        void KeyboardListenerService_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
            if (!CanListen || args.Handled) return;

            KeyDownPressed?.Invoke(sender, args);
            // Guidelines:
            // If first VirtualKey is Ctrl, Alt, or Shift, then we're waiting for another key
            var i = 0;
            while (i < MaxVirtualKeys && _virtualKeys[i] != VirtualKey.None)
            {
                i++;
            }

            if (i == MaxVirtualKeys)
                _virtualKeys = new VirtualKey[3];
            else
                _virtualKeys[i] = args.VirtualKey;

            switch (args.VirtualKey)
            {
                case VirtualKey.Control:
                    Debug.WriteLine("Ctrl key was pressed, waiting another key ...");
                    break;
                case VirtualKey.Shift:
                    Debug.WriteLine("Shift key was pressed, waiting another key ...");
                    break;
                default:
                    Debug.WriteLine($"{args.VirtualKey} key was pressed");
                   
                    var action = Shortcuts.FirstOrDefault(x => 
                        (x.MainKey == _virtualKeys[0] || x.KeyCode == (int)_virtualKeys[0]) && 
                        (x.SecondKey == _virtualKeys[1] || x.KeyCode == (int)_virtualKeys[1]));

                    if (_virtualKeys.All(key => key == VirtualKey.None)) return; 

                    if (action != null)
                    {
                        // if there's a match, get the ActionId
                        DoKeyboardAction(action);
                    }
                    else
                    {
                        if (Locator.NavigationService.CurrentPage == VLCPage.VideoPlayerPage)
                        {
                            switch (args.VirtualKey)
                            {
                                case VirtualKey.GamepadLeftThumbstickButton:
                                case VirtualKey.GamepadRightThumbstickButton:
                                    if (!Locator.VideoPlayerVm.Is3DVideo) break;
                                    var vp = new VideoViewpoint(0f, 0f, 0f,
                                        args.VirtualKey == VirtualKey.GamepadRightThumbstickButton ? -0.5f : 0.5f);
                                    Locator.PlaybackService.UpdateViewpoint(vp, false);
                                    break;
                                case VirtualKey.GamepadA:
                                case VirtualKey.GamepadDPadDown:
                                case VirtualKey.GamepadDPadLeft:
                                case VirtualKey.GamepadDPadUp:
                                case VirtualKey.GamepadDPadRight:
                                case VirtualKey.GamepadLeftThumbstickUp:
                                case VirtualKey.GamepadLeftThumbstickDown:
                                case VirtualKey.GamepadLeftThumbstickRight:
                                case VirtualKey.GamepadLeftThumbstickLeft:
                                    Locator.VideoPlayerVm.RequestChangeControlBarVisibility(true);
                                    break;
                                case VirtualKey.GamepadY:
                                    Locator.MediaPlaybackViewModel.PlayOrPauseCommand.Execute(null);
                                    break;
                                case VirtualKey.GamepadLeftShoulder:
                                case VirtualKey.Left:
                                    Locator.VideoPlayerVm.RequestExtendCurrentControlBarVisibility();
                                    Locator.MediaPlaybackViewModel.FastSeekCommand.Execute(-5000);
                                    break;
                                case VirtualKey.GamepadLeftTrigger:
                                    Locator.VideoPlayerVm.RequestExtendCurrentControlBarVisibility();
                                    Locator.MediaPlaybackViewModel.FastSeekCommand.Execute(-30000);
                                    break;
                                case VirtualKey.GamepadRightShoulder:
                                case VirtualKey.Right:
                                    Locator.VideoPlayerVm.RequestExtendCurrentControlBarVisibility();
                                    Locator.MediaPlaybackViewModel.FastSeekCommand.Execute(5000);
                                    break;
                                case VirtualKey.GamepadRightTrigger:
                                    Locator.VideoPlayerVm.RequestExtendCurrentControlBarVisibility();
                                    Locator.MediaPlaybackViewModel.FastSeekCommand.Execute(30000);
                                    break;
                                case VirtualKey.GamepadRightThumbstickDown:
                                    if (!Locator.VideoPlayerVm.Is3DVideo) break;
                                    Locator.PlaybackService.UpdateViewpoint(new VideoViewpoint(0f, 5f, 0f, 0f), false);
                                    break;
                                case VirtualKey.GamepadRightThumbstickUp:
                                    if (!Locator.VideoPlayerVm.Is3DVideo) break;
                                    Locator.PlaybackService.UpdateViewpoint(new VideoViewpoint(0f, -5f, 0f, 0f), false);
                                    break;
                                case VirtualKey.GamepadRightThumbstickLeft:
                                    if (!Locator.VideoPlayerVm.Is3DVideo) break;
                                    Locator.PlaybackService.UpdateViewpoint(new VideoViewpoint(-5f, 0f, 0f, 0f), false);
                                    break;
                                case VirtualKey.GamepadRightThumbstickRight:
                                    if (!Locator.VideoPlayerVm.Is3DVideo) break;
                                    Locator.PlaybackService.UpdateViewpoint(new VideoViewpoint(5f, 0f, 0f, 0f), false);
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    break;
            }
        }

        void DoKeyboardAction(KeyboardAction keyboardAction)
        {
            // determine if it's a combination of keys or not

            if (_virtualKeys[1] == VirtualKey.None && _virtualKeys[2] == VirtualKey.None)
            {
                // this is a simple shortcut
                switch (keyboardAction.Action)
                {
                    case VLCAction.FullscreenToggle:
                        AppViewHelper.ToggleFullscreen();
                        break;
                    case VLCAction.LeaveFullscreen:
                        AppViewHelper.LeaveFullscreen();
                        break;
                    case VLCAction.PauseToggle:
                        Locator.MediaPlaybackViewModel.PlaybackService.Pause();
                        break;
                    case VLCAction.Quit:
                        App.Current.Exit();
                        break;
                    case VLCAction.Stop:
                        if (Locator.MediaPlaybackViewModel.PlaybackService.PlayingType == PlayingType.Video)
                        {
                            Locator.MediaPlaybackViewModel.GoBack.Execute(null);
                        }
                        break;
                    case VLCAction.Previous:
                        Locator.PlaybackService.Previous();
                        break;
                    case VLCAction.Next:
                        Locator.PlaybackService.Next();
                        break;
                    case VLCAction.Faster:
                        Locator.MediaPlaybackViewModel.ChangePlaybackSpeedRateCommand.Execute("faster");
                        break;
                    case VLCAction.Slow:
                        Locator.MediaPlaybackViewModel.ChangePlaybackSpeedRateCommand.Execute("slower");
                        break;
                    case VLCAction.NormalRate:
                        Locator.MediaPlaybackViewModel.ChangePlaybackSpeedRateCommand.Execute("reset");
                        break;
                    case VLCAction.Mute:
                        Locator.MediaPlaybackViewModel.ChangeVolumeCommand.Execute("mute");
                        break;
                    case VLCAction.Back:
                        Locator.NavigationService.GoBack_Specific();
                        break;
                }
            }
            else if (_virtualKeys[2] == VirtualKey.None)
            {
                // two keys shortcut
                if (_virtualKeys[0] == VirtualKey.Control 
                    && (_virtualKeys[1] == keyboardAction.SecondKey || (int)_virtualKeys[1] == keyboardAction.KeyCode))
                {
                    //two keys shortcut, first key is Ctrl
                    switch (keyboardAction.Action)
                    {
                        case VLCAction.OpenFile:
                            {
                                Locator.MediaPlaybackViewModel.PickMediaCommand.Execute(null);
                            }
                            break;
                        case VLCAction.OpenNetwork:
                            {
                                Locator.MainVM.GoToStreamPanel.Execute(null);
                            }
                            break;
                        case VLCAction.VolumeUp:
                            Locator.MediaPlaybackViewModel.ChangeVolumeCommand.Execute("higher");
                            break;
                        case VLCAction.VolumeDown:
                            Locator.MediaPlaybackViewModel.ChangeVolumeCommand.Execute("lower");
                            break;
                        case VLCAction.TabNext:
                            var pivotIndex = Locator.MainVM.Panels.IndexOf(Locator.MainVM.CurrentPanel);
                            pivotIndex = (pivotIndex < Locator.MainVM.Panels.Count - 1) ? ++pivotIndex : 0;
                            Locator.NavigationService.Go(Locator.MainVM.Panels[pivotIndex].Target);
                            break;
                    }
                }
            }
        }
    }
}
