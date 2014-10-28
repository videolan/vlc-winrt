using Windows.UI.Xaml.Controls;
using VLC_WINRT_APP.Helpers;


namespace VLC_WINRT_APP.Views.UserControls
{
    public sealed partial class VLCSearchBox : UserControl
    {
        public VLCSearchBox()
        {
            this.InitializeComponent();
        }

        private void AutoSuggestBox_OnTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            sender.ItemsSource = SearchHelpers.Search(sender.Text);
        }
    }
}
