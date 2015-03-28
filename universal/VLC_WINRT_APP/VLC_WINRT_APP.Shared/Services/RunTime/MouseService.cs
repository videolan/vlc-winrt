/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System;
using Windows.Devices.Geolocation;
using Windows.Devices.Input;
using Windows.UI.Core;
using Windows.UI.Xaml;
using VLC_WINRT_APP.Model;
using VLC_WINRT_APP.ViewModels;

namespace VLC_WINRT_APP.Services.RunTime
{
    public class MouseService
    {
#if WINDOWS_APP
        private CoreCursor _oldCursor;
#else
#endif
        private DispatcherTimer _cursorTimer;
        private const int CursorHiddenAfterSeconds = 5;
        private bool _shouldMouseBeHidden = false;
        private bool isMouseVisible=true;

        public delegate void MouseHidden();

        public delegate void OnMouseMoved();

        public MouseHidden OnHidden;
        public OnMouseMoved OnMoved;

        public MouseService()
        {
            _cursorTimer = new DispatcherTimer();
            _cursorTimer.Interval = TimeSpan.FromSeconds(CursorHiddenAfterSeconds);
            _cursorTimer.Tick += HideCursor;

            if (!Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
#if WINDOWS_APP
                if (Window.Current.CoreWindow.PointerCursor != null)
                    _oldCursor = Window.Current.CoreWindow.PointerCursor;
                var mouse = MouseDevice.GetForCurrentView();
                if (mouse != null) mouse.MouseMoved += MouseMoved;
#else
#endif
            }
        }

        public void Content_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (!isMouseVisible)
                InputDetected();
            else
                LostInput();
        }

        private void MouseMoved(MouseDevice sender, MouseEventArgs args)
        {
            InputDetected();
        }

        void InputDetected()
        {
            if (!_shouldMouseBeHidden) return;
            _cursorTimer.Stop();
            _cursorTimer.Start();
            if (isMouseVisible) return;
            isMouseVisible = true;
#if WINDOWS_APP
            Window.Current.CoreWindow.PointerCursor = _oldCursor;
#else
#endif
            if (OnMoved != null)
                OnMoved();
        }

        void LostInput()
        {
            if (Locator.MediaPlaybackViewModel.PlayingType != PlayingType.Video) return;
            if (App.OpenFilePickerReason != OpenFilePickerReason.Null) return;
            isMouseVisible = false;
#if WINDOWS_APP
            Window.Current.CoreWindow.PointerCursor = null;
#else
#endif
            _cursorTimer.Stop();
            if (OnHidden != null)
                OnHidden();
        }

        private void HideCursor(object sender, object e)
        {
            LostInput();
        }

        public void HideMouse()
        {
            lock (this)
            {
                _shouldMouseBeHidden = true;
                _cursorTimer.Start();
            }
        }

        public void RestoreMouse()
        {
            lock (this)
            {
                _shouldMouseBeHidden = false;
                _cursorTimer.Stop();
#if WINDOWS_APP
                Window.Current.CoreWindow.PointerCursor = _oldCursor;
#endif
            }
        }
    }
}
