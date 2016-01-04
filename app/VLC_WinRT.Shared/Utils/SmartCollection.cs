using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace VLC_WinRT.Utils
{
    public class SmartCollection<T> : ObservableCollection<T>
    {
        public SmartCollection() : base()
        {

        }
        public SmartCollection(IEnumerable<T> collection)
        : base(collection)
        {
        }

        public SmartCollection(List<T> list)
            : base(list)
        {
        }

        public void AddRange(IEnumerable<T> range)
        {
            foreach (var item in range)
            {
                Items.Add(item);
            }

            this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
            this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(Items)));
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public void Reset(IEnumerable<T> range)
        {
            this.Items.Clear();

            AddRange(range);
        }
    }
}
