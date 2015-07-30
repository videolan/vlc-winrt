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
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Slide2D
{
    public class MetroSlideshow
    {
        public static CanvasAnimatedControl canvas;
        public static double WindowHeight;
        public static double WindowWidth;

        private ImgSlideshow slideshow;
        private List<Img> ImgQueue = new List<Img>();

        public MetroSlideshow()
        {
            SetWindowSize();
        }

        public void Initialize(ref CanvasAnimatedControl cac)
        {
            canvas = cac;
            canvas.CreateResources += Canvas_CreateResources;
            canvas.Draw += Canvas_Draw;
            canvas.TargetElapsedTime = TimeSpan.FromMilliseconds(30);
            Window.Current.SizeChanged += Current_SizeChanged;
            slideshow = new ImgClassicSlideshow();
        }

        private void Current_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            SetWindowSize();
        }

        void SetWindowSize()
        {
            WindowWidth = Window.Current.Bounds.Width;
            WindowHeight = Window.Current.Bounds.Height;
        }

        private void Canvas_Draw(ICanvasAnimatedControl sender, CanvasAnimatedDrawEventArgs args)
        {
            slideshow.Draw(args);
        }

        private async void Canvas_CreateResources(CanvasAnimatedControl sender, Microsoft.Graphics.Canvas.UI.CanvasCreateResourcesEventArgs args)
        {
            AddImg("ms-appx:///Assets/wallpaper.jpg");
        }

        public async void AddImg(string file)
        {
            if (string.IsNullOrEmpty(file)) return;
            var i = new Img(file);
            ImgQueue.Add(i);

            foreach (var img in ImgQueue.ToList())
            {
                await img.Initialize(canvas);
            }
            slideshow.CreateResources(ref canvas, ref ImgQueue);
        }

        public void AddText(List<Txt> texts)
        {
            slideshow.CreateResources(ref canvas, texts);
        }

        public bool IsPaused
        {
            get { return canvas.Paused; }
            set { canvas.Paused = value; }
        }
    }
}
