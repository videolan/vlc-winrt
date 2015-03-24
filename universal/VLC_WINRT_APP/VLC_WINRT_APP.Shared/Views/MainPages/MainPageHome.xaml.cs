using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;

using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using VLC_WINRT_APP.Helpers;
using VLC_WINRT_APP.Model.Video;
using VLC_WINRT_APP.ViewModels;
#if WINDOWS_PHONE_APP
using Windows.Phone.UI.Input;
#endif

namespace VLC_WINRT_APP.Views.MainPages
{
    public sealed partial class MainPageHome : Page
    {
        public MainPageHome()
        {
            InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
            this.SizeChanged += OnSizeChanged;
            Loaded += OnLoaded;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
#if WINDOWS_PHONE_APP
            HardwareButtons.BackPressed += HardwareButtonsOnBackPressed;
#endif
            }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
#if WINDOWS_PHONE_APP
            HardwareButtons.BackPressed -= HardwareButtonsOnBackPressed;
#endif
            }

        private void OnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
            Responsive();
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Responsive();
        }

#if WINDOWS_PHONE_APP
        private void HardwareButtonsOnBackPressed(object sender, BackPressedEventArgs backPressedEventArgs)
        {
            if (Locator.MainVM.PreventAppExit)
            {
                backPressedEventArgs.Handled = true;
                return;
            }
        }
#endif

        private void Responsive()
        {
        }

        private void VideoWrapGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            TemplateSizer.ComputeCompactVideo(sender as ItemsWrapGrid, VideosListView.ActualWidth);
        }

        private void AlbumsWrapGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            TemplateSizer.ComputeAlbums(sender as ItemsWrapGrid, TemplateSize.Compact, AlbumsListView.ActualWidth);
        }
    }
}