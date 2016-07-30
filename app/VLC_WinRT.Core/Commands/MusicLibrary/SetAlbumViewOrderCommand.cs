using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VLC_WinRT.Utils;
using VLC_WinRT.ViewModels;
using Windows.UI.Xaml.Controls;

namespace VLC_WinRT.Commands.MusicLibrary
{
    public class SetAlbumViewOrderCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            if (parameter is string)
            {
                switch (parameter.ToString())
                {
                    case nameof(Strings.OrderByArtist):
                        Locator.SettingsVM.AlbumsOrderType = Model.OrderType.ByArtist;
                        break;
                    case nameof(Strings.OrderByAlbum):
                        Locator.SettingsVM.AlbumsOrderType = Model.OrderType.ByAlbum;
                        break;
                    case nameof(Strings.OrderByDate):
                        Locator.SettingsVM.AlbumsOrderType = Model.OrderType.ByDate;
                        break;
                    default:
                        break;
                }
            }
            else if (parameter is SelectionChangedEventArgs)
            {
                var selected = ((SelectionChangedEventArgs)parameter).AddedItems[0];
                Locator.SettingsVM.AlbumsOrderType = (Model.OrderType)selected;
            }
        }

    }
}
