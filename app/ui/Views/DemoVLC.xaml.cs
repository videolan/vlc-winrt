using System.Diagnostics;
using VLC_Wrapper;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
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
            this.Loaded += ImLoaded;
           
        }

        private void ImLoaded(object sender, RoutedEventArgs e)
        {
            VideoSurface.Source = new SurfaceImageSource((int)VideoSurface.ActualWidth, (int)VideoSurface.ActualHeight, true);
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
            _vlcPlayer = new Player();
            Debug.WriteLine("VLC Started");
        }
    }
}