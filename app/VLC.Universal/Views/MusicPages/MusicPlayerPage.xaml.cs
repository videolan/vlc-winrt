using System;
using System.ComponentModel;
using Windows.Devices.Input;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Microsoft.Xaml.Interactivity;
using VLC.Helpers;
using VLC.ViewModels;

namespace VLC.UI.Views.MusicPages
{
    public sealed partial class MusicPlayerPage : Page
    {
        readonly SolidColorBrush _white;
        readonly SolidColorBrush _red;
        MediaPlaybackViewModel _viewModel => Locator.MediaPlaybackViewModel;

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
            _viewModel.SliderBindingEnabled = true;
            _viewModel.PropertyChanged += MediaPlaybackViewModelOnPropertyChanged;
            if(DeviceHelper.GetDeviceType() == DeviceTypeEnum.Tablet)
                PointerWheelChanged += OnPointerWheelChanged;
        }

        void OnPointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType != PointerDeviceType.Mouse)
                return;

            e.Handled = true;

            var mouseWheelDelta = e.GetCurrentPoint(this).Properties.MouseWheelDelta;
            if (mouseWheelDelta > 0)
                _viewModel.IncreaseVolumeCommand.Execute(null);
            else
                _viewModel.DecreaseVolumeCommand.Execute(null);
        }

        void MediaPlaybackViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_viewModel.Volume))
            {
                var volume = _viewModel.Volume;
                if (volume > _viewModel.VOLUME_THRESHOLD)
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
            _viewModel.PropertyChanged -= MediaPlaybackViewModelOnPropertyChanged;
            if (DeviceHelper.GetDeviceType() == DeviceTypeEnum.Tablet)
                PointerWheelChanged -= OnPointerWheelChanged;
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
