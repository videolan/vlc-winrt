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
                    //MiniPlayer.Visibility = Visibility.Collapsed;
                    RemoveOtherPanels();
                }
                else
                {
                    //MiniPlayer.Visibility = Visibility.Visible;
                    AddOtherPanels();
                }

                if (x == 320)
                {
                }
                else
                {
                }
            });
        }

        void AddOtherPanels()
        {
            //if (Locator.MainPageVM.Panels.Count != 3) return;
            //Locator.MainPageVM.Panels.Add(new Model.Panel("external devices", 3, 0.4));
            //Locator.MainPageVM.Panels.Add(new Model.Panel("media servers", 4, 0.4));
        }
        void RemoveOtherPanels()
        {
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
            CreateVLCMenu();
        }

        public async void CreateVLCMenu()
        {
            var popupMenu = new PopupMenu();
            popupMenu.Commands.Add(new UICommand("External storage", async h =>
            {
                await FadeOutPage.BeginAsync();
                NavigationService.NavigateTo(typeof(RemovableStoragePage));
            }));

            popupMenu.Commands.Add(new UICommand("Media servers", async h =>
            {
                await FadeOutPage.BeginAsync();
                NavigationService.NavigateTo(typeof(DLNAPage));
            }));

            popupMenu.Commands.Add(new UICommand("Open video", async h =>
            {
                Locator.MainPageVM.PickVideo.Execute(null);
            }));

            popupMenu.Commands.Add(new UICommand("Open stream", async h =>
            {
                Locator.MainPageVM.PlayNetworkMRL.Execute(null);
            }));

            var transform = RootGrid.TransformToVisual(this);
            var point = transform.TransformPoint(new Point(270, 110));
            await popupMenu.ShowAsync(point);
        }
    }
}