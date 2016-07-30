using System.Threading.Tasks;
using VLC.Model.Stream;
using VLC.Utils;
using VLC.ViewModels;

namespace VLC.Commands.StreamsLibrary
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
