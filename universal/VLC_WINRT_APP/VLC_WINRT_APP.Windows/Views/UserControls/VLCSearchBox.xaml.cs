using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using VLC_WINRT_APP.Helpers;
using VLC_WINRT_APP.Model.Music;
using VLC_WINRT_APP.Model.Search;
using VLC_WINRT_APP.Model.Video;
using VLC_WINRT_APP.ViewModels;
using VLC_WINRT_APP.Helpers.MusicPlayer;
using VLC_WINRT_APP.Helpers.VideoPlayer;
using VLC_WINRT_APP.Views.MusicPages;

namespace VLC_WINRT_APP.Views.UserControls
{
    public sealed partial class VLCSearchBox : UserControl
    {
        public VLCSearchBox()
        {
            this.InitializeComponent();
        }
        private void LargeSearchBox_SuggestionsRequested(SearchBox sender, SearchBoxSuggestionsRequestedEventArgs args)
        {
            SearchHelpers.Search(args.QueryText, args);
        }

        private void LargeSearchBox_QuerySubmitted(SearchBox sender, SearchBoxQuerySubmittedEventArgs args)
        {
        }

        private async void LargeSearchBox_OnResultSuggestionChosen(SearchBox sender, SearchBoxResultSuggestionChosenEventArgs args)
        {
            int separatorIndex = args.Tag.IndexOf("://", System.StringComparison.Ordinal);
            int separatorEndIndex = separatorIndex + 3;
            var type = (SearchItemType)int.Parse(args.Tag.Remove(separatorIndex));
            string query = args.Tag.Remove(0, separatorEndIndex);
            // Instead of searching the database, search the music library VM. This way we already have the track and album information and
            // don't have to call the database for it again.
            int id = 0;
            if (int.TryParse(query, out id))
            {
                SearchHelpers.OpenSearchItem(type, query, id);
            }
            else
            {
                SearchHelpers.OpenSearchItem(type, query, -1);
            }
        }
    }
}
