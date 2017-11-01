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
using VLC.Model;
using VLC.ViewModels;
using Windows.UI.Core;
using VLC.Helpers;

namespace VLC.Services.RunTime
{
    public class MouseService
    {
        private CoreCursor _oldCursor;
        private DispatcherTimer _cursorTimer;
        private const int CursorHiddenAfterSeconds = 4;
        private bool isMouseVisible = true;

        public delegate void MouseHidden();

        public delegate void OnMouseMoved();

        public MouseHidden OnHidden;
        public OnMouseMoved OnMoved;

        public void Start()
        {
            _cursorTimer = new DispatcherTimer();
            _cursorTimer.Interval = TimeSpan.FromSeconds(CursorHiddenAfterSeconds);
            _cursorTimer.Tick += CursorDisappearanceTimer;

            if (!Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                var mouse = MouseDevice.GetForCurrentView();
                if (mouse != null)
                    mouse.MouseMoved += MouseMoved;
            }
        }

        public void Stop()
        {
            _cursorTimer.Tick -= CursorDisappearanceTimer;
            _cursorTimer.Stop();

            var mouse = MouseDevice.GetForCurrentView();
            if (mouse != null)
                mouse.MouseMoved -= MouseMoved;
        }

        private void MouseMoved(MouseDevice sender, MouseEventArgs args)
        {
            InputDetected();
        }

        void InputDetected()
        {
            if (!IsCursorInWindow())
                return;

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

            if (IsCursorInWindow())
            {
                _oldCursor = Window.Current.CoreWindow.PointerCursor;
                Window.Current.CoreWindow.PointerCursor = null;
                isMouseVisible = false;
            }
        }

        public void ShowCursor()
        {
            if (isMouseVisible)
                return;
            isMouseVisible = true;
            if (_oldCursor != null)
                Window.Current.CoreWindow.PointerCursor = _oldCursor;
        }

        static Point GetPointerPosition()
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

        static bool IsCursorInWindow()
        {
            var pos = GetPointerPosition();
            if (pos == null) return false;


            return /*pos.Y > AppViewHelper.TitleBarHeight &&*/
                   pos.Y < Window.Current.Bounds.Height &&
                   pos.X > 0 &&
                   pos.X < Window.Current.Bounds.Width;
        }
    }
}
