using libVLCX;
using System;
using System.Collections.Generic;
using System.Text;

namespace VLC.Model
{
    public class VLCEqualizer
    {
        public string Name { get; set; }
        public uint Index { get; set; }

        public IList<float> Amps { get; set; }

        public VLCEqualizer(uint index)
        {
            Name = Equalizer.presetName(index);
            Index = index;

            var bandCount = Equalizer.bandCount();
            var eq = new Equalizer(index);
            Amps = new List<float>();
            for (uint i = 0; i < bandCount; i++)
            {
                Amps.Add(eq.amp(i));
            }
        }

        public VLCEqualizer()
        {

        }
    }
}
