using System;

namespace VLC_WINRT.Utility.IoC
{
    public class ResolutionException : Exception
    {
        public ResolutionException(string s) : base(s)
        {
        }
    }
}