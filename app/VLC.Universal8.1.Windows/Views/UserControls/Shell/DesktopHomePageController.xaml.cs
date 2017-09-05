using VLC.Helpers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;


namespace VLC.UI.Views.UserControls.Shell
{
    public sealed partial class DesktopHomePageController : UserControl
    {
        public DesktopHomePageController()
        {
            this.InitializeComponent();
        }

        private void TitleBar_Loaded(object sender, RoutedEventArgs e)
        {
            AppViewHelper.SetTitleBar(TitleBar);
            Responsive();
        }

        private void Responsive()
        {
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
