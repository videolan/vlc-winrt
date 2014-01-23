// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using VLC_WINRT.Utility.Helpers;
using VLC_WINRT.ViewModels;

namespace VLC_WINRT.Views
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : BasePage
    {
        private int _currentSection;
        public MainPage()
        {
            InitializeComponent();
            this.SizeChanged += OnSizeChanged;
            this.Loaded += (sender, args) =>
            {
                FadeInPage.Begin();
                for (int i = 0; i < SectionsGrid.Children.Count; i++)
                {
                    if (i == _currentSection) continue;
                    UIAnimationHelper.FadeOut(SectionsGrid.Children[i]);
                }
            };
            NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
            Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                if (sizeChangedEventArgs.NewSize.Width < 1080)
                {
                    MainLogoHeader.VerticalAlignment = VerticalAlignment.Top;
                    MainLogoHeader.Margin = new Thickness(-43,0,0,0);
                    MiniPlayer.Visibility = Visibility.Collapsed;
                }
                else
                {
                    MainLogoHeader.VerticalAlignment = VerticalAlignment.Center;
                    MainLogoHeader.Margin = new Thickness(0);
                    MiniPlayer.Visibility = Visibility.Visible;
                }
            });
        }

        public override void SetDataContext()
        {
            _vm = (NavigateableViewModel)DataContext;
        }

        private void SectionsHeaderListView_OnItemClick(object sender, ItemClickEventArgs e)
        {
            var i = ((Model.Panel)e.ClickedItem).Index;
            ChangedSectionsHeadersState(i);
        }

        private void ChangedSectionsHeadersState(int i)
        {
            if (i == _currentSection) return;
            UIAnimationHelper.FadeOut(SectionsGrid.Children[_currentSection]);
            UIAnimationHelper.FadeIn(SectionsGrid.Children[i]);
            _currentSection = i;
            for (int j = 0; j < SectionsHeaderListView.Items.Count; j++)
                Locator.MainPageVM.Panels[j].Opacity = 0.4;
            Locator.MainPageVM.Panels[i].Opacity = 1;
        }

        private void MinimizedBottomAppBar_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            BottomAppBar.IsOpen = true;
        }
    }
}