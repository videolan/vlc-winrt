using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using VLC_WINRT.Utility.Helpers;
using VLC_WINRT.Utility.Services.RunTime;
using VLC_WINRT.ViewModels.MainPage;
using Windows.UI.Core;

namespace VLC_WINRT.Views
{
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class RemovableStoragePage : Page
    {
        public RemovableStoragePage()
        {
            this.InitializeComponent();
            this.SizeChanged += OnSizeChanged;
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            FadeInPage.Begin();
        }
        private async void GoBack_Click(object sender, RoutedEventArgs e)
        {
            if (Window.Current.Bounds.Width == 320 && FirstPanelGridView.Visibility == Windows.UI.Xaml.Visibility.Collapsed)
            {
                FirstPanelGridView.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
            else
            {
                await FadeOutPage.BeginAsync();
                NavigationService.NavigateTo(typeof(MainPage));
            }
        }

        private void FirstPanelGridView_SelectionChanged(object sender, ItemClickEventArgs e)
        {
            if (Window.Current.Bounds.Width == 320)
            {
                FirstPanelGridView.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
            SecondPanelGridView.ItemsSource = (e.ClickedItem as RemovableLibraryViewModel).Media;
            SecondPanelListView.ItemsSource = (e.ClickedItem as RemovableLibraryViewModel).Media;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
            Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                if (sizeChangedEventArgs.NewSize.Width == 320)
                {
                    FirstPanelGridView.Margin = new Thickness(10, 0, 0, 0);
                    SecondPanelListView.Visibility = Visibility.Visible;
                    SecondPanelGridView.Visibility = Visibility.Collapsed;
                }
                else
                {
                    FirstPanelGridView.Margin = new Thickness(100, 0, 0, 0);
                    SecondPanelListView.Visibility = Visibility.Collapsed;
                    SecondPanelGridView.Visibility = Visibility.Visible;
                }
            });
        }
    }
}
