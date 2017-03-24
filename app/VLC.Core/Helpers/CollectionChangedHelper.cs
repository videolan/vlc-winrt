using System.Collections.ObjectModel;
using System.Threading.Tasks;
using VLC.Utils;
using Windows.UI.Core;

namespace VLC.Helpers
{
    public static class CollectionChangedHelper
    {
        static public async Task Handle<T>(ObservableCollection<T> collection, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Move ||
                    e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace)
            {
                LogHelper.Log("Unexpected collection change: " + e.Action);
                return;
            }
            await DispatchHelper.InvokeInUIThread(CoreDispatcherPriority.Normal, () =>
            {
                switch (e.Action)
                {
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                        foreach (T v in e.NewItems)
                            collection.Add(v);
                        break;
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                        collection.Clear();
                        break;
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                        foreach (T v in e.OldItems)
                            collection.Remove(v);
                        break;
                }
            });
        }
    }
}
