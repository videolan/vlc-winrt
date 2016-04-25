using System.Threading.Tasks;
using VLC_WinRT.Model.Stream;
using VLC_WinRT.Utils;
using VLC_WinRT.ViewModels;

namespace VLC_WinRT.Commands.VideoLibrary
{
    public class DeleteStreamCommand : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            if (parameter is StreamMedia)
            {
                await Locator.MediaLibrary.RemoveStreamFromCollectionAndDatabase(parameter as StreamMedia);
            }
        }
    }
}
