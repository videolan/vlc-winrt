using System.Collections.Generic;
using System.Diagnostics;
using Windows.UI;
using Windows.UI.Input;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Xaml.Interactivity;
using VLC_WinRT.Slideshow.Texts;
using VLC_WinRT.ViewModels;

namespace VLC_WinRT.Views.MusicPages
{
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
#if WINDOWS_APP
            Locator.MediaPlaybackViewModel.MouseService.OnHidden += MouseStateChanged;
            Locator.MediaPlaybackViewModel.MouseService.OnMoved += MouseMoved;
            Locator.MusicPlayerVM.PropertyChanged += MusicPlayerVM_PropertyChanged;
#endif
            Locator.Slideshow.SetTheme(true, true);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            MouseMoved();
            Locator.Slideshow.ClearTextList();
#if WINDOWS_APP
            Locator.MediaPlaybackViewModel.MouseService.OnHidden -= MouseStateChanged;
            Locator.MediaPlaybackViewModel.MouseService.OnMoved -= MouseMoved;
            Locator.MusicPlayerVM.PropertyChanged -= MusicPlayerVM_PropertyChanged;
#endif
            Locator.Slideshow.SetTheme(false);
        }

        private void MouseStateChanged()
        {
            App.SplitShell.HideTopBar();
            FadeOut.Begin();
            Locator.Slideshow.TextInSlideshowEnabled = true;
        }

        private void MusicPlayerVM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Locator.MusicPlayerVM.CurrentTrack))
            {
                PopulateSlideshowWithText();
            }
        }

        void PopulateSlideshowWithText()
        {
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
            Locator.Slideshow.SetText(texts);
        }

        private void MouseMoved()
        {
            Locator.MusicPlayerVM.PropertyChanged -= MusicPlayerVM_PropertyChanged;
            App.SplitShell.ShowTopBar();
            FadeIn.Begin();
            Locator.Slideshow.TextInSlideshowEnabled = false;
        }

        private void PlaceholderInteractionGrid_OnTapped(object sender, TappedRoutedEventArgs args)
        {
            if (args.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch)
                Locator.MediaPlaybackViewModel.MouseService.Content_Tapped(sender, args);
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
