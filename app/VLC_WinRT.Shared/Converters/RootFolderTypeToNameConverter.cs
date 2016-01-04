using System;
using VLC_WinRT.Utils;
using Windows.UI.Xaml.Data;
using VLC_WinRT.Model.FileExplorer;

namespace VLC_WinRT.Converters
{
    public class RootFolderTypeToNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is RootFolderType)
            {
                switch ((RootFolderType)value)
                {
                    case RootFolderType.ExternalDevice:
                        return Strings.RemovableStorage.ToUpperFirstChar();
                    case RootFolderType.Library:
                        return Strings.Library.ToUpperFirstChar();
                    case RootFolderType.Network:
                        return Strings.Network.ToUpperFirstChar();
                }
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
