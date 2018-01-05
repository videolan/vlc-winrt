using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using VLC.ViewModels;

namespace VLC.UI.Views.MainPages.MainVideoControls
{
    public sealed partial class ShowsPivotItem : Page
    {
        ListViewItem focusedItem;

        public ShowsPivotItem()
        {
            this.InitializeComponent();
        }

        void ShowsListView_OnGotFocus(object sender, RoutedEventArgs e)
        {
            ListView list = (ListView)sender;
            if (FocusManager.GetFocusedElement() is ListViewItem)
            {
                focusedItem = (ListViewItem)FocusManager.GetFocusedElement();
                var showItem = focusedItem?.ContentTemplateRoot as UserControls.ShowItem;
                showItem?.StartAutoScroll();
            }
        }

        void ShowsListView_OnLostFocus(object sender, RoutedEventArgs e)
        {
            var showItem = focusedItem?.ContentTemplateRoot as UserControls.VideoItem;
            showItem?.StopAutoScroll();
        }
    }
}
