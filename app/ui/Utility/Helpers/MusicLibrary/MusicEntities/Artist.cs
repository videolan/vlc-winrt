/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Windows.Storage.FileProperties;
using VLC_WINRT.Model;
using Windows.ApplicationModel.Resources;
using VLC_WINRT.Utility.Helpers.MusicLibrary.LastFm;

namespace VLC_WINRT.Utility.Helpers.MusicLibrary.MusicEntities
{
    public class Artist
    {
        public string Name { get; set; }

        public string Url { get; set; }

        public List<Image> Images { get; set; }

        public long Listeners { get; set; }

        public long Playcount { get; set; }

        public string Biograpgy { get; set; }

        public bool OnTour { get; set; }

        /// <summary>
        /// Map from LastFmClient TopArtist LastFmClient Entity.
        /// </summary>
        /// <param name="artist">TopArist Entity.</param>
        public void MapFrom(TopArtist artist)
        {
            this.Name = artist.Name;
            this.Url = artist.Url;
        }

        public void MapFrom(SimilarArtist artist)
        {
            this.Name = artist.Name;
            this.Url = artist.Url;
            this.Images = new List<Image>();
            foreach (var image in artist.Image)
            {
                var artistImage = new Image();
                artistImage.MapFrom(image);
                this.Images.Add(artistImage);
            }
        }

        public void MapFrom(ArtistInformation artistInformation)
        {
            var artist = artistInformation.Artist;
            this.Name = artist.Name;
            this.Url = Url;

            // I hope this is not stupid code. It broke on Boolean.Parse :(.
            switch (artist.Ontour)
            {
                case "0":
                    this.OnTour = false;
                    break;
                case "1":
                    this.OnTour = true;
                    break;
                default:
                    this.OnTour = false;
                    break;
            }

            this.Images = new List<Image>();
            foreach (var image in artist.Image)
            {
                var artistImage = new Image();
                artistImage.MapFrom(image);
                this.Images.Add(artistImage);
            }

            this.Playcount = Convert.ToInt64(this.Playcount);
            this.Listeners = Convert.ToInt64(this.Listeners);
            string biography;

            var resourceLoader = new ResourceLoader();
            var bioSummary = artist.Bio.Summary;
            if (bioSummary != null)
            {
                // Deleting the html tags
                biography = Regex.Replace(bioSummary, "<.*?>", string.Empty);
                // Remove leading new lines.
                biography = biography.TrimStart('\r', '\n');
                // Remove leading and ending white spaces.
                biography = biography.Trim();
                // TODO: Replace string "remove" with something better. It may not work on all artists and in all languages.
                biography = !string.IsNullOrEmpty(biography) ? biography.Remove(biography.Length - "Read more about  on Last.fm".Length - artist.Name.Length - 6)
                    : resourceLoader.GetString("NoBiographyFound");
            }
            else
            {
                biography = resourceLoader.GetString("NoBiographyFound");
            }
            this.Biograpgy = biography;
        }
    }
}
