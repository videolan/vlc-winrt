/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236
using VLC_WINRT.Utility.Services.RunTime;

namespace VLC_WINRT.Views.Controls.MainPage
{
    public sealed partial class Welcome : UserControl
    {
        public Welcome()
        {
            this.InitializeComponent();
        }

        private void GoToSpecialThanksPage(object sender, RoutedEventArgs e)
        {
            NavigationService.NavigateTo(typeof(SpecialThanks));
        }
    }
}
