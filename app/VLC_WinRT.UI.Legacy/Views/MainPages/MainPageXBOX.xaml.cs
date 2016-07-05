using System;
using VLC_WinRT.Helpers;
using VLC_WinRT.ViewModels;
using Windows.UI.Xaml.Controls;

namespace VLC_WinRT.UI.Legacy.Views.MainPages
{
    public sealed partial class MainPageXBOX
    {
        public MainPageXBOX()
        {
            this.InitializeComponent();
            AppViewHelper.SetAppView(true);
            PanelsListView.Focus(Windows.UI.Xaml.FocusState.Keyboard);
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var model = e.ClickedItem as Model.Panel;
            Locator.NavigationService.Go(model.Target);
        }
    }
}
