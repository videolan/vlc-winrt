using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
namespace VLC_WINRT_APP.Views.MusicPages
{
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class MusicPlayerPage : Page
    {
        public MusicPlayerPage()
        {
            this.InitializeComponent();
            this.SizeChanged += OnSizeChanged;
        }

        #region layout
        private void OnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
            MainHub.Orientation = (Window.Current.Bounds.Width < 1080)
                ? Orientation.Vertical
                : Orientation.Horizontal;
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
