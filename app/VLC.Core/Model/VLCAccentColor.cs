using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI;

namespace VLC.Model
{
    public class VLCAccentColor
    {
        public Color Color { get; private set; }
        public VLCAccentColor(Color c)
        {
            this.Color = c;
        }
    }
}
