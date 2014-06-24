using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VLC_WINRT.ViewModels.MainPage.VlcExplorer;

namespace VLC_WINRT_APP.DataTemplateSelectors
{
    public class StorageItemDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate StorageFolderDataTemplate { get; set; }

        public DataTemplate StorageFileDataTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            base.SelectTemplateCore(item, container);
            if (item is FileExplorerViewModel)
            {
                return StorageFolderDataTemplate;
            }
            else
            {
                return StorageFileDataTemplate;
            }
        }
    }
}
