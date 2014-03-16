/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System;
using System.Diagnostics;
using VLC_WINRT.Common;
using VLC_WINRT.Utility.IoC;
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
        private IAsyncAction _thumbailGeneration;

        public ThumbnailViewModel(StorageFile storageFile)
        {
            File = storageFile;
            _thumbsService = IoC.GetInstance<IThumbnailService>();
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
                _thumbailGeneration = ThreadPool.RunAsync(GenerateThumbnail);
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
                await DispatchHelper.InvokeAsync(async () =>
                                          {
                                              var image = new BitmapImage();
                                              await image.SetSourceAsync(thumb);
                                              Image = image;
                                          });
            }
        }
    }
}
