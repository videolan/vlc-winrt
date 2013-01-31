using System;
using System.Diagnostics;
using VLC_Wrapper;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace VLC_WINRT.Views
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DemoVLC : Page
    {
        private Player _vlcPlayer;

        public DemoVLC()
        {
            InitializeComponent();
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
            Debug.WriteLine("Starting VLC");
            try
            {
                ImageBrush brush = new ImageBrush();
                VideoSurface.Fill = brush;
                _vlcPlayer = new Player(brush);

                Debug.WriteLine("VLC Started");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Couldn't start VLC: " + ex.ToString());
                return;
            }
            Debug.WriteLine("Testing a media");
            _vlcPlayer.TestMedia();
        }
    }
}
