using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Slide2D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Windows.UI;
using VLC_WinRT.Helpers;

namespace VLC_WinRT.Slideshow.Texts
{
    public class Txt
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public CanvasTextFormat Format { get; private set; }
        public CanvasTextLayout TextLayout { get; private set; }
        public Color Color { get; private set; }
        public float RowHeight => Format.FontSize;
        float posX;
        float posY;
        bool initialized = false;
        private int frame;
        private bool needToRecomputePosY;

        public Txt(string content, Color col, CanvasTextFormat format)
        {
            Content = content;
            Color = col;
            Format = format;
            MetroSlideshow.WindowSizeUpdated += MetroSlideshow_WindowSizeUpdated;
        }

        private void MetroSlideshow_WindowSizeUpdated()
        {
            needToRecomputePosY = true;
        }

        public void Draw(ref CanvasAnimatedDrawEventArgs drawArgs, ref List<Txt> txts)
        {
            if (!initialized)
            {
                if (TextLayout == null)
                {
                    TextLayout = new CanvasTextLayout(drawArgs.DrawingSession, Content, Format, 0.0f, 0.0f)
                    {
                        WordWrapping = CanvasWordWrapping.NoWrap
                    };
                }

                frame = 0;
                switch (Format.Direction)
                {
                    case CanvasTextDirection.LeftToRightThenTopToBottom:
                        posX = -(float)TextLayout.DrawBounds.Width;
                        break;
                    case CanvasTextDirection.RightToLeftThenTopToBottom:
                        posX = (float)((float)MetroSlideshow.WindowWidth + TextLayout.DrawBounds.Width);
                        break;
                }
                posY = ComputePosY(ref txts);
                initialized = true;
            }

            if (needToRecomputePosY)
            {
                posY = ComputePosY(ref txts);
                needToRecomputePosY = false;
            }

            switch (Format.Direction)
            {
                case CanvasTextDirection.LeftToRightThenTopToBottom:
                    if (posX < (MetroSlideshow.WindowWidth / 4) || posX > (MetroSlideshow.WindowWidth * (float)(3.0 / 4)))
                    {
                        posX += 3;
                    }
                    else
                    {
                        posX++;
                    }

                    if (posX > MetroSlideshow.WindowWidth || posY > MetroSlideshow.WindowHeight)
                    {
                        initialized = false;
                        frame = 0;
                    }
                    break;
                case CanvasTextDirection.RightToLeftThenTopToBottom:
                    if (posX < (MetroSlideshow.WindowWidth / 4) || posX > (MetroSlideshow.WindowWidth * (float)(3.0 / 4)))
                    {
                        posX -= 3;
                    }
                    else
                    {
                        posX--;
                    }

                    if (posX < 0 || posY > MetroSlideshow.WindowHeight)
                    {
                        initialized = false;
                        frame = 0;
                    }
                    break;
            }

            drawArgs.DrawingSession.DrawTextLayout(TextLayout, new Microsoft.Graphics.Canvas.Numerics.Vector2()
            {
                X = posX,
                Y = posY
            }, Color);

            frame++;
        }

        float ComputePosY(ref List<Txt> txts)
        {
            // Considering a vertical margin of 15% of the height
            float Y = 0;
            var heightMargin = (float)MetroSlideshow.WindowHeight * 0.15;
            var stackHeightTextOnTopItThis = txts.TakeWhile(x => x.Id < Id).Sum(x => x.RowHeight);
            Y = (float)heightMargin + stackHeightTextOnTopItThis;
            return Y;
        }
    }
}
