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
using Windows.UI.Core;
using Windows.UI.Xaml;
using VLC_WINRT_APP.Model;

namespace VLC_WINRT_APP.Services.RunTime
{
    public class MouseService
    {
        private CoreCursor _oldCursor;
        private DispatcherTimer _cursorTimer;
        private const int CursorHiddenAfterSeconds = 3;
        private bool _shouldMouseBeHidden = false;

        public MouseService()
        {
            _cursorTimer = new DispatcherTimer();
            _cursorTimer.Interval = TimeSpan.FromSeconds(CursorHiddenAfterSeconds);
            _cursorTimer.Tick += HideCursor;

            if (!Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                _oldCursor = Window.Current.CoreWindow.PointerCursor;
                Windows.Devices.Input.MouseDevice.GetForCurrentView().MouseMoved += MouseMoved;
            }
        }

        private void MouseMoved(MouseDevice sender, MouseEventArgs args)
        {
            if (_shouldMouseBeHidden)
            {
                Window.Current.CoreWindow.PointerCursor = _oldCursor;
                _cursorTimer.Stop();
                _cursorTimer.Start();
            }
        }

        private void HideCursor(object sender, object e)
        {
            if (App.OpenFilePickerReason != OpenFilePickerReason.Null) return;
            Window.Current.CoreWindow.PointerCursor = null;
            _cursorTimer.Stop();
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
                Window.Current.CoreWindow.PointerCursor = _oldCursor;
            }
        }
    }
}
