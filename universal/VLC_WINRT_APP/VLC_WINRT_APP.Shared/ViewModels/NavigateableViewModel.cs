/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System.Threading.Tasks;
using VLC_WINRT.Common;
using Windows.UI.Xaml.Navigation;

namespace VLC_WINRT_APP.ViewModels
{
    public class NavigateableViewModel : BindableBase
    {
        public virtual Task OnNavigatedTo(NavigationEventArgs e) { return Task.FromResult<bool>(true); }
        public virtual Task OnNavigatedFrom(NavigationEventArgs e) { return Task.FromResult<bool>(true); }
    }
}
