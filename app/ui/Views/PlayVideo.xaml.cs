using System;
using System.Diagnostics;
using Windows.Media;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace VLC_WINRT.Views
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PlayVideo : Page
    {
        public static StorageFile CurrentFile;

        private bool _playing;

        public PlayVideo()
        {
            InitializeComponent();

            VideoSurface.AutoPlay = true;

            this.Loaded += ImLoaded;
        }

        private async void ImLoaded(object sender, RoutedEventArgs e)
        {
            if (CurrentFile != null)
            {
                Debug.WriteLine("Playing video: " + CurrentFile.Path);
                IRandomAccessStream stream = await CurrentFile.OpenAsync(FileAccessMode.Read);
                MediaControl.TrackName = CurrentFile.DisplayName;
                VideoSurface.SetSource(stream, CurrentFile.ContentType);
              
                _playing = true;
            }
        }

       

        /// <summary>
        ///     Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">
        ///     Event data that describes how this page was reached.  The Parameter
        ///     property is typically used to configure the page.
        /// </param>
        protected override  void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        private async void PlayVideo_Click(object sender, RoutedEventArgs e)
        {
          if (_playing)
          {
              VideoSurface.Pause();
          }
          else
          {
              VideoSurface.Play();
          }
        }

        private void Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }
    }
}