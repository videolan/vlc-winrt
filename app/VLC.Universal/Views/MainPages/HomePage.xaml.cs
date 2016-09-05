using Microsoft.Xaml.Interactivity;
using VLC.Helpers;
using VLC.Model;
using VLC.ViewModels;
using VLC.UI.Views.MainPages;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Navigation;

namespace VLC.UI.Views.MainPages
{
    public sealed partial class HomePage : Page
    {
        public HomePage()
        {
            this.InitializeComponent();
            this.SizeChanged += HomePage_SizeChanged;
        }

        private void HomePage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Responsive();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (Locator.SettingsVM.MediaCenterMode)
            {
                (FindName(nameof(MediaCenterController)) as FrameworkElement).Visibility = Visibility.Visible;
                (FindName(nameof(Pivot)) as FrameworkElement).Visibility = Visibility.Collapsed;
            }
            else
            {
                (FindName(nameof(MediaCenterController)) as FrameworkElement).Visibility = Visibility.Collapsed;
                (FindName(nameof(Pivot)) as FrameworkElement).Visibility = Visibility.Visible;
                AppViewHelper.SetAppView(true);
            }
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            if (!Locator.SettingsVM.MediaCenterMode)
                AppViewHelper.SetAppView(false);
        }

        private void TitleBar_Loaded(object sender, RoutedEventArgs e)
        {
            AppViewHelper.SetTitleBar(TitleBar);
            Responsive();
        }

        private void Responsive()
        {
            if (Window.Current.Bounds.Width < 850)
            {
                VLCIcon.Visibility = Visibility.Collapsed;
            }
            else
            {
                VLCIcon.Visibility = Visibility.Visible;
            }


            if (AppViewHelper.TitleBarRightOffset == 0)
                return;

            var pivotHeader = WinRTXamlToolkit.Controls.Extensions.VisualTreeHelperExtensions.GetFirstDescendantOfType<PivotHeaderPanel>(Pivot);
            if (pivotHeader == null)
                return;

            if (Window.Current.Bounds.Width < 850)
            {
                pivotHeader.Margin = new Thickness(0, 16, 0, 0);
            }
            else
            {
                pivotHeader.Margin = new Thickness();
            }
        }
    }
}
