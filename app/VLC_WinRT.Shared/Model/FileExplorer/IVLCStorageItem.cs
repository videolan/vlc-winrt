using System;
using System.Collections.Generic;
using System.Text;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml.Media.Imaging;

namespace VLC_WinRT.Model.FileExplorer
{
    public interface IVLCStorageItem
    {
        IStorageItem StorageItem { get; }
        string Name { get; }
        string SizeHumanizedString { get; }
        bool SizeAvailable { get; }
        string LastModified { get; }
        StorageItemThumbnail Thumbnail { get; }

    }
}
