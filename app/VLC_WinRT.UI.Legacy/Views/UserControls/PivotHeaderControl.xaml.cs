using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Xaml.Interactivity;
using VLC_WinRT.Model;
using VLC_WinRT.ViewModels;

namespace VLC_WinRT.Views.UserControls
{
    public sealed partial class PivotHeaderControl : UserControl
    {
        public PivotHeaderControl()
        {
            this.InitializeComponent();
            this.Loaded += PivotHeaderControl_Loaded;
            Responsive();
            Window.Current.SizeChanged += Current_SizeChanged;
        }
        

        public Model.Panel Panel
        {
            get { return (Model.Panel)GetValue(PanelProperty); }
            set { SetValue(PanelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Panel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PanelProperty =
            DependencyProperty.Register(nameof(Panel), typeof(Model.Panel), typeof(PivotHeaderControl), new PropertyMetadata(null, PropertyChangedCallback));
        
        private static void PropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var that = (PivotHeaderControl)dependencyObject;
            that.Init();
        }

        public void Init()
        {
            Icon.Glyph = Panel.DefaultIcon;
            Title.Text = Panel.Title;
            UpdatePivot();
        }

        private void PivotHeaderControl_Loaded(object sender, RoutedEventArgs e)
        {
            Locator.MainVM.PropertyChanged += MainVM_PropertyChanged;
            this.Unloaded += PivotHeaderControl_Unloaded;
        }

        private void PivotHeaderControl_Unloaded(object sender, RoutedEventArgs e)
        {
            Locator.MainVM.PropertyChanged -= MainVM_PropertyChanged;
        }

        private void MainVM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainVM.CurrentPanel))
            {
                UpdatePivot();
            }
        }

        void UpdatePivot()
        {
            if (Panel.Target == Locator.MainVM.CurrentPanel.Target)
            {
                Icon.Glyph = Panel.FilledIcon;
            }
            else
            {
                Icon.Glyph = Panel.DefaultIcon;
            }
        }

        void Responsive()
        {
            if (Window.Current.Bounds.Width < 1000)
            {
                VisualStateUtilities.GoToState(this, nameof(HalfSnap), false);
            }
            else
            {
                VisualStateUtilities.GoToState(this, nameof(Normal), false);
            }
        }

        private void Current_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            Responsive();
        }
    }
}
