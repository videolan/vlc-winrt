using Windows.UI.Xaml.Controls;
using VLC_WINRT.ViewModels.MainPage;

namespace VLC_WINRT.Views.Controls
{
    public class VariableSizedItemsGridView : GridView
    {
        private int i = -1;
        protected override void PrepareContainerForItemOverride(Windows.UI.Xaml.DependencyObject element, object item)
        {
            var _Item = item as MediaViewModel;
            i++;
            if (i == 0)
            {
                element.SetValue(VariableSizedWrapGrid.ColumnSpanProperty, 2);
                element.SetValue(VariableSizedWrapGrid.RowSpanProperty, 2);
            }

            base.PrepareContainerForItemOverride(element, item);
        }
    }
}
