/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/
 
using VLC_WinRT.Utils;

namespace VLC_WinRT.Model
{
    public class Panel : BindableBase
    {
        private string _title;
        private int _index;
        private string _pathData;

        public Panel(string t, int i, string pd, bool isdefault = false)
        {
            _title = t;
            _index = i;
            _pathData = pd;
        }

        public string Title { get { return _title; } set { SetProperty(ref _title, value); } }
        public int Index { get { return _index; } set { SetProperty(ref _index, value); } }
        public string PathData { get { return _pathData; } set { SetProperty(ref _pathData, value); } }

    }
}
