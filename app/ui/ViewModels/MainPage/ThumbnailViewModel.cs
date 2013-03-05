using System;
using System.Diagnostics;
using GalaSoft.MvvmLight.Ioc;
using VLC_WINRT.Common;
using VLC_WINRT.Utility.Services.Interface;
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
        private StorageFile _file;
        private ImageSource _imageBrush;
        private readonly IThumbnailService _thumbsService;


        public ThumbnailViewModel(StorageFile storageFile)
        {
            File = storageFile;
            _thumbsService = SimpleIoc.Default.GetInstance<IThumbnailService>();
        }

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
            StorageItemThumbnail thumb = null;
            try
            {
                thumb = await _thumbsService.GetThumbnail(File);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
            
            if (thumb != null)
            {
                DispatchHelper.Invoke(() =>
                                          {
                                              var image = new BitmapImage();
                                              image.SetSourceAsync(thumb);
                                              Image = image;
                                          });
            }
        }
    }
}