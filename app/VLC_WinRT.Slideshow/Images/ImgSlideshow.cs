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
        private bool clearSlideshow;
        private float blurAmount = MaximumBlur;
        
        public List<Txt> Texts = new List<Txt>();
        private int ImgIndex = 0;

        private Img currentImg;
        private List<Img> images = new List<Img>();

        private bool _richAnimations;

        public bool RichAnimations
        {
            get { return _richAnimations; }
            set { _richAnimations = value; }
        }
        
        public ImgSlideshow()
        {
            Locator.MusicLibraryVM.PropertyChanged += MusicLibraryVM_PropertyChanged;
            Locator.MusicPlayerVM.PropertyChanged += MusicLibraryVM_PropertyChanged;
            Locator.NavigationService.ViewNavigated += ViewNavigated;
        }

        private void ViewNavigated(object sender, VLCPage page)
        {
            Navigated(true);
        }

        private void MusicLibraryVM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MusicLibraryVM.CurrentArtist)
                || e.PropertyName == nameof(MusicLibraryVM.CurrentAlbum))
            {
                Navigated(false);
            }
        }

        async void Navigated(bool newPage)
        {
            bool newPic = false;
            if (newPage)
            {
                if (Locator.NavigationService.CurrentPage != VLCPage.AlbumPage
                    && Locator.NavigationService.CurrentPage != VLCPage.ArtistPage
                    && Locator.NavigationService.CurrentPage != VLCPage.MusicPlayerPage
                    && Locator.NavigationService.CurrentPage != VLCPage.MainPageMusic)
                {
                    newPic = true;
                    clearSlideshow = true;
                }
            }
            else
            {
                images.Clear();
                clearSlideshow = false;
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
                    var album = await Locator.MusicLibraryVM.MusicLibrary.LoadAlbum(Locator.MusicPlayerVM.CurrentTrack.AlbumId);
                    if (album != null)
                    {
                        if (album.IsPictureLoaded)
                        {
                            images.Add(new Img(album.AlbumCoverFullUri));
                            newPic = true;
                        }
                    }
                }
                else if (Locator.NavigationService.CurrentPage == VLCPage.MainPageMusic)
                {
                    if (Locator.MusicLibraryVM.MusicView == VLC_WinRT.Model.Music.MusicView.Artists)
                    {
                        if (Locator.MusicLibraryVM.CurrentArtist == null) return;
                        if (Locator.MusicLibraryVM.CurrentArtist.IsPictureLoaded)
                        {
                            images.Add(new Img(Locator.MusicLibraryVM.CurrentArtist.Picture));
                            newPic = true;
                        }

                        var albums = await Locator.MusicLibraryVM.MusicLibrary.LoadAlbums(Locator.MusicLibraryVM.CurrentArtist.Id);
                        foreach (var album in albums)
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
                        newPic = true;
                        clearSlideshow = true;
                    }
                }
            }

            if (newPic)
            {
                MetroSlideshow.canvas.Paused = false;
                frame = OutroFrameThreshold;
                fastChange = true;
            }
            return;
        }

        public async void Update(ICanvasAnimatedControl sender, CanvasAnimatedUpdateEventArgs args)
        {
            try
            {
                if (currentImg == null) return;

                if (!currentImg.Loaded)
                {
                    await currentImg.Initialize(sender);
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
                            blurAmount = 4f;
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
                    if (currentImg.Bmp == null) return;
                    currentImg.GaussianBlurCache = new GaussianBlurEffect()
                    {
                        Source = currentImg.Bmp,
                        BlurAmount = blurAmount,
                        Optimization = EffectOptimization.Speed
                    };
                }

                float screenRatio = (float)MetroSlideshow.WindowWidth / (float)MetroSlideshow.WindowHeight;
                float imgRatio = (float)currentImg.Width / (float)currentImg.Height;
                if (currentImg.Width == -1 || currentImg.Height == -1) return;
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
                if (currentImg.GaussianBlurCache == null) return;

                var scaleEffect = new ScaleEffect()
                {
                    Source = currentImg.GaussianBlurCache,
                    Scale = new System.Numerics.Vector2()
                    {
                        X = currentImg.Scale,
                        Y = currentImg.Scale
                    },
                };

                scaleEffect.CenterPoint = new System.Numerics.Vector2()
                {
                    X = 0,
                    Y = 0
                };

                currentImg.ScaleEffect = scaleEffect;

                if (frame < IntroFrameThreshold)
                {
                    currentImg.Opacity += 0.0020f;
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
            catch { }
        }

        public async void Draw(ICanvasAnimatedControl sender, CanvasAnimatedDrawEventArgs args)
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
                args.DrawingSession.DrawImage(currentImg.ScaleEffect, new System.Numerics.Vector2(), new Rect()
                {
                    Height = MetroSlideshow.WindowHeight,
                    Width = MetroSlideshow.WindowWidth
                }, currentImg.Opacity);
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

            if (clearSlideshow)
            {
                sender.Paused = true;
            }

            // Choosing a new img to display in the next loop
            getNextImg();
            frame = 0;
            blurAmount = MaximumBlur;
        }
        
        void getNextImg()
        {
            Debug.WriteLine($"Choosing a picture out of {images.Count} pictures.");
            if (images.Count == 0) return;
            int nextImgIndex = 0;
            if (images.Count > 2)
                nextImgIndex = random.Next(0, images.Count - 1);
            else if (images.Count > 1)
                nextImgIndex = random.NextDouble() < 0.5 ? 0 : 1;
            currentImg = images[nextImgIndex];
            Debug.WriteLine($"Choose picture uri = {currentImg.Src}");
        }
    }
}
