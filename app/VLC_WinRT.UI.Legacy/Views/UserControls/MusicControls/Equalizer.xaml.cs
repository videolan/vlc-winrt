using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using VLC_WinRT.Model;
using VLC_WinRT.Utils;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

namespace VLC_WinRT.UI.Legacy.Views.UserControls.MusicControls
{
    public sealed partial class Equalizer : UserControl
    {
        public Equalizer()
        {
            this.InitializeComponent();
        }

        public VLCEqualizer EqualizerValue
        {
            get { return (VLCEqualizer)GetValue(EqualizerValueProperty); }
            set { SetValue(EqualizerValueProperty, value); }
        }

        public static readonly DependencyProperty EqualizerValueProperty =
            DependencyProperty.Register(nameof(EqualizerValue), typeof(VLCEqualizer), typeof(Equalizer), new PropertyMetadata(null, EqualizerPropertyChangedCallback));

        private static void EqualizerPropertyChangedCallback(DependencyObject dO, DependencyPropertyChangedEventArgs args)
        {
            var that = (Equalizer)dO;
            that.SetEqualizerUI(args.NewValue as VLCEqualizer);
        }

        public async void SetEqualizerUI(VLCEqualizer eq)
        {
            if (eq == null)
                return;

            await DispatchHelper.InvokeAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    for (int i = 0; i < eq.Amps.Count; i++)
                    {
                        var anim = new DoubleAnimation();
                        anim.Duration = TimeSpan.FromMilliseconds(600);
                        anim.EasingFunction = new ExponentialEase()
                        {
                            EasingMode = EasingMode.EaseOut,
                            Exponent = 3,
                        };
                        anim.To = Convert.ToDouble(eq.Amps[i]);
                        anim.EnableDependentAnimation = true;

                        Storyboard.SetTarget(anim, RootGrid.Children[i]);
                        Storyboard.SetTargetProperty(anim, nameof(Slider.Value));

                        var sb = new Storyboard();
                        sb.Children.Add(anim);
                        sb.Begin();
                    }
                });
        }
    }
}
