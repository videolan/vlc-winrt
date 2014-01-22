using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using VLC_WINRT.Common;

namespace VLC_WINRT.Model
{
    public class Panel : BindableBase
    {
        private string _title;
        private int _index;
        private double _opacity;

        public Panel(string t, int i, double o)
        {
            _title = t;
            _index = i;
            _opacity = o;
        }
        public string Title { get { return _title; } set { SetProperty(ref _title, value); } }
        public int Index { get { return _index; } set { SetProperty(ref _index, value); } }
        public double Opacity { get { return _opacity; } set { SetProperty(ref _opacity, value); } }
    }
}
