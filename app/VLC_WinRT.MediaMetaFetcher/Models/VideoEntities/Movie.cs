using System;
using System.Collections.Generic;
using System.Text;
using VLC.MediaMetaFetcher.Models.MovieDB;
using VLC.MediaMetaFetcher.Models.SharedEntities;
using System.Linq;
namespace VLC.MediaMetaFetcher.Models.VideoEntities
{
    public class Movie
    {
        public string Name { get; private set; }
        public int MovieDbId { get; private set; }
        public List<Image> Images { get; private set; } = new List<Image>();
        public void MapFrom(MovieInformation movie)
        {
            this.Name = movie.title;
            this.MovieDbId = movie.id;
        }

        public void MapFrom(MovieImagesInformation imgs)
        {
            this.Images = imgs.backdrops.Select(x => Image.MapFrom(x)).ToList();
        }
    }
}
