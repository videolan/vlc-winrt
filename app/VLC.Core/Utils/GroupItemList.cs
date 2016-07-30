using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace VLC.Utils
{
    public class GroupItemList<T> : SmartCollection<T>
    {
        public GroupItemList(T item) : base(new[] { item })
        {

        }

        public GroupItemList()
        {

        }

        public object Key { get; set; }

        public new IEnumerator GetEnumerator()
        {
            return (IEnumerator)base.GetEnumerator();
        }
    }
}
