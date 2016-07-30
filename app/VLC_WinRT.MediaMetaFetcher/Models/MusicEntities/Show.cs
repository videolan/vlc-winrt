using System;
using System.Collections.Generic;

namespace VLC_WinRT.MusicMetaFetcher.Models.MusicEntities
{
    /// <summary>
    /// Entity representing a show
    /// </summary>
    public class Show
    {
        public string Title { get; set; }
        public DateTime Date { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public List<string> Artists { get; set; }

        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public Show(string title, DateTime date, string city, string country, string lat = null, string _long = null)
        {
            Title = title;
            Date = date;
            City = city;
            Country = country;
            Artists = new List<string>();
            if (lat != null) Latitude = lat;
            if (_long != null) Longitude = _long;
        }
    }
}
