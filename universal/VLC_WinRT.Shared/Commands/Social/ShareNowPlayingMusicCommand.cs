using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using System;
using System.Collections.Generic;
using Windows.Storage;
using VLC_WinRT.Utils;
using VLC_WinRT.ViewModels;
using WinRTXamlToolkit.IO.Extensions;

namespace VLC_WinRT.Commands.Social
{
    public class ShareNowPlayingMusicCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            if (Locator.MusicPlayerVM.CurrentTrack == null || Locator.MusicPlayerVM.CurrentAlbum == null ||
                Locator.MusicPlayerVM.CurrentAlbum == null) return;
            RegisterForShare();
            DataTransferManager.ShowShareUI();
        }

        private void RegisterForShare()
        {
            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += new TypedEventHandler<DataTransferManager, DataRequestedEventArgs>(this.ShareLinkHandler);
        }

        private async void ShareLinkHandler(DataTransferManager sender, DataRequestedEventArgs e)
        {
            DataRequest request = e.Request;
            var uri = string.Format("http://www.last.fm/music/{0}/{1}", Locator.MusicPlayerVM.CurrentArtist.Name, Locator.MusicPlayerVM.CurrentAlbum.Name);
            var title = string.Format("#NowPlaying {0} - {1}", Locator.MusicPlayerVM.CurrentTrack.Name, Locator.MusicPlayerVM.CurrentArtist.Name);

            request.Data.Properties.Title = title;
            request.Data.Properties.Description = title;
            request.Data.SetWebLink(new Uri(uri, UriKind.Absolute));

            DataRequestDeferral deferral = request.GetDeferral();
            try
            {
                string fileName = string.Format("{0}.jpg", Locator.MusicPlayerVM.CurrentAlbum.Id);
                var albumPic = await ApplicationData.Current.LocalFolder.GetFolderAsync("albumPic");
                if (await albumPic.ContainsFileAsync(fileName))
                {
                    var file = await albumPic.GetFileAsync(fileName);
                    request.Data.SetStorageItems(new List<StorageFile> { file });
                }
            }
            catch
            {
                deferral.Complete();
            }
            finally
            {
                deferral.Complete();
            }
        }
    }
}
