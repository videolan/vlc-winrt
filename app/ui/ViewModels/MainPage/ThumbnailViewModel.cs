using System;
using System.Diagnostics;
using VLC_WINRT.Common;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.System.Threading;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace VLC_WINRT.ViewModels.MainPage
{
    public class ThumbnailViewModel : BindableBase
    {
        public ThumbnailViewModel(StorageFile storageFile)
        {
            File = storageFile;
        }

        private StorageFile _file;
        private ImageSource _imageBrush;

        public ImageSource Image
        {
            get { return _imageBrush; }
            set { SetProperty(ref _imageBrush, value); }
        }

        public StorageFile File
        {
            get { return _file; }
            set
            {
                SetProperty(ref _file, value);
                ThreadPool.RunAsync(GenerateThumbnail);
            }
        }

        private async void GenerateThumbnail(IAsyncAction operation)
        {
            //TODO: make this whole thing keyed on an enum
            try
            {
                StorageItemThumbnail thumb = await File.GetThumbnailAsync(ThumbnailMode.VideosView);

                DispatchHelper.Invoke(() =>
                                          {
                                              var image = new BitmapImage();
                                              image.SetSource(thumb);
                                              Image = image;
                                          });
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error getting thumbnail");
                Debug.WriteLine(ex);
            }
        }
    }
}