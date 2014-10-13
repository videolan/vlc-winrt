using System.Diagnostics;
using Windows.Graphics.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VLC_WINRT_APP.Model;
using VLC_WINRT_APP.ViewModels;

namespace VLC_WINRT_APP.Views.MainPages
{
    public sealed partial class MainPageHome : Page
    {
        public MainPageHome()
        {
            InitializeComponent();
            this.SizeChanged += OnSizeChanged;
            Loaded += OnLoaded;
        }


        protected override void OnNavigatedTo(Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (Locator.VideoLibraryVM.LoadingState == LoadingState.NotLoaded)
            {
                Locator.VideoLibraryVM.Initialize();
            }
            if (Locator.MusicLibraryVM.LoadingState == LoadingState.NotLoaded)
            {
                Locator.MusicLibraryVM.Initialize();
            }
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
            Responsive();
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Responsive();
        }

        private void Responsive()
        {
            var width = Window.Current.Bounds.Width;
            Debug.WriteLine(width);
        }

        private void VideoItemWrapGrid_Loaded(object sender, RoutedEventArgs e)
        {
            Responsive(sender as ItemsWrapGrid);
        }


        private void VideoItemWrapGrid_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            Responsive(sender as ItemsWrapGrid);
        }

        private void Responsive(ItemsWrapGrid itemsWrap)
        {
            //int usefulWidth = (int) Window.Current.Bounds.Width;
            //Debug.WriteLine(usefulWidth);
            //int sidebar;
            //if (Window.Current.Bounds.Width < 400)
            //{
            //    sidebar = 65;
            //}
            //else if (Window.Current.Bounds.Width < 1080)
            //{
            //    sidebar = 180;
            //}
            //else
            //{
            //    sidebar = 420;
            //}
            //usefulWidth -= sidebar;

            //if (usefulWidth < 400)
            //{
            //    itemsWrap.ItemWidth = usefulWidth;
            //    itemsWrap.ItemHeight = usefulWidth*0.561;
            //}
            //else if (usefulWidth < 890)
            //{
            //    itemsWrap.ItemWidth = usefulWidth/2;
            //    itemsWrap.ItemHeight = (usefulWidth/2)*0.561;
            //}
            //else if (usefulWidth < 1300)
            //{
            //    itemsWrap.ItemWidth = usefulWidth/3;
            //    itemsWrap.ItemHeight = (usefulWidth/3)*0.561;
            //}
            //else
            //{
            //    itemsWrap.ItemWidth = usefulWidth/4;
            //    itemsWrap.ItemHeight = (usefulWidth/4)*0.561;
            //}
        }
    }
}