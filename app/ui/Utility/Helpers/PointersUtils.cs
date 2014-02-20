using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.UI.Xaml;

namespace VLC_WINRT.Utility.Helpers
{
    public static class PointerUtils
    {
        public static Point GetPointerPosition()
        {
            Window currentWindow = Window.Current;

            Point point;

            try
            {
                point = currentWindow.CoreWindow.PointerPosition;
            }
            catch (UnauthorizedAccessException)
            {
                return new Point(double.NegativeInfinity, double.NegativeInfinity);
            }

            Rect bounds = currentWindow.Bounds;

            return new Point(DipToPixel(point.X - bounds.X), DipToPixel(point.Y - bounds.Y));
        }

        private static double DipToPixel(double dip)
        {
            return (dip * DisplayProperties.LogicalDpi) / 96.0;
        }
    }
}
