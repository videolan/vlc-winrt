using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236
using VLC_WINRT.Utility.Helpers;
using VLC_WINRT.ViewModels;
namespace VLC_WINRT.Views.Controls.MainPage
{
    public sealed partial class VideoColumn : UserControl
    {
        private int _currentSection;
        public VideoColumn()
        {
            InitializeComponent();
            this.Loaded += (sender, args) =>
            {
                for (int i = 1; i < SectionsGrid.Children.Count; i++)
                {
                    UIAnimationHelper.FadeOut(SectionsGrid.Children[i]);
                }
            };
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
            //for (int j = 0; j < SectionsHeaderListView.Items.Count; j++)
            //    Locator.MainPageVM.VideoVM.Panels[j].Opacity = 0.4;
            Locator.MainPageVM.VideoVM.Panels[i].Opacity = 1;
        }
    }
}