using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using VLC.Commands;
using VLC.Helpers;
using VLC.Model;
using VLC.Services.RunTime;
using VLC.Utils;
using Windows.Storage;
using Windows.Storage.Pickers;

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
            Locator.UploaderVM.server = new HttpServer(8080);

            var prepareClient = new HttpClient();
            
            var uploadRequestConfirmation = await prepareClient.GetStringAsync(new Uri(Locator.UploaderVM.DestinationIP + ":8080/" + "prepUploadRequest", UriKind.RelativeOrAbsolute));

            if (uploadRequestConfirmation == "OK")
            {

            }
        });

        public void InitializeAsReceiver()
        {
            server = new HttpServer(8080);
            server.UploadRequestCallback += UploadRequest;
        }

        async void UploadRequest(object source, string remoteAdress)
        {
            var httpClient = new HttpClient();
            var stream = await httpClient.GetByteArrayAsync(new Uri($"http://{remoteAdress}:8080/fileRequest", UriKind.RelativeOrAbsolute));
            
            var testFile = await ApplicationData.Current.LocalFolder.CreateFileAsync("testFile.mp4", CreationCollisionOption.ReplaceExisting);
            var writeStream = await testFile.OpenAsync(FileAccessMode.ReadWrite);
            using (var outputStream = writeStream.GetOutputStreamAt(0))
            {
                using (var dataWriter = new Windows.Storage.Streams.DataWriter(outputStream))
                {
                    dataWriter.WriteBytes(stream);
                }
            }

            writeStream.Dispose(); // Or use the stream variable (see previous code snippet) with a using statement as well.
        }
    }
}
