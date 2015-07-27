using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Slide2D;
using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI;

namespace VLC_WinRT.Slideshow.Texts
{
    public class Txt
    {
        
        public string Content { get; set; }
        public  CanvasTextFormat Format { get; private set;}
        public Color Color { get; private set; }
        float posX;
        float posY;
        bool initialized = false;

        public Txt(string content, 
            Color col, 
            CanvasTextFormat format)
        {
            Content = content;
            Color = col;
            Format = format;
        }

        public void Draw(ref CanvasAnimatedDrawEventArgs drawArgs)
        {
            if (!initialized)
            {
                if (posX > MetroSlideshow.WindowWidth)
                {
                    posX = 0;
                }
                posY = (float)MetroSlideshow.WindowHeight / 2;
                initialized = true;
            }
            drawArgs.DrawingSession.DrawText(Content, new Microsoft.Graphics.Canvas.Numerics.Vector2()
            {
                X = posX,
                Y = posY
            }, Color, Format);
            if (Format.Direction == CanvasTextDirection.LeftToRightThenTopToBottom)
            {
                posX++;
            }
        }
    }
}
