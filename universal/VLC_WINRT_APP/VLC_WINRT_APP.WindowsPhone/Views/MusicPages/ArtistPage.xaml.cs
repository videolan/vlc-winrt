using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Phone.UI.Input;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// Pour en savoir plus sur le modèle d’élément Page vierge, consultez la page http://go.microsoft.com/fwlink/?LinkID=390556
using VLC_WINRT_APP.Model.Video;
using WinRTXamlToolkit.Controls.Extensions;

namespace VLC_WINRT_APP.Views.MusicPages
{
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class ArtistPage : Page
    {
        public ArtistPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            HardwareButtons.BackPressed -= HardwareButtonsOnBackPressed;
        }

        private void HardwareButtonsOnBackPressed(object sender, BackPressedEventArgs backPressedEventArgs)
        {
            App.ApplicationFrame.GoBack();
            backPressedEventArgs.Handled = true;
        }

        /// <summary>
        /// Invoqué lorsque cette page est sur le point d'être affichée dans un frame.
        /// </summary>
        /// <param name="e">Données d'événement décrivant la manière dont l'utilisateur a accédé à cette page.
        /// Ce paramètre est généralement utilisé pour configurer la page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            HardwareButtons.BackPressed += HardwareButtonsOnBackPressed;
        }

        private async void AlbumsView_Loaded(object sender, RoutedEventArgs e)
        {
            ScrollViewer sV = (sender as GridView).GetFirstDescendantOfType<ScrollViewer>();
            sV.ViewChanging += async (o, args) =>
            {
                if (args.NextView.VerticalOffset > 50 && FadeOutHeader.GetCurrentState() == ClockState.Stopped)
                {
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => FadeOutHeader.Begin());
                }
            };
        }

        private void ItemsWrapGrid_Loaded(object sender, RoutedEventArgs e)
        {
            TemplateSizer.ComputeAlbums(sender as ItemsWrapGrid, TemplateSize.Normal);
        }
    }
}
