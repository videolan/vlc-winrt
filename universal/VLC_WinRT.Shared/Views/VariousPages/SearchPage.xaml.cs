using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VLC_WinRT.ViewModels;

namespace VLC_WinRT.Views.VariousPages
{
    public sealed partial class SearchPage : Page
    {
        public SearchPage()
        {
            this.InitializeComponent();
        }

        private void SearchBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            Locator.MainVM.SearchTag = SearchBox.Text;
        }

        private void RootGrid_Loaded(object sender, RoutedEventArgs e)
        {
#if WINDOWS_APP
            this.Margin = new Thickness(24, 0, 24, 0);
#else
            this.Margin = new Thickness(12, 26, 12, 0);
#endif
        }

        private void CheckBox_Loaded(object sender, RoutedEventArgs e)
        {
#if WINDOWS_PHONE_APP
            (sender as CheckBox).Style = CheckBoxStyleCentered;
#else
            (sender as CheckBox).Margin = new Thickness(0,8,12,0);
#endif
        }
    }
}
