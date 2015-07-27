using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.Numerics;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;

namespace Slide2D.Images
{
    public abstract class ImgSlideshow
    {
        // Debug only, to slow down frames
        int threshold = 0;

        // ------
        int x;
        int y;
        int frame = 0;
        private float zoom = 0;
        private List<Img> Imgs = new List<Img>();
        private int ImgIndex = 0;
        private Img currentImg
        {
            get
            {
                if (ImgIndex >= Imgs.Count) return null;
                return Imgs[ImgIndex];
            }
        }
        
        Color col = Color.FromArgb(150, 255, 255, 255);
        GaussianBlurEffect bl;
        public void AddImg(Img img)
        {
            Imgs.Add(img);
        }

        public void CreateResources(ref CanvasAnimatedControl sender, ref List<Img> imgQueue)
        {
            foreach (var img in imgQueue)
            {
                AddImg(img);
            }
            imgQueue.Clear();
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

            if (currentImg.Width > MetroSlideshow.WindowWidth)
            {
                currentImg.Scale = (float)(MetroSlideshow.WindowHeight / currentImg.Height);
            }
            else
            {
                currentImg.Scale = (float)(MetroSlideshow.WindowWidth / currentImg.Width);
            }
            if (frame <= 900)
            {
                if (frame == 0)
                {
                    // Set Default values
                    currentImg.Opacity = 0;
                }
                else if (frame < 200)
                {
                }
                zoom += 0.0001f;
            }
            else if (frame < 1000)
            {
                zoom += 0.005f;
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

            if (frame < 200)
            {
                currentImg.Opacity += 0.002f;
            }
            else if (frame > 900)
            {
                currentImg.Opacity -= 0.003f;
            }

            foreach(var text in Texts)
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
            if (frame == 1200)
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
    }
}
