using Microsoft.Xaml.Interactivity;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace VLC_WinRT.UI.Legacy.Views.MusicPages.ArtistPageControls
{
    public sealed partial class MainArtistHeader : UserControl
    {
        public MainArtistHeader()
        {
            this.InitializeComponent();
            this.Loaded += MainArtistHeader_Loaded;
        }
        
        public Visibility BackButtonVisibility
        {
            get { return (Visibility)GetValue(BackButtonVisibilityProperty); }
            set { SetValue(BackButtonVisibilityProperty, value); }
        }

        public static readonly DependencyProperty BackButtonVisibilityProperty = DependencyProperty.Register(nameof(BackButtonVisibility), typeof(Visibility), typeof(MainArtistHeader), new PropertyMetadata(Visibility.Collapsed, BackButtonPropertyChangedCallback));

        private static void BackButtonPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var that = (MainArtistHeader)dependencyObject;
            that.UpdateBackButton();
        }

        void UpdateBackButton()
        {
            BackButton.Visibility = BackButtonVisibility;
        }

        private void MainArtistHeader_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            this.Unloaded += MainArtistHeader_Unloaded;
            Window.Current.SizeChanged += Current_SizeChanged;
            Responsive();
            UpdateBackButton();
        }

        private void MainArtistHeader_Unloaded(object sender, RoutedEventArgs e)
        {
            Window.Current.SizeChanged -= Current_SizeChanged;
        }

        private void Current_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            Responsive();
        }

        void Responsive()
        {
            if (Window.Current.Bounds.Width < 600)
            {
                VisualStateUtilities.GoToState(this, "Snap", false);
            }
            else
            {
                VisualStateUtilities.GoToState(this, "Wide", false);
            }
        }
    }
}
