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
            if (!(value is Languages languages)) return "";
            switch (languages)
            {
                case Languages.English:
                    return Strings.EnglishLanguage;
                case Languages.Japanese:
                    return Strings.JapaneseLanguage;
                case Languages.French:
                    return Strings.FrenchLanguage;
                case Languages.German:
                    return Strings.GermanLanguage;
                case Languages.Polish:
                    return Strings.PolishLanguage;
                case Languages.Slovak:
                    return Strings.SlovakLanguage;
                case Languages.Danish:
                    return Strings.DanishLanguage;
                case Languages.Spanish:
                    return Strings.SpanishLanguage;
                case Languages.Hungarian:
                    return Strings.HungarianLanguage;
                case Languages.Italian:
                    return Strings.ItalianLanguage;
                case Languages.Korean:
                    return Strings.KoreanLanguage;
                case Languages.Malay:
                    return Strings.MalayLanguage;
                case Languages.Norwegian:
                    return Strings.NorwegianLanguage;
                case Languages.Dutch:
                    return Strings.DutchLanguage;
                case Languages.Russian:
                    return Strings.RussianLanguage;
                case Languages.Swedish:
                    return Strings.SwedishLanguage;
                case Languages.Turkish:
                    return Strings.TurkishLanguage;
                case Languages.Ukrainian:
                    return Strings.UkrainianLanguage;
                case Languages.Chinese:
                    return Strings.ChineseLanguage;
                case Languages.Portuguese:
                    return Strings.PortugueseLanguage;
                case Languages.Czech:
                    return Strings.CzechLanguage;
                default:
                    throw new NotImplementedException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
