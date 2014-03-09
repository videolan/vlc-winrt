/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace VLC_WINRT.Views.Controls.MainPage
{
    public sealed partial class Welcome : UserControl
    {
        public Welcome()
        {
            this.InitializeComponent();
        }
    }
}
