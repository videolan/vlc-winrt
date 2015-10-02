using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Slide2D.Images;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VLC_WinRT.Slideshow.Texts;
using VLC_WinRT.ViewModels;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Slide2D
{
    public class MetroSlideshow
    {
        public TaskCompletionSource<bool> IsLoaded = new TaskCompletionSource<bool>();
        public static CanvasAnimatedControl canvas;
        public static double WindowHeight;
        public static double WindowWidth;

        public delegate void WindowSizeChanged();

        public static event WindowSizeChanged WindowSizeUpdated;
        private ImgSlideshow slideshow;
        
        public MetroSlideshow()
        {
            SetWindowSize();
        }

        public void Initialize(ref CanvasAnimatedControl cac)
        {
            canvas = cac;
            canvas.CreateResources += Canvas_CreateResources;
            canvas.Update += Canvas_Update;
            canvas.Draw += Canvas_Draw;
            canvas.ForceSoftwareRenderer = false;
            canvas.Paused = true;

            float dpiLimit = 96.0f;
            if (canvas.Dpi > dpiLimit)
            {
                canvas.DpiScale = dpiLimit / canvas.Dpi;
            }

            Window.Current.SizeChanged += Current_SizeChanged;
            slideshow = new ImgClassicSlideshow();
        }

        private void Current_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            SetWindowSize();
            WindowSizeUpdated?.Invoke();
        }

        private void SetWindowSize()
        {
            WindowWidth = Window.Current.Bounds.Width;
            WindowHeight = Window.Current.Bounds.Height;
        }

        private void Canvas_Update(ICanvasAnimatedControl sender, CanvasAnimatedUpdateEventArgs args)
        {
            slideshow.Update(args);
        }

        private void Canvas_Draw(ICanvasAnimatedControl sender, CanvasAnimatedDrawEventArgs args)
        {
            slideshow.Draw(args);
        }

        private async void Canvas_CreateResources(CanvasAnimatedControl sender,
            Microsoft.Graphics.Canvas.UI.CanvasCreateResourcesEventArgs args)
        {
            IsLoaded.TrySetResult(true);
        }

        public async void AddImg(string file)
        {
            try
            {
                await IsLoaded.Task;
                if (string.IsNullOrEmpty(file)) return;
                Img i = new Img(file);
                try
                {
                    await i.Initialize(canvas);
                }
                catch
                {
                }
                if (i.Loaded)
                    slideshow.Imgs.Add(i);
            }
            catch
            {
            }
        }

        public async void SetDefaultPic(string file)
        {
            await IsLoaded.Task;
            if (string.IsNullOrEmpty(file)) return;
            var i = new Img(file);
            await i.Initialize(canvas);
            slideshow.DefaultImg = i;
        }

        public async void GoDefaultPic()
        {
            await IsLoaded.Task;
            canvas.Paused = false;
            slideshow.ChangePicFast(true);
        }

        public async void RestoreSlideshow()
        {
            await IsLoaded.Task;
            canvas.Paused = false;
            slideshow.ChangePicFast(false);
        }

        public void SetText(List<Txt> texts)
        {
            slideshow.Texts.Clear();
            var id = 0;
            foreach (var txt in texts)
            {
                txt.Id = id;
                id++;
            }
            slideshow.Texts.AddRange(texts);
        }

        public void ClearTextList()
        {
            slideshow.Texts.Clear();
        }

        public bool IsPaused
        {
            get { return canvas.Paused; }
            set { canvas.Paused = value; }
        }

        public bool RichAnimations
        {
            get { return slideshow.RichAnimations; }
            set { slideshow.RichAnimations = value; }
        }

        public bool TextInSlideshowEnabled
        {
            get { return slideshow.TextInSlideshowEnabled; }
            set { slideshow.TextInSlideshowEnabled = value; }
        }
    }
}
