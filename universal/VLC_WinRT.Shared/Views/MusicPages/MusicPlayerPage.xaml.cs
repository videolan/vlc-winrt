using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Microsoft.Xaml.Interactivity;
using VLC_WinRT.ViewModels;

namespace VLC_WinRT.Views.MusicPages
{
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class MusicPlayerPage : Page
    {
        public MusicPlayerPage()
        {
            this.InitializeComponent();
            this.Loaded += MusicPlayerPage_Loaded;
        }

        void MusicPlayerPage_Loaded(object sender, RoutedEventArgs e)
        {
            Responsive();
            this.SizeChanged += OnSizeChanged;
            this.Unloaded += OnUnloaded;
        }
        
        #region layout
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            try
            {
                Locator.SettingsVM.UpdateRequestedTheme();
            }
            catch { }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            Locator.SettingsVM.UpdateRequestedTheme();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
            Responsive();
        }

        private void OnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            this.SizeChanged -= OnSizeChanged;
        }

        void Responsive()
        {
            if (Window.Current.Bounds.Width < 640)
            {
                VisualStateUtilities.GoToState(this, this.ActualHeight < 900 ? "NarrowAndTiny" : "Narrow", false);
            }
            else if (this.ActualWidth < 1150)
            {
                VisualStateUtilities.GoToState(this, "Wide", false);
                VisualStateUtilities.GoToState(this, this.ActualHeight < 700 ? "WideTiny" : "WideTall", false);
            }
            else
            {
                VisualStateUtilities.GoToState(this, "ExtraWide", false);
            }
        }

        private void GridView_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
        }
        #endregion

        #region interactions
        private void GoBack(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }
        #endregion
    }
}
