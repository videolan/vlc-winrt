using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VLC_WINRT.Utility.Helpers
{
    public static class MathHelper
    {
        public static double Clamp(int m, int M, int x)
        {
            return (x > M) ? M : (x < m) ? m : x;
        }
    }
}
