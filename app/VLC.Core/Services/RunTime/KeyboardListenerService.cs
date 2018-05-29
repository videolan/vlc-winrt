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
        public KeyboardListenerService()
        {
            CanListen = true;
            CoreWindow coreWindow = CoreWindow.GetForCurrentThread();
            coreWindow.KeyUp += KeyboardListenerService_KeyUp;
            coreWindow.KeyDown += KeyboardListenerService_KeyDown;
            _shortcuts = CreateDefaultShortcuts();
        }

        readonly Dictionary<(VirtualKey, VirtualKeyModifiers), KeyboardAction> _shortcuts;

        public IReadOnlyDictionary<(VirtualKey, VirtualKeyModifiers), KeyboardAction> Shortcuts => _shortcuts;

        public bool CanListen { get; set; }

        public event TypedEventHandler<CoreWindow, KeyEventArgs> KeyDownPressed;

        Dictionary<(VirtualKey, VirtualKeyModifiers), KeyboardAction> CreateDefaultShortcuts()
        {
            var shortcuts = new List<KeyboardAction>
            {
                new KeyboardAction
                {
                    Action = VLCAction.FullscreenToggle,
                    Key = VirtualKey.F
                },
                new KeyboardAction
                {
                    Action = VLCAction.LeaveFullscreen,
                    Key = VirtualKey.Escape
                },
                new KeyboardAction
                {
                    Action = VLCAction.PauseToggle,
                    Key = VirtualKey.Space
                },
                new KeyboardAction
                {
                    Action = VLCAction.Faster,
                    Key = VirtualKey.Add ,
                    KeyCode = 0xBB //OEM plus
                },
                new KeyboardAction
                {
                    Action = VLCAction.Slow,
                    Key = VirtualKey.Subtract,
                    KeyCode = 0xBD //OEM minus
                },
                new KeyboardAction
                {
                    Action = VLCAction.NormalRate,
                    Key = VirtualKey.Execute
                },
                new KeyboardAction
                {
                    Action = VLCAction.Next,
                    Key = VirtualKey.N
                },
                new KeyboardAction
                {
                    Action = VLCAction.Previous,
                    Key = VirtualKey.P
                },
                new KeyboardAction
                {
                    Action = VLCAction.Stop,
                    Key = VirtualKey.S
                },
                new KeyboardAction
                {
                    Action = VLCAction.Quit,
                    Key = VirtualKey.Q
                },
                new KeyboardAction
                {
                    Action = VLCAction.Back,
                    Key = VirtualKey.Back
                },
                new KeyboardAction
                {
                    Action = VLCAction.Back,
                    Key = VirtualKey.GoBack
                },
                new KeyboardAction
                {
                    Action = VLCAction.VolumeUp,
                    Key = VirtualKey.Add,
                    Modifiers = VirtualKeyModifiers.Control,
                    KeyCode = 0xBB //OEM plus
                },
                new KeyboardAction
                {
                    Action = VLCAction.VolumeDown,
                    Key = VirtualKey.Subtract,
                    Modifiers = VirtualKeyModifiers.Control,
                    KeyCode = 0xBD //OEM minus
                },
                new KeyboardAction
                {
                    Action = VLCAction.Mute,
                    Key = VirtualKey.M
                },
                new KeyboardAction
                {
                    Action = VLCAction.ChangeAudioTrack,
                    Key = VirtualKey.B
                },
                new KeyboardAction
                {
                    Action = VLCAction.ChangeSubtitle,
                    Key = VirtualKey.V
                },
                new KeyboardAction
                {
                    Action = VLCAction.OpenFile,
                    Key = VirtualKey.O,
                    Modifiers = VirtualKeyModifiers.Control
                },
                new KeyboardAction
                {
                    Action = VLCAction.OpenNetwork,
                    Key = VirtualKey.N,
                    Modifiers = VirtualKeyModifiers.Control
                },
                new KeyboardAction
                {
                    Action = VLCAction.TabNext,
                    Key = VirtualKey.Tab,
                    Modifiers = VirtualKeyModifiers.Control
                },
                new KeyboardAction
                {
                    Action = VLCAction.TabPrevious,
                    Key = VirtualKey.Tab,
                    Modifiers = VirtualKeyModifiers.Control | VirtualKeyModifiers.Shift
                }
            };
            return shortcuts.ToDictionary(ka => (ka.Key, ka.Modifiers), ka => ka);
        }

        void KeyboardListenerService_KeyUp(CoreWindow sender, KeyEventArgs args)
        {
            if (!CanListen) return;
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
            CoreWindow coreWindow = CoreWindow.GetForCurrentThread();
            switch (args.VirtualKey)
            {
                case VirtualKey.None:
                    return;
                case VirtualKey.Control:
                    Debug.WriteLine("Ctrl key was pressed, waiting another key ...");
                    break;
                case VirtualKey.Shift:
                    Debug.WriteLine("Shift key was pressed, waiting another key ...");
                    break;
                default:
                    Debug.WriteLine($"{args.VirtualKey} key was pressed");

                    VirtualKeyModifiers modifiers = VirtualKeyModifiers.None;
                    
                    if (coreWindow.GetKeyState(VirtualKey.Shift).HasFlag(CoreVirtualKeyStates.Down)) modifiers |= VirtualKeyModifiers.Shift;
                    if (coreWindow.GetKeyState(VirtualKey.Control).HasFlag(CoreVirtualKeyStates.Down)) modifiers |= VirtualKeyModifiers.Control;
                    if (coreWindow.GetKeyState(VirtualKey.Menu).HasFlag(CoreVirtualKeyStates.Down)) modifiers |= VirtualKeyModifiers.Menu;

                    if (_shortcuts.TryGetValue((args.VirtualKey, modifiers), out KeyboardAction action))
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
                case VLCAction.Forward:
                    Locator.NavigationService.GoForward_Default();
                    break;
                case VLCAction.Home:
                    Locator.NavigationService.GoHome();
                    break;
                case VLCAction.OpenFile:
                    Locator.MediaPlaybackViewModel.PickMediaCommand.Execute(null);
                    break;
                case VLCAction.OpenNetwork:
                    Locator.MainVM.GoToStreamPanel.Execute(null);
                    break;
                case VLCAction.VolumeUp:
                    Locator.MediaPlaybackViewModel.ChangeVolumeCommand.Execute("higher");
                    break;
                case VLCAction.VolumeDown:
                    Locator.MediaPlaybackViewModel.ChangeVolumeCommand.Execute("lower");
                    break;
                case VLCAction.TabNext:
                    var pivotIndex = Locator.MainVM.Panels.IndexOf(Locator.MainVM.CurrentPanel);
                    pivotIndex = pivotIndex < Locator.MainVM.Panels.Count - 1 ? ++pivotIndex : 0;
                    Locator.NavigationService.Go(Locator.MainVM.Panels[pivotIndex].Target);
                    break;
                case VLCAction.TabPrevious:
                    pivotIndex = Locator.MainVM.Panels.IndexOf(Locator.MainVM.CurrentPanel);
                    pivotIndex = pivotIndex > 0 ? --pivotIndex : Locator.MainVM.Panels.Count - 1;
                    Locator.NavigationService.Go(Locator.MainVM.Panels[pivotIndex].Target);
                    break;
            }
        }
    }
}
