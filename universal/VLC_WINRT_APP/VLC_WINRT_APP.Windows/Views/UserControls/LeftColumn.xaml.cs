using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VLC_WINRT_APP.Helpers;
using VLC_WINRT_APP.ViewModels;

namespace VLC_WINRT_APP.Views.UserControls
{
    public sealed partial class LeftColumn : UserControl
    {
        public LeftColumn()
        {
            this.InitializeComponent();
            Window.Current.SizeChanged += CurrentOnSizeChanged;
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Responsive();
        }

        private void CurrentOnSizeChanged(object sender, WindowSizeChangedEventArgs windowSizeChangedEventArgs)
        {
            Responsive();
        }

        void Responsive()
        {
            if (ApplicationSettingsHelper.ReadSettingsValue("IsSidebarAlwaysMinimized") == null)
            {
                ApplicationSettingsHelper.SaveSettingsValue("IsSidebarAlwaysMinimized", false);
            }
            if ((bool)ApplicationSettingsHelper.ReadSettingsValue("IsSidebarAlwaysMinimized"))
            {
                ToMediumVisualState();
                return;
            }
            
            if (Window.Current.Bounds.Width < 1080)
            {
                ToMediumVisualState();
            }
            else
            {
                ToNormalVisualState();
            }
        }

        public void MinimizeSidebar()
        {
            ToMediumVisualState();
        }

        public void RestoreSidebar()
        {
            if(ColumnGrid.Width == 100)
                ToNormalVisualState();
        }

        void ToMediumVisualState()
        {
            Locator.MusicLibraryVM.SidebarState = SidebarState.Minimized;
            ColumnGrid.Width = 100;
            TitleTextBlock.Visibility = Visibility.Collapsed;
            LargeSearchBox.Visibility = Visibility.Collapsed;
            LittleSearchBox.Visibility = Visibility.Visible;

            HeaderGrid.Margin = new Thickness(0);
            HeaderGrid.HorizontalAlignment = HorizontalAlignment.Center;

            PanelsListView.ItemTemplate = App.Current.Resources["SidebarIconItemTemplate"] as DataTemplate;
            MiniPlayer.Visibility = Visibility.Collapsed;
            Separator.Visibility = Visibility.Collapsed;
        }

        void ToNormalVisualState()
        {
            if ((bool)ApplicationSettingsHelper.ReadSettingsValue("IsSidebarAlwaysMinimized"))
                return;
            ColumnGrid.Width = 340; 
            Locator.MusicLibraryVM.SidebarState = SidebarState.Standard;
            TitleTextBlock.Visibility = Visibility.Visible;
            LargeSearchBox.Visibility = Visibility.Visible;
            LittleSearchBox.Visibility = Visibility.Collapsed;

            HeaderGrid.Margin = new Thickness(42, 0, 20, 0);
            HeaderGrid.HorizontalAlignment = HorizontalAlignment.Left;
            PanelsListView.ItemTemplate = App.Current.Resources["SidebarItemTemplate"] as DataTemplate;
            MiniPlayer.Visibility = Visibility.Visible;
            Separator.Visibility = Visibility.Visible;
        }
    }
}
