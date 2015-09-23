using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
namespace VLC_WinRT.Views.UserControls
{
    public sealed partial class SideBar : UserControl
    {
        public SideBar()
        {
            this.InitializeComponent();
        }

        private void Sidebar_Loaded(object sender, RoutedEventArgs e)
        {
#if WINDOWS_APP
            SidebarGrid.Width = 50;
#else
            SidebarGrid.Width = 60;
#endif
        }
    }
}
