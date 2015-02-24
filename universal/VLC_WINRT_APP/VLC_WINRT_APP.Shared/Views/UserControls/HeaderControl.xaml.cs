using System;
using System.Diagnostics;
using Windows.UI;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using VLC_WINRT_APP.Helpers;
using WinRTXamlToolkit.Controls.Extensions;

namespace VLC_WINRT_APP.Views.UserControls
{
    public sealed partial class HeaderControl : UserControl
    {
        public HeaderControl()
        {
            this.InitializeComponent();
            this.Loaded += (sender, args) => this.DataContext = this;
        }

        public static readonly DependencyProperty HeaderTextBlockProperty = DependencyProperty.Register(
            "HeaderTextBlock", typeof(String), typeof(HeaderControl), new PropertyMetadata(default(String), HeaderTextBlockPropertyChangedCallback));

        private static void HeaderTextBlockPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            (dependencyObject as HeaderControl).HeaderTextBlock = dependencyPropertyChangedEventArgs.NewValue.ToString();
        }

        public String HeaderTextBlock
        {
            get { return (String)GetValue(HeaderTextBlockProperty); }
            set
            {
                SetValue(HeaderTextBlockProperty, value);
            }
        }

        public static readonly DependencyProperty HeaderBackgroundProperty = DependencyProperty.Register(
            "HeaderBackground", typeof(SolidColorBrush), typeof(HeaderControl), new PropertyMetadata(new SolidColorBrush(Colors.Transparent), HeaderBackgroundPropertyChangedCallback));

        private static void HeaderBackgroundPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            (dependencyObject as HeaderControl).HeaderBackground = ((SolidColorBrush)dependencyPropertyChangedEventArgs.NewValue);
        }

        public SolidColorBrush HeaderBackground
        {
            get { return (SolidColorBrush)GetValue(HeaderBackgroundProperty); }
            set
            {
                SetValue(HeaderBackgroundProperty, value);

            }
        }

        private void BackButton_Loaded(object sender, RoutedEventArgs e)
        {
#if WINDOWS_PHONE_APP
#else
            sender = PathHelper.CreateGeometry((sender as Path), App.Current.Resources["BackPath"].ToString());
#endif
        }

        private void GoBack_Click(object sender, RoutedEventArgs e)
        {
            var flyout = this.GetFirstAncestorOfType<SettingsFlyout>();
            if (flyout != null) flyout.Hide();
            else if (App.ApplicationFrame.CanGoBack) App.ApplicationFrame.GoBack();
        }

        private void RootGrid_loaded(object sender, RoutedEventArgs e)
        {
#if WINDOWS_PHONE_APP
            (sender as Button).HorizontalContentAlignment=HorizontalAlignment.Left;
#endif
        }
    }
}
