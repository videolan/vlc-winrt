using System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using VLC_WINRT.Utility.Helpers;
using VLC_WINRT.ViewModels;

namespace VLC_WINRT.Views
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PlayVideo : BasePage
    {
        private bool isCommandShown;
        DispatcherTimer _timer = new DispatcherTimer();
        public PlayVideo()
        {
            InitializeComponent();
            this.SizeChanged += OnSizeChanged;

            _timer.Interval = TimeSpan.FromSeconds(7);
            _timer.Tick += TimerOnTick;
            ShowCommands();
        }

        private void TimerOnTick(object sender, object o)
        {
            if(isCommandShown)
                HideCommands();
        }

        private void OnPointerEntered(object sender, PointerRoutedEventArgs pointerRoutedEventArgs)
        {
            ShowCommands();
        }
        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ShowCommands();
        }

        private void ShowCommands()
        {
            _timer.Start();
            UIAnimationHelper.FadeIn(Commands);
            isCommandShown = true;
        }

        void HideCommands()
        {
            _timer.Stop();
            UIAnimationHelper.FadeOut(Commands);
            isCommandShown = false;
        }
        private void OnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
            var x = Window.Current.Bounds.Width;
            var y = Window.Current.Bounds.Height;
            Locator.PlayVideoVM.SetSizeVideoPlayer((uint)x, (uint)y);
        }

        public override void SetDataContext()
        {
            _vm = (NavigateableViewModel)DataContext;
            base.SetDataContext();
        }
    }
}