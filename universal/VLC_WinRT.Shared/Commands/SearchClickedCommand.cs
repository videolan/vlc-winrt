using Windows.UI.Xaml.Controls;
using VLC_WINRT.Common;
using VLC_WinRT.Helpers;
using VLC_WinRT.Model.Search;
using VLC_WinRT.Utils;

namespace VLC_WinRT.Commands
{
    public class SearchClickedCommand : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            if (parameter is ItemClickEventArgs)
            {
                var item = (parameter as ItemClickEventArgs).ClickedItem as SearchResult;
                await SearchHelpers.OpenSearchItem(item.SearchItemType, item.Text, item.Id);
            }
            else
            {

            }
        }
    }
}
