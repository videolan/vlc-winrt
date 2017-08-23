using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace VLC.Universal8._1.Views.UserControls
{
    public sealed partial class PlayPauseButtonControl : UserControl
    {
        public object ButtonStyle
        {
            get { return (object)GetValue(ButtonStyleProperty); }
            set { SetValue(ButtonStyleProperty, value); }
        }

        /// <summary>
        /// Identified the Label dependency property
        /// </summary>
        public static readonly DependencyProperty ButtonStyleProperty =
            DependencyProperty.Register("ButtonStyle", typeof(object),
              typeof(PlayPauseButtonControl), new PropertyMetadata(""));

        public double ButtonFontSize
        {
            get { return (double)GetValue(ButtonFontSizeProperty); }
            set { SetValue(ButtonFontSizeProperty, value); }
        }

        /// <summary>
        /// Identified the Label dependency property
        /// </summary>
        public static readonly DependencyProperty ButtonFontSizeProperty =
            DependencyProperty.Register("ButtonFontSize", typeof(double),
              typeof(PlayPauseButtonControl), new PropertyMetadata((double)11));

        public PlayPauseButtonControl()
        {
            this.InitializeComponent();
        }
    }
}
