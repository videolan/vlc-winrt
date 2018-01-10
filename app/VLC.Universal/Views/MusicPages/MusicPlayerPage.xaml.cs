using System.ComponentModel;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Microsoft.Xaml.Interactivity;
using VLC.ViewModels;

namespace VLC.UI.Views.MusicPages
{
    public sealed partial class MusicPlayerPage : Page
    {
        readonly SolidColorBrush _white;
        readonly SolidColorBrush _red;

        public MusicPlayerPage()
        {
            this.InitializeComponent();
            this.Loaded += MusicPlayerPage_Loaded;
            _white = new SolidColorBrush(Colors.White);
            _red = new SolidColorBrush(Colors.Red);
        }

        void MusicPlayerPage_Loaded(object sender, RoutedEventArgs e)
        {
            Responsive();
            this.SizeChanged += OnSizeChanged;
            this.Unloaded += OnUnloaded;
            Locator.MediaPlaybackViewModel.SliderBindingEnabled = true;
            Locator.MediaPlaybackViewModel.PropertyChanged += MediaPlaybackViewModelOnPropertyChanged;
        }

        void MediaPlaybackViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Locator.MediaPlaybackViewModel.Volume))
            {
                var volume = Locator.MediaPlaybackViewModel.Volume;
                if (volume > 100)
                {
                    if (VolumeSlider.Foreground == _red)
                        return;
                    VolumeSlider.Foreground = _red;
                }
                else
                {
                    if (VolumeSlider.Foreground == _white)
                        return;
                    VolumeSlider.Foreground = _white;
                }
            }
        }

        private void Slideshower_Loaded_1(object sender, RoutedEventArgs e)
        {
            Locator.Slideshow.Initialize(ref Slideshower);
        }

        #region layout
        private void OnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
            Responsive();
        }

        private void OnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            this.SizeChanged -= OnSizeChanged;
            Locator.MediaPlaybackViewModel.PropertyChanged -= MediaPlaybackViewModelOnPropertyChanged;
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
