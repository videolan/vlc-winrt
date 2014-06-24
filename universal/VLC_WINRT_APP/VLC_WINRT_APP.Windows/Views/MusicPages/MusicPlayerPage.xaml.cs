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

// Pour en savoir plus sur le modèle d'élément Page vierge, consultez la page http://go.microsoft.com/fwlink/?LinkId=234238

namespace VLC_WINRT_APP.Views.MusicPages
{
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class MusicPlayerPage : Page
    {
        public MusicPlayerPage()
        {
            this.InitializeComponent();
            this.SizeChanged += OnSizeChanged;
        }

        #region layout
        private void OnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
            MainHub.Orientation = (Window.Current.Bounds.Width < 1080)
                ? Orientation.Vertical
                : Orientation.Horizontal;
        }
        private void GridView_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            GridView gV = sender as GridView;
            (gV.ItemsPanelRoot as WrapGrid).Orientation = (Window.Current.Bounds.Width < 1080)
                ? Orientation.Horizontal
                : Orientation.Vertical;
        }
        #endregion

        #region interactions
        private void GoBack(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }
        #endregion
    }
}
