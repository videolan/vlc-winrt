using System;
using Windows.UI.Xaml.Data;
using VLC.Model;
using VLC.Utils;

namespace VLC.Converters
{
    public class LanguageStringifyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is Languages)
            {
                switch ((Languages)value)
                {
                    case Languages.English:
                        return Strings.EnglishLanguage;
                    case Languages.Japanese:
                        return Strings.JapaneseLanguage;
                    case Languages.French:
                        return Strings.FrenchLanguage;
                    default:
                        throw new NotImplementedException();
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
