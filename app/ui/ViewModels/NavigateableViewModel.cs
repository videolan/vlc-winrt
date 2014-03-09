/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using VLC_WINRT.Common;

namespace VLC_WINRT.ViewModels
{
    public class NavigateableViewModel : BindableBase
    {
        public virtual void OnNavigatedTo(){}
        public virtual void OnNavigatedFrom(){}
    }
}
