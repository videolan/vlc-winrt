/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using Windows.UI;
using Windows.UI.Xaml.Media;
using VLC_WINRT.Common;
using VLC_WINRT_APP.Common;

namespace VLC_WINRT_APP.Model
{
    public class Panel : BindableBase
    {
        private string _title;
        private int _index;
        private double _opacity;
        private string _pathData;
        private SolidColorBrush _color;

        public Panel(string t, int i, double o, string pd, bool isdefault = false)
        {
            _title = t;
            _index = i;
            _opacity = o;
            _pathData = pd;
            _color = isdefault
                ? App.Current.Resources["MainColor"] as SolidColorBrush
                : new SolidColorBrush(Colors.DimGray);
        }
        public string Title { get { return _title; } set { SetProperty(ref _title, value); } }
        public int Index { get { return _index; } set { SetProperty(ref _index, value); } }
        public double Opacity { get { return _opacity; } set { SetProperty(ref _opacity, value); } }
        public string PathData { get { return _pathData; } set { SetProperty(ref _pathData, value); } }

        public SolidColorBrush Color
        {
            get { return _color; }
            set { SetProperty(ref _color, value); }
        }
    }
}
