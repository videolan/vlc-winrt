/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.UI.Xaml;
using VLC_WinRT.Model;
using VLC_WinRT.ViewModels;
using Windows.UI.Core;
using VLC_WinRT.Helpers;

namespace VLC_WinRT.Services.RunTime
{
    public class MouseService
    {
#if WINDOWS_APP
        private CoreCursor _oldCursor;
#else
#endif
        private DispatcherTimer _cursorTimer;
        private const int CursorHiddenAfterSeconds = 4;
        private bool _shouldMouseBeHidden = false;
        private bool isMouseVisible = true;

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
            if (!IsCursorInWindow()) return;

            _cursorTimer.Stop();
            _cursorTimer.Start();

            if (isMouseVisible) return;
            isMouseVisible = true;
#if WINDOWS_APP
                Window.Current.CoreWindow.PointerCursor = _oldCursor;
#else
#endif
            OnMoved?.Invoke();
        }

        void LostInput()
        {
            if (Locator.NavigationService.CurrentPage != VLCPage.VideoPlayerPage &&
                Locator.NavigationService.CurrentPage != VLCPage.MusicPlayerPage)
                return;
            if (App.OpenFilePickerReason != OpenFilePickerReason.Null) return;
            isMouseVisible = false;
#if WINDOWS_APP
            if (Locator.NavigationService.CurrentPage == VLCPage.VideoPlayerPage)
            {
                if (IsCursorInWindow())
                {
                    Window.Current.CoreWindow.PointerCursor = null;
                }
            }
#else
#endif
            _cursorTimer.Stop();
            OnHidden?.Invoke();
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

        public static Point GetPointerPosition()
        {
            Window currentWindow = Window.Current;
            Point point;

            try
            {
                point = currentWindow.CoreWindow.PointerPosition;
            }
            catch (UnauthorizedAccessException)
            {
                return new Point(double.NegativeInfinity, double.NegativeInfinity);
            }

            Rect bounds = currentWindow.Bounds;
            return new Point(point.X - bounds.X, point.Y - bounds.Y);
        }

        public static bool IsCursorInWindow()
        {
            var pos = GetPointerPosition();
            if (pos == null) return false;


            return pos.Y > AppViewHelper.TitleBarHeight &&
                   pos.Y < Window.Current.Bounds.Height &&
                   pos.X > 0 &&
                   pos.X < Window.Current.Bounds.Width;
        }
    }
}
