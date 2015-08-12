using System.Collections.Generic;
using System.Diagnostics;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Xaml.Interactivity;
using VLC_WinRT.Slideshow.Texts;
using VLC_WinRT.ViewModels;

namespace VLC_WinRT.Views.MusicPages
{
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class MusicPlayerPage : Page
    {
        public MusicPlayerPage()
        {
            this.InitializeComponent();
            this.Loaded += MusicPlayerPage_Loaded;
        }

        void MusicPlayerPage_Loaded(object sender, RoutedEventArgs e)
        {
            Responsive();
            this.SizeChanged += OnSizeChanged;
            this.Unloaded += OnUnloaded;
        }

        #region layout
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Locator.MediaPlaybackViewModel.MouseService.OnHidden += MouseStateChanged;
            Locator.MediaPlaybackViewModel.MouseService.OnMoved += MouseMoved;
        }

        private void MouseStateChanged()
        {
            App.SplitShell.HideTopBar();
            FadeOut.Begin();
            var texts = new List<Txt>();
            texts.Add(new Txt(Locator.MusicPlayerVM.CurrentTrack.ArtistName.ToUpper(), Color.FromArgb(80, 255, 255, 255), new CanvasTextFormat()
            {
                FontWeight = FontWeights.Bold,
                FontSize = 120,
            }));
            texts.Add(new Txt(Locator.MusicPlayerVM.CurrentTrack.AlbumName.ToUpper(), Color.FromArgb(70, 255, 255, 255), new CanvasTextFormat()
            {
                FontWeight = FontWeights.Normal,
                FontSize = 120,
                Direction = CanvasTextDirection.RightToLeftThenTopToBottom
            }));
            texts.Add(new Txt(Locator.MusicPlayerVM.CurrentTrack.Name.ToUpper(), Color.FromArgb(50, 255, 255, 255), new CanvasTextFormat()
            {
                FontWeight = FontWeights.Light,
                FontSize = 90,
            }));
            Locator.Slideshow.AddText(texts);
        }

        private void MouseMoved()
        {
            App.SplitShell.ShowTopBar();
            FadeIn.Begin();
            Locator.Slideshow.ClearTextList();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            Locator.MainVM.UpdateRequestedTheme();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
            Responsive();
        }

        private void OnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            this.SizeChanged -= OnSizeChanged;
        }

        void Responsive()
        {
            if (Window.Current.Bounds.Width < 640)
            {
                VisualStateUtilities.GoToState(this, "Narrow", false);
            }
            else
            {
                VisualStateUtilities.GoToState(this, "Wide", false);
            }
        }

        private void GridView_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
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
