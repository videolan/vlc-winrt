using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Core;
using VLC.Model;
using VLC.ViewModels;
using libVLCX;
using VLC.Database;
using VLC.Helpers;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml;
using System;
using Windows.Foundation;

namespace VLC.Services.RunTime
{
    public class KeyboardListenerService
    {
        public event TypedEventHandler<CoreWindow, KeyEventArgs> KeyDownPressed;
        public KeyboardActionDatabase _keyboardActionDatabase = new KeyboardActionDatabase();

        private const uint MaxVirtualKeys = 3;
        private VirtualKey[] virtualKeys = new VirtualKey[MaxVirtualKeys];
        public bool CanListen { get; set; }
        public KeyboardListenerService()
        {
            CanListen = true;
            CoreWindow.GetForCurrentThread().KeyUp += KeyboardListenerService_KeyUp;
            CoreWindow.GetForCurrentThread().KeyDown += KeyboardListenerService_KeyDown;
            InitializeDefault();
        }

        void InitializeDefault()
        {
            if (_keyboardActionDatabase.IsEmpty())
            {
                var actionsToSet = new List<KeyboardAction>()
                {
                    new KeyboardAction()
                    {
                        Action = VLCAction.FullscreenToggle,
                        MainKey = VirtualKey.F
                    },
                    new KeyboardAction()
                    {
                        Action = VLCAction.LeaveFullscreen,
                        MainKey = VirtualKey.Escape,
                    },
                    new KeyboardAction()
                    {
                        Action = VLCAction.PauseToggle,
                        MainKey = VirtualKey.Space
                    },
                    new KeyboardAction()
                    {
                        Action = VLCAction.Faster,
                        MainKey = VirtualKey.Add
                    },
                    new KeyboardAction()
                    {
                        Action = VLCAction.Slow,
                        MainKey = VirtualKey.Subtract,
                    },
                    new KeyboardAction()
                    {
                        Action = VLCAction.NormalRate,
                        MainKey = VirtualKey.Execute,
                    },
                    new KeyboardAction()
                    {
                        Action = VLCAction.Next,
                        MainKey = VirtualKey.N,
                    },
                    new KeyboardAction()
                    {
                        Action = VLCAction.Previous,
                        MainKey = VirtualKey.P,
                    },
                    new KeyboardAction()
                    {
                        Action = VLCAction.Stop,
                        MainKey = VirtualKey.S,
                    },
                    new KeyboardAction()
                    {
                        Action = VLCAction.Quit,
                        MainKey = VirtualKey.Q,
                    },
                    new KeyboardAction()
                    {
                        Action = VLCAction.Back,
                        MainKey = VirtualKey.Back
                    },
                    new KeyboardAction()
                    {
                        Action = VLCAction.VolumeUp,
                        MainKey = VirtualKey.Control,
                        SecondKey = VirtualKey.Add,
                    },
                    new KeyboardAction()
                    {
                        Action = VLCAction.VolumeDown,
                        MainKey = VirtualKey.Control,
                        SecondKey = VirtualKey.Subtract,
                    },
                    new KeyboardAction()
                    {
                        Action = VLCAction.Mute,
                        MainKey = VirtualKey.M,
                    },
                    new KeyboardAction()
                    {
                        Action = VLCAction.ChangeAudioTrack,
                        MainKey = VirtualKey.B,
                    },
                    new KeyboardAction()
                    {
                        Action = VLCAction.ChangeSubtitle,
                        MainKey = VirtualKey.V
                    },
                    new KeyboardAction()
                    {
                        Action = VLCAction.OpenFile,
                        MainKey = VirtualKey.Control,
                        SecondKey = VirtualKey.O,
                    },
                    new KeyboardAction()
                    {
                        Action = VLCAction.OpenNetwork,
                        MainKey = VirtualKey.Control,
                        SecondKey = VirtualKey.N
                    },
                    new KeyboardAction()
                    {
                        Action = VLCAction.TabNext,
                        MainKey = VirtualKey.Control,
                        SecondKey = VirtualKey.Tab,
                    }
                };
                _keyboardActionDatabase.AddKeyboardActions(actionsToSet);
            }
        }

        void KeyboardListenerService_KeyUp(CoreWindow sender, KeyEventArgs args)
        {
            if (!CanListen) return;
            for (int i = 0; i < MaxVirtualKeys; i++)
            {
                if (virtualKeys[i] == args.VirtualKey)
                {
                    virtualKeys[i] = VirtualKey.None;
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

        async void KeyboardListenerService_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
            if (!CanListen) return;

            KeyDownPressed?.Invoke(sender, args);
            // Guidelines:
            // If first VirtualKey is Ctrl, Alt, or Shift, then we're waiting for another key
            var i = 0;
            while (i < MaxVirtualKeys && virtualKeys[i] != VirtualKey.None)
            {
                i++;
            }

            if (i == MaxVirtualKeys)
                virtualKeys = new VirtualKey[3];
            else
                virtualKeys[i] = args.VirtualKey;

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
                    // look in the db for a match
                    var action = await _keyboardActionDatabase.GetKeyboardAction(virtualKeys[0], virtualKeys[1]);
                    if (action != null)
                    {
                        // if there's a match, get the ActionId
                        await DoKeyboardAction(action);
                    }
                    else
                    {
                        if (Locator.NavigationService.CurrentPage == VLCPage.VideoPlayerPage)
                        {
                            switch (args.VirtualKey)
                            {
                                case VirtualKey.GamepadA:
                                case VirtualKey.GamepadLeftThumbstickButton:
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
                                    Locator.VideoPlayerVm.RequestChangeControlBarVisibility(true);
                                    Locator.MediaPlaybackViewModel.FastSeekCommand.Execute(-5000);
                                    break;
                                case VirtualKey.GamepadLeftTrigger:
                                    Locator.VideoPlayerVm.RequestChangeControlBarVisibility(true);
                                    Locator.MediaPlaybackViewModel.FastSeekCommand.Execute(-30000);
                                    break;
                                case VirtualKey.GamepadRightShoulder:
                                    Locator.VideoPlayerVm.RequestChangeControlBarVisibility(true);
                                    Locator.MediaPlaybackViewModel.FastSeekCommand.Execute(5000);
                                    break;
                                case VirtualKey.GamepadRightTrigger:
                                    Locator.VideoPlayerVm.RequestChangeControlBarVisibility(true);
                                    Locator.MediaPlaybackViewModel.FastSeekCommand.Execute(30000);
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    break;
            }
        }

        async Task DoKeyboardAction(KeyboardAction keyboardAction)
        {
            // determine if it's a combination of keys or not

            if (virtualKeys[1] == VirtualKey.None && virtualKeys[2] == VirtualKey.None)
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
            else if (virtualKeys[2] == VirtualKey.None)
            {
                // two keys shortcut
                if (virtualKeys[0] == VirtualKey.Control && virtualKeys[1] == keyboardAction.SecondKey)
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
