using Microsoft.Graphics.Canvas.Effects;
using System;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;

namespace VLC_WinRT.UI.UWP.Views.UserControls
{
    public class BackDrop : Control
    {
        public double BlurAmount
        {
            get
            {
                float value = 0;
                m_rootVisual.Properties.TryGetScalar(BlurAmountProperty, out value);
                return value;
            }
            set
            {
                newBlurAmount = (float)value;
                RefreshBlur();
            }
        }

        Compositor m_compositor;
        SpriteVisual m_blurVisual;
        CompositionBrush m_blurBrush;
        Visual m_rootVisual;

        float oldBlurAmount = 0;
        float newBlurAmount = 0;
        bool m_setUpExpressions;

        public BackDrop()
        {
            m_rootVisual = ElementCompositionPreview.GetElementVisual(this as UIElement);
            Compositor = m_rootVisual.Compositor;

            m_blurVisual = Compositor.CreateSpriteVisual();
        
            CompositionEffectBrush brush = BuildBlurBrush();
            brush.SetSourceParameter("source", m_compositor.CreateBackdropBrush());
            m_blurBrush = brush;
            m_blurVisual.Brush = m_blurBrush;

            ElementCompositionPreview.SetElementChildVisual(this as UIElement, m_blurVisual);

            this.Loading += OnLoading;
            this.Unloaded += OnUnloaded;
        }

        public const string BlurAmountProperty = nameof(BlurAmount);

        public Compositor Compositor
        {
            get
            {
                return m_compositor;
            }

            private set
            {
                m_compositor = value;
            }
        }

#pragma warning disable 1998
        private async void OnLoading(FrameworkElement sender, object args)
        {
            this.SizeChanged += OnSizeChanged;
            OnSizeChanged(this, null);
            
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            this.SizeChanged -= OnSizeChanged;
        }


        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (m_blurVisual != null)
            {
                m_blurVisual.Size = new System.Numerics.Vector2((float)this.ActualWidth, (float)this.ActualHeight);
            }
        }
        
        private void SetUpPropertySetExpressions()
        {
            m_setUpExpressions = true;

            var exprAnimation = Compositor.CreateExpressionAnimation();
            exprAnimation.Expression = $"sourceProperties.{BlurAmountProperty}";
            exprAnimation.SetReferenceParameter("sourceProperties", m_rootVisual.Properties);

            m_blurBrush.Properties.StartAnimation("Blur.BlurAmount", exprAnimation);
        }


        private CompositionEffectBrush BuildBlurBrush()
        {
            GaussianBlurEffect blurEffect = new GaussianBlurEffect()
            {
                Name = "Blur",
                BlurAmount = 0.0f,
                BorderMode = EffectBorderMode.Hard,
                Optimization = EffectOptimization.Balanced,
                Source = new CompositionEffectSourceParameter("source"),
            };
            return Compositor.CreateEffectFactory(blurEffect, new[] { "Blur.BlurAmount" }).CreateBrush();
        }

        public CompositionPropertySet VisualProperties
        {
            get
            {
                if (!m_setUpExpressions)
                {
                    SetUpPropertySetExpressions();
                }
                return m_rootVisual.Properties;
            }
        }
        
        public void Show(double blur)
        {
            newBlurAmount = (float)blur;
            oldBlurAmount = 0;
            RefreshBlur();
        }

        public void Hide()
        {
            oldBlurAmount = newBlurAmount;
            newBlurAmount = 0f;
            RefreshBlur();
        }

        public void RefreshBlur()
        {
            if (!m_setUpExpressions)
            {
                m_blurBrush.Properties.InsertScalar("Blur.BlurAmount", 0f);
            }
            m_rootVisual.Properties.InsertScalar(BlurAmountProperty, 0f);

            var blurAnim = Compositor.CreateScalarKeyFrameAnimation();
            blurAnim.Duration = TimeSpan.FromSeconds(1);
            blurAnim.InsertKeyFrame(0.0f, oldBlurAmount);
            blurAnim.InsertKeyFrame(1f, newBlurAmount);
            blurAnim.StopBehavior = AnimationStopBehavior.SetToFinalValue;
            blurAnim.IterationBehavior = AnimationIterationBehavior.Count;
            blurAnim.IterationCount = 1;
        
            this.VisualProperties.StartAnimation(BackDrop.BlurAmountProperty, blurAnim);
            oldBlurAmount = newBlurAmount;
        }
    }
}