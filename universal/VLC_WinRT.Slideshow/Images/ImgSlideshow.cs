using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.Numerics;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VLC_WinRT.Slideshow.Texts;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;

namespace Slide2D.Images
{
    public abstract class ImgSlideshow
    {
        private const int IntroFrameThreshold = 200;
        private const int OutroFrameThreshold = 900;
        private const int EndFrameThreshold = 1200;
        // Debug only, to slow down frames
        int threshold = 0;

        // ------
        int x;
        int y;
        int frame = 0;
        private float zoom = 0;
        public bool UseDefaultPic;
        public Img DefaultImg;
        public List<Img> Imgs = new List<Img>();
        public List<Txt> Texts = new List<Txt>();

        private int ImgIndex = 0;
        private Img currentImg
        {
            get
            {
                if (UseDefaultPic)
                    return DefaultImg;
                if (ImgIndex >= Imgs.Count)
                    return null;
                return Imgs[ImgIndex];
            }
        }

        private bool _richAnimations;

        public bool RichAnimations
        {
            get { return _richAnimations; }
            set { _richAnimations = value; }
        }

        GaussianBlurEffect bl;
        
        public void CreateResources(ref CanvasAnimatedControl sender, List<Txt> txtQueue)
        {
            Texts.Clear();
            foreach (var txt in txtQueue)
            {
                Texts.Add(txt);
            }
            txtQueue.Clear();
        }

        public void Draw(CanvasAnimatedDrawEventArgs args)
        {
            if (currentImg == null) return;

            if (currentImg.GaussianBlurCache == null)
            {
                currentImg.GaussianBlurCache = new GaussianBlurEffect()
                {
                    Source = currentImg.Bmp,
                    BlurAmount = 1.0f,
                    Optimization = EffectOptimization.Speed
                };
            }

            if (frame <= OutroFrameThreshold)
            {
                if (frame == 0)
                {
                    // Set Default values
                    currentImg.Opacity = 0;
                }
                if (_richAnimations)
                    zoom += 0.0001f;
            }
            else if (frame < 1000)
            {
                if (_richAnimations)
                    zoom += 0.005f;
            }

            float screenRatio = (float)MetroSlideshow.WindowWidth/(float)MetroSlideshow.WindowHeight;
            float imgRatio = (float) currentImg.Width/(float) currentImg.Height;
            if (imgRatio > screenRatio)
            {
                //img wider than screen, need to scale horizontally
                currentImg.Scale = (float)(MetroSlideshow.WindowHeight / currentImg.Height);
            }
            else
            {
                currentImg.Scale = (float)(MetroSlideshow.WindowWidth / currentImg.Width);
            }
            
            var scaleEffect = new ScaleEffect()
            {
                Source = currentImg.GaussianBlurCache,
                Scale = new Vector2()
                {
                    X = currentImg.Scale + zoom,
                    Y = currentImg.Scale + zoom
                },
            };

            scaleEffect.CenterPoint = new Vector2()
            {
                X = 0,
                Y = 0
            };

            if (frame < IntroFrameThreshold)
            {
                currentImg.Opacity += 0.0016f;
            }
            else if (frame > OutroFrameThreshold)
            {
                currentImg.Opacity -= 0.0027f;
            }

            foreach (var text in Texts)
            {
                text.Draw(ref args);
            }

            args.DrawingSession.DrawImage(scaleEffect, new Vector2(), new Rect()
            {
                Height = MetroSlideshow.WindowHeight,
                Width = MetroSlideshow.WindowWidth
            }, currentImg.Opacity);
            
            x = x + 2;
            y++;

            threshold++;

            if (threshold == 1)
            {
                threshold = 0;
                frame++;
            }
            if (frame == EndFrameThreshold)
            {
                // Resetting variables
                if (ImgIndex < Imgs.Count - 1)
                {
                    ImgIndex++;
                }
                else
                {
                    ImgIndex = 0;
                }
                frame = 0;
                zoom = 0;
            }
        }

        public void ChangePicFast()
        {
            // seek to outro
            frame = 0;
        }
    }
}
