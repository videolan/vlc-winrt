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

        public Model.Panel Panel
        {
            get { return (Model.Panel)GetValue(PanelProperty); }
            set
            {
                SetValue(PanelProperty, value);
            }
        }

        void SetPanel(Model.Panel p)
        {
            Icon.Glyph = p.PathData;
            Title.Text = p.Title;
            SetCurrentPanelInterface();
            p.PropertyChanged += Value_PropertyChanged;
        }

        void SetCurrentPanelInterface()
        {
            if (Panel.IsCurrent)
            {
                Title.Opacity = 1;
                Icon.Opacity = 1;
            }
            else
            {
                Title.Opacity = 0.5;
                Icon.Opacity = 0.5;
            }
        }

        private void Value_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            SetCurrentPanelInterface();
        }


        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PanelProperty = DependencyProperty.Register("Panel", typeof(Model.Panel), typeof(PivotHeaderControl), new PropertyMetadata(default(Model.Panel), PanelPropertyChangedCallback));

        private static void PanelPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var that = (PivotHeaderControl)dependencyObject;
            that.SetPanel((Model.Panel)dependencyPropertyChangedEventArgs.NewValue);
        }

        void Responsive()
        {
            if (Window.Current.Bounds.Width < 600)
            {
                VisualStateUtilities.GoToState(this, "Snap", false);
            }
            else if (Window.Current.Bounds.Width < 800)
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
