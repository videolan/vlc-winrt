using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace VLC_WinRT.Utils
{
    public class GroupItemList<T> : SmartCollection<T>
    {
        public GroupItemList(T item) : base(new[] { item })
        {

        }

        public GroupItemList()
        {

        }

        public string Key { get; set; }

        public new IEnumerator GetEnumerator()
        {
            return (IEnumerator)base.GetEnumerator();
        }
    }
}
