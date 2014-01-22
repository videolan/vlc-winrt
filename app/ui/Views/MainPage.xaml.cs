// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

using VLC_WINRT.ViewModels;

namespace VLC_WINRT.Views
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : BasePage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        public override void SetDataContext()
        {
            _vm = (NavigateableViewModel) DataContext;
        }

        private void SectionsHeaderListView_OnItemClick(object sender, Windows.UI.Xaml.Controls.ItemClickEventArgs e)
        {

        }
    }
}