using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace VLC_WINRT.Views
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PlayVideo : Page
    {
        public PlayVideo()
        {
            InitializeComponent();
            Loaded += ImLoaded;
        }

        private void ImLoaded(object sender, RoutedEventArgs e)
        {
            //_vlcPlayer.Open("http://localhost/tears_of_steel_720p.mkv");
            //_vlcPlayer.Play();
            //_playing = true;
        }


        /// <summary>
        ///     Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">
        ///     Event data that describes how this page was reached.  The Parameter
        ///     property is typically used to configure the page.
        /// </param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        private void PlayVideo_Click(object sender, RoutedEventArgs e)
        {
            //if (_playing)
            //{
            //    _vlcPlayer.Pause();
            //}
            //else
            //{
            //    _vlcPlayer.Play();
            //}
            //_playing = !_playing;
        }

        private void Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            //VideoSurface.Fill = null;
            //Frame.GoBack(); 
            //_vlcPlayer.Stop();
        }

        private void ScreenTapped(object sender, TappedRoutedEventArgs e)
        {
            //if (BottomAppBar != null && !BottomAppBar.IsOpen)
            //    BottomAppBar.IsOpen = true;
            //if (TopAppBar != null && !TopAppBar.IsOpen)
            //    TopAppBar.IsOpen = true;
        }
    }
}