using System;
using Windows.Phone.UI.Input;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using ScrollWatchedSelector;

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
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as FlipView).SelectedIndex == 0)
            {
                if (ToNormalHeader == null) return;
                ToNormalHeader.Begin();
            }
            else if ((sender as FlipView).SelectedIndex == 1)
            {
                if (FadeInBigHeader == null) return;
                FadeInBigHeader.Begin();
            }
        }

        private void ScrollWatchedListView_OnGoingTopOrBottom(IScrollWatchedSelector lv, EventArgs eventArgs)
        {
            var e = eventArgs as ScrollingEventArgs;
            if (e.ScrollingType == ScrollingType.ToBottom)
            {
                Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => FadeOutHeader.Begin());
            }
            else if (e.ScrollingType == ScrollingType.ToTop)
            {
                Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => ToNormalHeader.Begin());
            }
        }
    }
}
