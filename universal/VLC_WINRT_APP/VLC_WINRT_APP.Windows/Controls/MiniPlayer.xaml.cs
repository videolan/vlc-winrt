/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;

// Pour en savoir plus sur le modèle d'élément Contrôle utilisateur, consultez la page http://go.microsoft.com/fwlink/?LinkId=234236
using VLC_WINRT.Utility.Helpers;
using VLC_WINRT.Utility.Services.RunTime;
using VLC_WINRT_APP;

namespace VLC_WINRT.Views.Controls
{
    public sealed partial class MiniPlayer : UserControl
    {
        public MiniPlayer()
        {
            this.InitializeComponent();
        }
        private async void InformationsCurrentPlayingGrid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var frame = App.ApplicationFrame;
            var page = frame.Content as Views.MainPage;
            if (page != null)
            {
                var sB = page.Resources["FadeOutPage"] as Storyboard;
                if (sB != null)
                {
                    await sB.BeginAsync();
                    NavigationService.NavigateTo(typeof(PlayMusic));
                }
            }
        }
    }
}
