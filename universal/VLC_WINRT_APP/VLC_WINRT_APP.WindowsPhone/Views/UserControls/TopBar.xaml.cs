using System;
using System.Diagnostics;
using Windows.UI;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace VLC_WINRT_APP.Views.UserControls
{
    public sealed partial class TopBar : UserControl
    {
        public TopBar()
        {
            this.InitializeComponent();
            this.Loaded += (sender, args) => this.DataContext = this;
        }

        public static readonly DependencyProperty HeaderTextBlockProperty = DependencyProperty.Register(
            "HeaderTextBlock", typeof (String), typeof (TopBar), new PropertyMetadata(default(String), HeaderTextBlockPropertyChangedCallback));

        private static void HeaderTextBlockPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            (dependencyObject as TopBar).HeaderTextBlock = dependencyPropertyChangedEventArgs.NewValue.ToString();
        }

        public String HeaderTextBlock
        {
            get { return (String) GetValue(HeaderTextBlockProperty); }
            set
            {
                SetValue(HeaderTextBlockProperty, value);
            }
        }

        public static readonly DependencyProperty HeaderBackgroundProperty = DependencyProperty.Register(
            "HeaderBackground", typeof (SolidColorBrush), typeof (TopBar), new PropertyMetadata(App.Current.Resources["MainColor"], HeaderBackgroundPropertyChangedCallback));

        private static void HeaderBackgroundPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            (dependencyObject as TopBar).HeaderBackground = ((SolidColorBrush)dependencyPropertyChangedEventArgs.NewValue);
        }

        public SolidColorBrush HeaderBackground
        {
            get { return (SolidColorBrush)GetValue(HeaderBackgroundProperty); }
            set
            {
                SetValue(HeaderBackgroundProperty, value);
                
            }
        }

        private void Menu_Click(object sender, TappedRoutedEventArgs tappedRoutedEventArgs)
        {
            App.RootPage.PanelsView.ShowSidebar();
        }
    }
}
