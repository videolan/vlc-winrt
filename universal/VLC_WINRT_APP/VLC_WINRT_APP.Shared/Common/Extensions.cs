using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace VLC_WINRT_APP
{
    public static class Extensions
    {
        public static bool Contains(this string source, string value, StringComparison comparisonType)
        {
            return source.IndexOf(value, comparisonType) >= 0;
        }

        public static ObservableCollection<T> ToObservable<T>(this IEnumerable<T> source)
        {
            return new ObservableCollection<T>(source);
        }

        public static async Task<ObservableCollection<T>> ToObservableAsync<T>(this Task<List<T>> source)
        {
            return new ObservableCollection<T>(await source);
        }
    }
}
