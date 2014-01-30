using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace VLC_WINRT.Views.Controls.MainPage
{
    public sealed partial class Welcome : UserControl
    {
        public Welcome()
        {
            this.InitializeComponent();
        }

        private void OpenAppBar_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var frame = App.ApplicationFrame;
            var page = frame.Content as Views.MainPage;
            if (page != null)
            {
                Dispatcher.RunAsync(CoreDispatcherPriority.Normal, page.CreateVLCMenu);
            }
        }
    }
}
