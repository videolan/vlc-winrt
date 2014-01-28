using System;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using VLC_WINRT.Utility.Helpers;
using VLC_WINRT.Utility.Services.RunTime;
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
                FadeOutPage.Begin();
                ChangeLayout(Window.Current.Bounds.Width);
                for (int i = 0; i < SectionsGrid.Children.Count; i++)
                {
                    if (i == _currentSection) continue;
                    UIAnimationHelper.FadeOut(SectionsGrid.Children[i]);
                }
                FadeInPage.Begin();
            };
            NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
            ChangeLayout(sizeChangedEventArgs.NewSize.Width);
        }
        void ChangeLayout(double x)
        {
            Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                if (x < 900)
                {
                    MainLogoGrid.VerticalAlignment = VerticalAlignment.Top;
                    MiniPlayer.Visibility = Visibility.Collapsed;
                }
                else
                {
                    MainLogoGrid.VerticalAlignment = VerticalAlignment.Center;
                    MiniPlayer.Visibility = Visibility.Visible;
                }

                if (x == 320)
                {
                    MainLogoGrid.Visibility = Visibility.Collapsed;
                }
                else
                {
                    MainLogoGrid.Visibility = Visibility.Visible;
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

        public void ChangedSectionsHeadersState(int i)
        {
            if (i == _currentSection) 
                return;
            App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                UIAnimationHelper.FadeOut(SectionsGrid.Children[_currentSection]);
                UIAnimationHelper.FadeIn(SectionsGrid.Children[i]);
                _currentSection = i;
                for (int j = 0; j < SectionsHeaderListView.Items.Count; j++)
                    Locator.MainPageVM.Panels[j].Opacity = 0.4;
                Locator.MainPageVM.Panels[i].Opacity = 1;
            });
        }

        private void MinimizedBottomAppBar_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            BottomAppBar.IsOpen = true;
        }

        private async void MorePanelsButton_OnClick(object sender, RoutedEventArgs e)
        {
            var popupMenu = new PopupMenu();
            popupMenu.Commands.Add(new UICommand("Removable storage", async h =>
            {
                await FadeOutPage.BeginAsync();
                NavigationService.NavigateTo(typeof (RemovableStoragePage));
            }));

            popupMenu.Commands.Add(new UICommand("DLNA", async h =>
            {
                await FadeOutPage.BeginAsync();
                NavigationService.NavigateTo(typeof (DLNAPage));
            }));
            
            var button = (Button)sender;
            var transform = button.TransformToVisual(this);
            var point = transform.TransformPoint(new Point(-60, 50));
            await popupMenu.ShowAsync(point);
        }
    }
}