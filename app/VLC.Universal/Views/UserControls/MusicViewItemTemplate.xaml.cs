using VLC.Model.Music;
using VLC.ViewModels;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace VLC.UI.Views.UserControls
{
    public sealed partial class MusicViewItemTemplate : UserControl
    {
        private Brush savedBrush = null;

        public MusicViewItemTemplate()
        {
            this.InitializeComponent();
            this.DataContextChanged += MusicViewItemTemplate_DataContextChanged;
            Locator.MusicLibraryVM.MusicViewSet += MusicViewSet;
            savedBrush = Title.Foreground;
        }

        ~MusicViewItemTemplate()
        {
            Locator.MusicLibraryVM.MusicViewSet -= MusicViewSet;
        }

        private void MusicViewItemTemplate_DataContextChanged(Windows.UI.Xaml.FrameworkElement sender, Windows.UI.Xaml.DataContextChangedEventArgs args)
        {
            if (this.DataContext != null)
                MusicViewSet(Locator.MusicLibraryVM.MusicView);
        }

        void MusicViewSet(MusicView v)
        {
            if (v == (MusicView)this.DataContext)
                Title.Foreground = (Brush)App.Current.Resources["MainColor"];
            else
                if (savedBrush != null)
                    Title.Foreground = savedBrush;
        }
    }
}
