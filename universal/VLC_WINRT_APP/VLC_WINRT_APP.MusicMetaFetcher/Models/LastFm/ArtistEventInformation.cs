using Newtonsoft.Json;

namespace VLC_WinRT.MusicMetaFetcher.Models.LastFm
{
    public class ArtistEventInformation
    {
        [JsonProperty("events")]
        public Events Events { get; set; }
    }

    public class Events
    {
        [JsonProperty("event")]
        public Event[] Event { get; set; }

        [JsonProperty("@attr")]
        public Attr Attr { get; set; }
    }

    public class Event
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("artists")]
        public SimpleArtists Artists { get; set; }

        [JsonProperty("venue")]
        public Venue Venue { get; set; }

        [JsonProperty("startDate")]
        public string StartDate { get; set; }

        [JsonProperty("endDate")]
        public string EndDate { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("image")]
        public Image2[] Images { get; set; }

        [JsonProperty("attendance")]
        public string Attendance { get; set; }

        [JsonProperty("reviews")]
        public string Reviews { get; set; }

        [JsonProperty("tag")]
        public string Tag { get; set; }

        [JsonProperty("website")]
        public string Website { get; set; }

        [JsonProperty("tickets")]
        public string Tickets { get; set; }

        [JsonProperty("cancelled")]
        public string Cancelled { get; set; }

        [JsonProperty("tags")]
        public SimpleTags Tags { get; set; }
    }

    public class SimpleTags
    {
        [JsonProperty("tag")]
        public object tag { get; set; }
    }

    public class SimpleArtists
    {
        [JsonProperty("artist")]
        public dynamic Artists { get; set; }

        [JsonProperty("headliner")]
        public string Headliner { get; set; }
    }

    public class Venue
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("location")]
        public Location Location { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("website")]
        public string Website { get; set; }

        [JsonProperty("phonenumber")]
        public string PhoneNumber { get; set; }

        [JsonProperty("image")]
        public Image[] Images { get; set; }
    }

    public class Location
    {
        [JsonProperty("geo:point")]
        public GeoPoint GeoPoint { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("street")]
        public string Street { get; set; }

        [JsonProperty("postalcode")]
        public string PostalCode { get; set; }
    }

    public class GeoPoint
    {
        [JsonProperty("geo:lat")]
        public string Latitude { get; set; }

        [JsonProperty("geo:long")]
        public string Longitute { get; set; }
    }
}
