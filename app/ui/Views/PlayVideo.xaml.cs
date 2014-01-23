using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using VLC_WINRT.ViewModels;

namespace VLC_WINRT.Views
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PlayVideo : BasePage
    {
        public PlayVideo()
        {
            InitializeComponent();
            this.SizeChanged += OnSizeChanged;
            this.PointerMoved += OnPointerMoved;
        }

        private void OnPointerMoved(object sender, PointerRoutedEventArgs pointerRoutedEventArgs)
        {

        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
            var x = Window.Current.Bounds.Width;
            var y = Window.Current.Bounds.Height;
            Locator.PlayVideoVM.SetSizeVideoPlayer((uint)x,(uint)y);
        }

        public override void SetDataContext()
        {
            _vm = (NavigateableViewModel) DataContext;
            base.SetDataContext();
        }

        private void ScreenTapped(object sender, TappedRoutedEventArgs e)
        {
            if (BottomAppBar != null && !BottomAppBar.IsOpen)
                BottomAppBar.IsOpen = true;
            if (TopAppBar != null && !TopAppBar.IsOpen)
                TopAppBar.IsOpen = true;
        }
    }
}