using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using VLC.Commands;
using VLC.Helpers;
using VLC.Model;
using VLC.Services.RunTime;
using VLC.Utils;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;

namespace VLC.ViewModels.Others
{
    public class UploaderViewModel : BindableBase
    {
        HttpServer server;
        IMediaItem mediaToUpload;
        string destIp;

        public IMediaItem MediaToUpload
        {
            get { return mediaToUpload; }
            set
            {
                SetProperty(ref mediaToUpload, value);
                OnPropertyChanged(nameof(IsReadyToUpload));
            }
        }

        public string DestinationIP
        {
            get { return destIp; }
            set
            {
                SetProperty(ref destIp, value);
                OnPropertyChanged(nameof(IsReadyToUpload));
            }
        }

        public bool IsReadyToUpload => !string.IsNullOrEmpty(DestinationIP) && mediaToUpload != null;

        public ActionCommand SelectFileToUploadCommand = new ActionCommand(async () =>
        {
            try
            {
                var picker = new FileOpenPicker
                {
                    ViewMode = PickerViewMode.List,
                    SuggestedStartLocation = PickerLocationId.VideosLibrary
                };

                foreach (var ext in VLCFileExtensions.VideoExtensions)
                    picker.FileTypeFilter.Add(ext);

                StorageFile file = null;
                file = await picker.PickSingleFileAsync();
                if (file != null)
                {
                    LogHelper.Log("Opening file: " + file.Path);
                    var videoItem = await MediaLibraryHelper.GetVideoItem(file);
                    if (videoItem != null)
                    {
                        Locator.UploaderVM.MediaToUpload = videoItem;
                    }
                }
                else
                {
                    LogHelper.Log("Cancelled");
                }

            }
            catch
            {
            }
        });

        public ActionCommand BeginUploadCommand = new ActionCommand(async () =>
        {
            Locator.UploaderVM.server = new HttpServer();

            var prepareClient = new HttpClient();
            
            var uploadRequestConfirmation = await prepareClient.GetStringAsync(new Uri(Locator.UploaderVM.DestinationIP + ":8080/" + "prepUploadRequest", UriKind.RelativeOrAbsolute));

            if (uploadRequestConfirmation == "OK")
            {

            }
        });

        public void InitializeAsReceiver()
        {
            server = new HttpServer();
            server.bind(8080);
        }
    }
}
