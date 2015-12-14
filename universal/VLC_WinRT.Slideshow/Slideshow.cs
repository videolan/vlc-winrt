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

        private bool wasPausedBeforeCoreWindowDeactivation = false;
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
            slideshow = new ImgSlideshow();

            CoreWindow.GetForCurrentThread().Activated += ApplicationState_Activated;
        }

        private void ApplicationState_Activated(CoreWindow sender, WindowActivatedEventArgs args)
        {
            if (args.WindowActivationState == CoreWindowActivationState.Deactivated)
            {
                if (IsPaused)
                {
                    wasPausedBeforeCoreWindowDeactivation = true;
                }
                IsPaused = true;
            }
            else
            {
                if (!wasPausedBeforeCoreWindowDeactivation)
                {
                    IsPaused = false;
                }
            }
        }

        private void Current_SizeChanged(object sender, WindowSizeChangedEventArgs e)
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
            get
            {
                return canvas != null && canvas.Paused;
            }
            set { if (canvas != null) canvas.Paused = value; }
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

        public async void SetTheme(bool dark)
        {
            await IsLoaded.Task;
            if (dark)
            {
                canvas.ClearColor = Colors.Black;
            }
            else
            {
                canvas.ClearColor = Colors.White;
            }
        }
    }
}
