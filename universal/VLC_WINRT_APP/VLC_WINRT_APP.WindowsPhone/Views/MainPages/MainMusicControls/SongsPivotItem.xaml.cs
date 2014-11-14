using System;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using ScrollWatchedSelector;

namespace VLC_WINRT_APP.Views.MainPages.MainMusicControls
{
    public sealed partial class SongsPivotItem : UserControl
    {
        public SongsPivotItem()
        {
            this.InitializeComponent();
        }

        private async void RadDataGrid_OnGoingTopOrBottom(IScrollWatchedSelector lv, EventArgs eventArgs)
        {
            var e = eventArgs as ScrollingEventArgs;
            if (e.ScrollingType == ScrollingType.ToBottom)
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => FadeOutHeader.Begin());
            }
            else if (e.ScrollingType == ScrollingType.ToTop)
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => FadeInHeader.Begin());
            }
        }
    }
}
