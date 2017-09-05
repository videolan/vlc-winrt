using System;

using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas.Effects;

namespace Slide2D.Images
{
    public class Img
    {
        public CanvasBitmap Bmp { get; private set; }
        
        public GaussianBlurEffect GaussianBlurCache { get; set; }

        public ScaleEffect ScaleEffect { get; set; }
        public string Src { get; private set; }
        public double Height
        {
            get
            {
                if (Bmp != null) return Bmp.SizeInPixels.Height;
                return -1;
            }
        }

        public double Width
        {
            get
            {
                if (Bmp != null) return Bmp.SizeInPixels.Width;
                return -1;
            }
        }

        public float Scale { get; set; }
        public float Opacity { get; set; }
        public bool Loaded { get; set; }
        public Img(string source)
        {
            Src = source;
            Opacity = 0;
        }

        public async Task Initialize(ICanvasAnimatedControl cac)
        {
            Bmp = await CanvasBitmap.LoadAsync(cac, new Uri(Src));
            Loaded = true;
        }
    }
}