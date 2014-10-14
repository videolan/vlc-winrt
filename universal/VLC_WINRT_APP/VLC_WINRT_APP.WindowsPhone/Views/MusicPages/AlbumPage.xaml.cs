using Windows.Phone.UI.Input;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using WinRTXamlToolkit.Controls.Extensions;

namespace VLC_WINRT_APP.Views.MusicPages
{
    public sealed partial class AlbumPage : Page
    {
        public AlbumPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ApplicationView.GetForCurrentView().SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);
            HardwareButtons.BackPressed += HardwareButtonsOnBackPressed;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            HardwareButtons.BackPressed -= HardwareButtonsOnBackPressed;
        }

        private void HardwareButtonsOnBackPressed(object sender, BackPressedEventArgs backPressedEventArgs)
        {
            App.ApplicationFrame.GoBack();
            backPressedEventArgs.Handled = true;
        }

        private void PlaylistView_Loaded(object sender, RoutedEventArgs e)
        {
            ScrollViewer sV = (sender as ListView).GetFirstDescendantOfType<ScrollViewer>();
            sV.ViewChanging += (o, args) =>
            {
                if (args.NextView.VerticalOffset > 50 && FadeOutHeader.GetCurrentState() == ClockState.Stopped)
                {
                    Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => FadeOutHeader.Begin());
                }
            };
        }
    }
}
