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
#if WINDOWS_PHONE_APP
#else
        private CoreCursor _oldCursor;
#endif
        private DispatcherTimer _cursorTimer;
        private const int CursorHiddenAfterSeconds = 4;
        private bool isMouseVisible = true;

        public delegate void MouseHidden();

        public delegate void OnMouseMoved();

        public MouseHidden OnHidden;
        public OnMouseMoved OnMoved;

        public MouseService()
        {
            _cursorTimer = new DispatcherTimer();
            _cursorTimer.Interval = TimeSpan.FromSeconds(CursorHiddenAfterSeconds);
            _cursorTimer.Tick += CursorDisappearanceTimer;

            if (!Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
#if WINDOWS_PHONE_APP
#else
                var mouse = MouseDevice.GetForCurrentView();
                if (mouse != null)
                    mouse.MouseMoved += MouseMoved;
#endif
            }
        }

        private void MouseMoved(MouseDevice sender, MouseEventArgs args)
        {
            InputDetected();
        }

        void InputDetected()
        {
            if (!IsCursorInWindow()) return;

            _cursorTimer.Stop();
            _cursorTimer.Start();

            OnMoved?.Invoke();
        }

        void OnCursorDisappearance()
        {
            _cursorTimer.Stop();
            OnHidden?.Invoke();
        }

        private void CursorDisappearanceTimer(object sender, object e)
        {
            OnCursorDisappearance();
        }

        public void HideCursor()
        {
            if (!isMouseVisible)
                return;
#if WINDOWS_PHONE_APP
#else
            if (IsCursorInWindow())
            {
                _oldCursor = Window.Current.CoreWindow.PointerCursor;
                Window.Current.CoreWindow.PointerCursor = null;
                isMouseVisible = false;
            }
#endif
        }

        public void ShowCursor()
        {
            if (isMouseVisible)
                return;
            isMouseVisible = true;
#if WINDOWS_PHONE_APP
#else
            Window.Current.CoreWindow.PointerCursor = _oldCursor;
#endif
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
