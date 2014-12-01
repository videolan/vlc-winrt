using Windows.UI.Xaml.Controls;
using VLC_WINRT.Common;
using VLC_WINRT_APP.Helpers;
using VLC_WINRT_APP.Model.Search;

namespace VLC_WINRT_APP.Commands
{
    public class SearchClickedCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            if (parameter is ItemClickEventArgs)
            {
                var item = (parameter as ItemClickEventArgs).ClickedItem as SearchResult;
                SearchHelpers.OpenSearchItem(item.SearchItemType, item.Text, item.Id);
            }
            else
            {

            }
        }
    }
}
