using System;
using System.Collections.Generic;
using VLC_WINRT_APP.Common;

namespace VLC_WINRT_APP.Model.Music
{
    public class ShowItem : BindableBase
    {
        public ShowItem(string title, DateTime date, string city, string country, string lat = null, string _long = null)
        {
            Title = title;
            Date = date;
            City = city;
            Country = country;
            Artists = new List<string>();
            if (lat != null) Latitude = lat;
            if (_long != null) Longitude = _long;
        }

        public string Title { get; set; }
        public DateTime Date { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public List<string> Artists { get; set; }

        public string Latitude { get; set; }
        public string Longitude { get; set; }
    }
}
