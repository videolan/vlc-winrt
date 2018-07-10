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
using VLC_WinRT.Helpers;

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
            App.SplitShell.FooterVisibility = AppBarClosedDisplayMode.Hidden;
#if WINDOWS_PHONE_APP
#else
            Locator.MusicPlayerVM.PropertyChanged += MusicPlayerVM_PropertyChanged;
#endif
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            App.SplitShell.FooterVisibility = AppBarClosedDisplayMode.Compact;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
#if WINDOWS_PHONE_APP
#else
            Locator.MusicPlayerVM.PropertyChanged -= MusicPlayerVM_PropertyChanged;
#endif
        }

        private void MusicPlayerVM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Locator.MusicPlayerVM.CurrentTrack))
            {
            }
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
        #endregion

        #region interactions
        #endregion
    }
}
