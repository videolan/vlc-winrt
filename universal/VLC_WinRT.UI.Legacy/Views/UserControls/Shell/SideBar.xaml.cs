using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
namespace VLC_WinRT.Views.UserControls
{
    public sealed partial class SideBar : UserControl
    {
        public SideBar()
        {
            this.InitializeComponent();
            this.Loaded += SideBar_Loaded;
        }

        private void SideBar_Loaded(object sender, RoutedEventArgs e)
        {
            Window.Current.SizeChanged += Current_SizeChanged;
            this.Unloaded += SideBar_Unloaded;
            Responsive();
        }

        private void SideBar_Unloaded(object sender, RoutedEventArgs e)
        {
            Window.Current.SizeChanged -= Current_SizeChanged;
        }

        private void Current_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            Responsive();
        }

        void Responsive()
        {
            if (Window.Current.Bounds.Width < 550)
            {
                SidebarGrid.Width = 0;
            }
            else if (Window.Current.Bounds.Width < 800)
            {
                SidebarGrid.Width = 64;
            }
            else
            {
                SidebarGrid.Width = 250;
            }
        }
    }
}
