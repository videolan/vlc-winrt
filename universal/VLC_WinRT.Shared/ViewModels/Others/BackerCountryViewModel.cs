using System;
using System.Collections.Generic;
using VLC_WinRT.Common;
using VLC_WinRT.Utils;

namespace VLC_WinRT.ViewModels.RemovableDevicesVM
{
    public class BackerCountryViewModel : BindableBase
    {
        private List<string> _backerNames = new List<string>();
        private Uri _flagSource;
        private string _name;

        public BackerCountryViewModel(string countryName, Uri flagSource)
        {
            Name = countryName;
            FlagSource = flagSource;
        }

        public List<string> BackerNames
        {
            get { return _backerNames; }
            set { SetProperty(ref _backerNames, value); }
        }

        public string Name
        {
            get { return _name; }
            private set { SetProperty(ref _name, value); }
        }

        public Uri FlagSource
        {
            get { return _flagSource; }
            private set { SetProperty(ref _flagSource, value); }
        }
    }
}