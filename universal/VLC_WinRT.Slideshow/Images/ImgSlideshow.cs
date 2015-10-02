using Microsoft.Graphics.Canvas.Effects;
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
using Windows.UI.Xaml.Media;
using Microsoft.Graphics.Canvas.Brushes;
using VLC_WinRT.ViewModels;
#if WINDOWS_APP
// This namespace only works on Windows/Windows Phone 8.1 apps.
using Microsoft.Graphics.Canvas.Numerics;
#endif

namespace Slide2D.Images
{
    public abstract class ImgSlideshow
    {
        private const int IntroFrameThreshold = 200;
        private const int OutroFrameThreshold = 900;
        private const int EndFrameThresholdDefaultPic = 1020;
        private const int EndFrameThreshold = 1100;

        // Debug only, to slow down frames
        int threshold = 0;

        // ------
        public bool TextInSlideshowEnabled;
        public bool UseDefaultPic;
        private bool nextIsDefaultPic;

        private int frame = 0;
        private bool fastChange;
        private float blurAmount = 10;
        public Img DefaultImg;
        public List<Img> Imgs = new List<Img>();
        public List<Txt> Texts = new List<Txt>();
        private int ImgIndex = 0;
        private int txtIndex = 0;

        private Img currentImg
        {
            get
            {
                try
                {
                    if (UseDefaultPic)
                        return DefaultImg;
                    if (ImgIndex >= Imgs.Count)
                        return null;
                    return Imgs[ImgIndex];
                }
                catch
                {
                    return null;
                }
            }
        }

        private bool _richAnimations;

        public bool RichAnimations
        {
            get { return _richAnimations; }
            set { _richAnimations = value; }
        }

        public void Update(CanvasAnimatedUpdateEventArgs args)
        {
            if (currentImg == null) return;


            bool computeBlurPic = true;
            if (frame < IntroFrameThreshold)
            {
                if (frame == 0)
                {
                    // Set Default values
                    currentImg.Opacity = 0;
                    blurAmount = 10;
                }
                blurAmount -= 0.035f;
            }
            else if (frame <= OutroFrameThreshold)
            {
                if (_richAnimations)
                {

                }

                if (currentImg.GaussianBlurCache != null)
                {
                    computeBlurPic = false;
                }
            }
            else if (frame < 1000)
            {
                if (_richAnimations)
                {

                }
                blurAmount += 0.035f;
            }

            if (computeBlurPic)
            {
                Debug.WriteLine("Computing blur img");
                currentImg.GaussianBlurCache = new GaussianBlurEffect()
                {
                    Source = currentImg.Bmp,
                    BlurAmount = blurAmount,
                    Optimization = EffectOptimization.Speed
                };
            }

            float screenRatio = (float)MetroSlideshow.WindowWidth / (float)MetroSlideshow.WindowHeight;
            float imgRatio = (float)currentImg.Width / (float)currentImg.Height;
            if (imgRatio > screenRatio)
            {
                //img wider than screen, need to scale horizontally
                currentImg.Scale = (float)(MetroSlideshow.WindowHeight / currentImg.Height);
            }
            else
            {
                currentImg.Scale = (float)(MetroSlideshow.WindowWidth / currentImg.Width);
            }



            // "Vector2" requires System.Numerics for UWP. But for some reason ScaleEffect can only use Windows.Foundation.Numerics,
            // which you can't use to make vectors. So... we can't use this yet until we can figure out what's wrong here.

#if WINDOWS_APP
            var scaleEffect = new ScaleEffect()
            {
                Source = currentImg.GaussianBlurCache,
                Scale = new Vector2()
                {
                    X = currentImg.Scale,
                    Y = currentImg.Scale
                },
            };

            scaleEffect.CenterPoint = new Vector2()
            {
                X = 0,
                Y = 0
            };

            currentImg.ScaleEffect = scaleEffect;
#endif

            if (frame < IntroFrameThreshold)
            {
                currentImg.Opacity += 0.0013f;
            }
            else if (frame > OutroFrameThreshold)
            {
                var decrease = 0.0027f;
                if (fastChange)
                {
                    currentImg.Opacity -= decrease * 4;
                }
                else
                {
                    currentImg.Opacity -= decrease;
                }
            }
        }

        public void Draw(CanvasAnimatedDrawEventArgs args)
        {
            if (currentImg?.ScaleEffect == null) return;
            if (TextInSlideshowEnabled)
            {
                var txts = Texts.ToList();
                foreach (var text in txts)
                {
                    text.Draw(ref args, ref txts);
                }
            }

#if WINDOWS_APP
            args.DrawingSession.DrawImage(currentImg.ScaleEffect, new Vector2(), new Rect()
            {
                Height = MetroSlideshow.WindowHeight,
                Width = MetroSlideshow.WindowWidth
            }, currentImg.Opacity);
#endif
            args.DrawingSession.FillRectangle(new Rect(0, 0, MetroSlideshow.WindowWidth, MetroSlideshow.WindowHeight), 
                Color.FromArgb(10, Locator.SettingsVM.AccentColor.R, Locator.SettingsVM.AccentColor.G, Locator.SettingsVM.AccentColor.B));

            threshold++;

            if (threshold == 1)
            {
                threshold = 0;
                if (fastChange)
                {
                    frame += 4;
                }
                else frame++;
            }
            if ((nextIsDefaultPic && frame < EndFrameThresholdDefaultPic) || (!nextIsDefaultPic && frame < EndFrameThreshold))
                return;
            // Resetting variables
            if (ImgIndex < Imgs.Count - 1)
            {
                ImgIndex++;
            }
            else
            {
                ImgIndex = 0;
            }
            fastChange = false;
            UseDefaultPic = nextIsDefaultPic;
            frame = 0;
        }

        public void ChangePicFast(bool nextIsDefault)
        {
            // seek to outro
            fastChange = true;
            frame = OutroFrameThreshold;
            nextIsDefaultPic = nextIsDefault;
        }
    }
}
