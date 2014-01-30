using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Pour en savoir plus sur le modèle d'élément Contrôle utilisateur, consultez la page http://go.microsoft.com/fwlink/?LinkId=234236
using VLC_WINRT.Utility.Helpers;

namespace VLC_WINRT.Views.Controls.MusicPage
{
    public sealed partial class VerticalAlbumItemTemplate : UserControl
    {
        public VerticalAlbumItemTemplate()
        {
            this.InitializeComponent();
            UIAnimationHelper.FadeOut(PlayListGrid);
        }

        private void Album_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            if (PlayListGrid.Visibility == Visibility.Visible)
            {
                UIAnimationHelper.FadeOut(PlayListGrid);
            }
            else
            {
                UIAnimationHelper.FadeIn(PlayListGrid);
            }
        }
    }
}
