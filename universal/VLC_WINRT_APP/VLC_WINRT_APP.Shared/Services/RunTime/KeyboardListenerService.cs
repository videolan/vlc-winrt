using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Core;
using libVLCX;
using VLC_WINRT_APP.DataRepository;
using VLC_WINRT_APP.Model;
using VLC_WINRT_APP.ViewModels;

namespace VLC_WINRT_APP.Services.RunTime
{
    public class KeyboardListenerService
    {
        private const uint MaxVirtualKeys = 3;
        private VirtualKey[] virtualKeys = new VirtualKey[MaxVirtualKeys];
        public bool CanListen { get; set; }
        public KeyboardListenerService()
        {
            CanListen = true;
            CoreWindow.GetForCurrentThread().KeyUp += KeyboardListenerService_KeyUp;
            CoreWindow.GetForCurrentThread().KeyDown += KeyboardListenerService_KeyDown;
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
        }

        async void KeyboardListenerService_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
            if (!CanListen) return;
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
                    Debug.WriteLine("OneShot key was pressed");
                    // look in the db for a match
                    var action = await Locator.SettingsVM._keyboardActionDataRepository.GetKeyboardAction(virtualKeys[0], virtualKeys[1]);
                    if (action != null)
                    {
                        // if there's a match, get the ActionId
                        await DoKeyboardAction(action);
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
                    case VLCAction.PauseToggle:
                        if (Locator.MediaPlaybackViewModel.MediaState == MediaState.Paused
                            || Locator.MediaPlaybackViewModel.MediaState == MediaState.Playing)
                        {
                            Locator.MediaPlaybackViewModel._mediaService.Pause();
                        }
                        break;
                    case VLCAction.Quit:
                        App.Current.Exit();
                        break;
                    case VLCAction.Stop:
                        if (Locator.MediaPlaybackViewModel.PlayingType == PlayingType.Video)
                        {
                            Locator.MediaPlaybackViewModel.GoBack.Execute(null);
                        }
                        break;
                    case VLCAction.Previous:
                        if (Locator.MediaPlaybackViewModel.PlayingType == PlayingType.Music)
                        {
                            await Locator.MediaPlaybackViewModel.PlayPrevious();
                        }
                        break;
                    case VLCAction.Next:
                        if (Locator.MediaPlaybackViewModel.PlayingType == PlayingType.Music)
                        {
                            await Locator.MediaPlaybackViewModel.PlayNext();
                        }
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
                                Locator.VideoLibraryVM.OpenVideo.Execute(null);
                            }
                            break;
                        case VLCAction.OpenNetwork:
                            {
                                Locator.MainVM.OpenStreamFlyout();
                            }
                            break;
                    }
                }
            }
        }
    }
}
