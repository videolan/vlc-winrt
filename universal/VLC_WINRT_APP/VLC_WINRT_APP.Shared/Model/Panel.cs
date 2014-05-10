/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using VLC_WINRT.Common;

namespace VLC_WINRT.Model
{
    public class Panel : BindableBase
    {
        private string _title;
        private int _index;
        private double _opacity;

        public Panel(string t, int i, double o)
        {
            _title = t;
            _index = i;
            _opacity = o;
        }
        public string Title { get { return _title; } set { SetProperty(ref _title, value); } }
        public int Index { get { return _index; } set { SetProperty(ref _index, value); } }
        public double Opacity { get { return _opacity; } set { SetProperty(ref _opacity, value); } }
    }
}
