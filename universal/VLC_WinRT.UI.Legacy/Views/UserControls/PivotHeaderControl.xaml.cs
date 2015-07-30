using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Xaml.Interactivity;
using VLC_WinRT.Model;
using Windows.UI.Text;
using System.Diagnostics;

namespace VLC_WinRT.Views.UserControls
{
    public sealed partial class PivotHeaderControl : UserControl
    {
        public PivotHeaderControl()
        {
            this.InitializeComponent();
            Responsive();
            Window.Current.SizeChanged += Current_SizeChanged;
        }
        
        void Responsive()
        {
            if (Window.Current.Bounds.Width < 770)
            {
                VisualStateUtilities.GoToState(this, "Snap", false);
            }
            else if (Window.Current.Bounds.Width < 1000)
            {
                VisualStateUtilities.GoToState(this, "HalfSnap", false);
            }
            else
            {
                VisualStateUtilities.GoToState(this, "Normal", false);
            }
        }

        private void Current_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            Responsive();
        }
    }
}
