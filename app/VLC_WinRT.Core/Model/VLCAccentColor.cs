using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI;

namespace VLC.Model
{
    public class VLCAccentColor
    {
        public string Name { get; private set; }
        public Color Color { get; private set; }
        public VLCAccentColor(string name, Color c)
        {
            this.Name = name;
            this.Color = c;
        }
    }
}
