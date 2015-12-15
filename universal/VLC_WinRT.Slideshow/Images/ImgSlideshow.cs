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
using VLC_WinRT.Model;
using VLC_WinRT.ViewModels.MusicVM;
using VLC_WinRT.Helpers;
#if WINDOWS_UWP
#else
// This namespace only works on Windows/Windows Phone 8.1 apps.
using Microsoft.Graphics.Canvas.Numerics;
#endif

namespace Slide2D.Images
{
    public class ImgSlideshow
    {
        private const int IntroFrameThreshold = 200;
        private const int OutroFrameThreshold = 900;
        private const int EndFrameThresholdDefaultPic = 1020;
        private const int EndFrameThreshold = 1100;

        private const float MaximumBlur = 10f;

        private Random random = new Random();
        // Debug only, to slow down frames
        int threshold = 0;

        // ------
        public bool TextInSlideshowEnabled;

        private int frame = 0;
        private bool fastChange;
        private float blurAmount = MaximumBlur;

        private Color backgroundColor;
        private bool newColorIsDark;

        public List<Txt> Texts = new List<Txt>();
        private int ImgIndex = 0;

        private Img currentImg;
        private List<Img> images = new List<Img>();

        private bool _richAnimations;
        private bool changeBackgroundColor;

        public bool RichAnimations
        {
            get { return _richAnimations; }
            set { _richAnimations = value; }
        }
        
        public ImgSlideshow()
        {
            backgroundColor.A = 255;
            Locator.MusicLibraryVM.PropertyChanged += MusicLibraryVM_PropertyChanged;
            Locator.MusicPlayerVM.PropertyChanged += MusicLibraryVM_PropertyChanged;
        }

        private void MusicLibraryVM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MusicLibraryVM.CurrentArtist)
                || e.PropertyName == nameof(MusicLibraryVM.CurrentAlbum))
            {
                Navigated();
            }
        }

        async void Navigated()
        {
            images.Clear();
            bool newPic = false;
            if (Locator.NavigationService.CurrentPage == VLCPage.AlbumPage ||
                Locator.NavigationService.CurrentPage == VLCPage.ArtistPage)
            {
                if (Locator.MusicLibraryVM.CurrentArtist == null) return;
                if (Locator.MusicLibraryVM.CurrentArtist.IsPictureLoaded)
                {
                    images.Add(new Img(Locator.MusicLibraryVM.CurrentArtist.Picture));
                    newPic = true;
                }
                var albums = await Locator.MusicLibraryVM.MusicLibrary.LoadAlbums(Locator.MusicLibraryVM.CurrentArtist.Id);
                if (albums != null)
                {
                    foreach (var albumItem in albums)
                    {
                        if (albumItem.IsPictureLoaded)
                        {
                            images.Add(new Img(albumItem.AlbumCoverFullUri));
                        }
                    }
                }
            }
            else if (Locator.NavigationService.CurrentPage == VLCPage.MusicPlayerPage)
            {
                if (Locator.MusicPlayerVM.CurrentArtist == null) return;
                if (Locator.MusicPlayerVM.CurrentArtist.IsPictureLoaded)
                {
                    images.Add(new Img(Locator.MusicPlayerVM.CurrentArtist.Picture));
                    newPic = true;
                }
                var album = Locator.MusicLibraryVM.MusicLibrary.LoadAlbum(Locator.MusicPlayerVM.CurrentTrack.AlbumId);
                if (album != null)
                {
                    if (album.IsPictureLoaded)
                    {
                        images.Add(new Img(album.AlbumCoverFullUri));
                        newPic = true;
                    }
                }
            }
            else
            {
                var artistList = await Locator.MusicLibraryVM.MusicLibrary.LoadArtists(x => x.IsPictureLoaded);
                if (artistList != null)
                {
                    foreach (var artist in artistList)
                    {
                        images.Add(new Img(artist.Picture));
                        newPic = true;
                    }
                }
            }

            if (newPic)
            {
                frame = OutroFrameThreshold;
                fastChange = true;
            }
            return;
        }

        public async void Update(CanvasAnimatedUpdateEventArgs args)
        {
            if (changeBackgroundColor)
            {
                backgroundColor.A = 255;
                if (newColorIsDark && backgroundColor.R > 0)
                {
                    backgroundColor.R = backgroundColor.G = backgroundColor.B -= 15;
                }
                else if (!newColorIsDark && backgroundColor.R < 255)
                {
                    backgroundColor.R = backgroundColor.G = backgroundColor.B += 15;
                }
                else
                {
                    changeBackgroundColor = false;
                    newColorIsDark = false;
                }
                MetroSlideshow.canvas.ClearColor = backgroundColor;
            }
            if (currentImg == null) return;

            if (!currentImg.Loaded)
            {
                await currentImg.Initialize(MetroSlideshow.canvas);
            }
            
            bool computeBlurPic = true;
            if (frame < IntroFrameThreshold)
            {
                if (frame == 0)
                {
                    // Set Default values
                    currentImg.Opacity = 0;
                    if (_richAnimations)
                    {
                        blurAmount = MaximumBlur;
                    }
                    else
                    {
                        blurAmount = 3f;
                    }
                }

                if (_richAnimations)
                    blurAmount -= 0.025f;
            }
            else if (frame <= OutroFrameThreshold)
            {
                if (currentImg.GaussianBlurCache != null)
                {
                    computeBlurPic = false;
                }
            }
            else if (frame < 1000)
            {
                if (_richAnimations)
                {
                    blurAmount += 0.025f;
                }
            }

            if (computeBlurPic)
            {
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

#if WINDOWS_UWP
#else
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

        public async void Draw(CanvasAnimatedDrawEventArgs args)
        {
            if (currentImg?.ScaleEffect != null)
            {
                if (TextInSlideshowEnabled)
                {
                    var txts = Texts.ToList();
                    foreach (var text in txts)
                    {
                        text.Draw(ref args, ref txts);
                    }
                }

#if WINDOWS_UWP
#else
                args.DrawingSession.DrawImage(currentImg.ScaleEffect, new Vector2(), new Rect()
                {
                    Height = MetroSlideshow.WindowHeight,
                    Width = MetroSlideshow.WindowWidth
                }, currentImg.Opacity);
#endif
                args.DrawingSession.FillRectangle(new Rect(0, 0, MetroSlideshow.WindowWidth, MetroSlideshow.WindowHeight),
                    Color.FromArgb(10, Locator.SettingsVM.AccentColor.R, Locator.SettingsVM.AccentColor.G, Locator.SettingsVM.AccentColor.B));
            }
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
            if (frame < EndFrameThreshold)
                return;
            // Resetting variables
            var artistCount = await Locator.MusicLibraryVM.MusicLibrary.ArtistCount();
            if (ImgIndex < artistCount - 1)
            {
                ImgIndex++;
            }
            else
            {
                ImgIndex = 0;
            }
            if (fastChange)
            {
                fastChange = false;
            }
            // Choosing a new img to display in the next loop
            getNextImg();
            frame = 0;
            blurAmount = MaximumBlur;
        }

        public void SetTheme(bool dark)
        {
            var newColor = (dark) ? Colors.Black : Colors.White;
            if (newColor != backgroundColor)
            {
                changeBackgroundColor = true;
                newColorIsDark = dark;
            }
        }

        void getNextImg()
        {
            Debug.WriteLine($"Choosing a picture out of {images.Count} pictures.");
            int nextImgIndex = 0;
            if (images.Count > 2)
                nextImgIndex = random.Next(0, images.Count - 1);
            else
                nextImgIndex = random.NextDouble() < 0.5 ? 0 : 1;
            currentImg = images[nextImgIndex];
            Debug.WriteLine($"Choose picture uri = {currentImg.Src}");
        }
    }
}
