using System.Linq;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238
using Windows.UI.Xaml.Input;
using VLC_WINRT.Utility.Helpers;
using VLC_WINRT.ViewModels;
using VLC_WINRT.ViewModels.Settings;

namespace VLC_WINRT.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            this.InitializeComponent();
        }

        private void OpenSearchPane(object sender, RoutedEventArgs e)
        {
            App.RootPage.SearchPane.Show();
        }

        private void MovieListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!e.AddedItems.Any()) return;

            PopupMenu popup = new PopupMenu();
            popup.Commands.Add(new UICommand
            {
                Label = "Remove",
                Invoked = new UICommandInvokedHandler((tapped) =>
                {
                    Locator.SettingsVM.RemoveVideoFolder(e.AddedItems[0] as CustomFolder);
                }),
            });
            popup.ShowForSelectionAsync((sender as FrameworkElement).GetBoundingRect());
        }
        private void MusicListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!e.AddedItems.Any()) return;
            PopupMenu popup = new PopupMenu();
            popup.Commands.Add(new UICommand
            {
                Label = "Remove",
                Invoked = new UICommandInvokedHandler((tapped) =>
                {
                    Locator.SettingsVM.RemoveMusicFolder(e.AddedItems[0] as CustomFolder);
                }),
            });
            popup.ShowForSelectionAsync((sender as FrameworkElement).GetBoundingRect());
        }

        private void GoBack_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }
    }
}
