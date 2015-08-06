using System;
using Windows.Graphics.Display;
using Windows.UI.Xaml.Controls;
namespace VLC_WinRT.UI.Legacy.Views.UserControls
{
    public sealed partial class TitleBar : UserControl
    {
        public TitleBar()
        {
            this.InitializeComponent();
            this.Height = Math.Floor(32 * (DisplayInformation.GetForCurrentView().LogicalDpi / 100));
        }
    }
}
