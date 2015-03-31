using System;
using System.Diagnostics;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace VLC_WinRT.Views.MusicPages.MusicPlayerPageControls
{
    public sealed partial class PopularAlbumsInfoControl : UserControl
    {
        public PopularAlbumsInfoControl()
        {
            this.InitializeComponent();
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
            Window.Current.SizeChanged += CurrentOnSizeChanged;
        }

        private void OnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Debug.WriteLine("Unloading panel PopularAlbumsInfoControl");
            Window.Current.SizeChanged -= CurrentOnSizeChanged;
        }

        private void CurrentOnSizeChanged(object sender, WindowSizeChangedEventArgs windowSizeChangedEventArgs)
        {
            Responsive();
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Responsive();
        }

        void Responsive()
        {
            if (Window.Current.Bounds.Width < 400)
            {
                (PopularItemGridView.ItemsPanelRoot as WrapGrid).ItemHeight = 120;
                (PopularItemGridView.ItemsPanelRoot as WrapGrid).ItemWidth = 120;
            }
            else if (Window.Current.Bounds.Width < 800)
            {
                (PopularItemGridView.ItemsPanelRoot as WrapGrid).ItemHeight = 160;
                (PopularItemGridView.ItemsPanelRoot as WrapGrid).ItemWidth = 160;
            }
            else 
            {
                (PopularItemGridView.ItemsPanelRoot as WrapGrid).ItemHeight = 185;
                (PopularItemGridView.ItemsPanelRoot as WrapGrid).ItemWidth = 185;
            }
        }
    }
}
