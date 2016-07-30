using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using VLC.MediaMetaFetcher.Fetchers;
using System.Diagnostics;
using VLC.Model.Video;

namespace VLC.MediaMetaFetcher
{
    public class VideoMDFetcher
    {
        public static string TheMovieDbApiKey;
        MovieDbClient movieDbClient = new MovieDbClient();
        OpenSubtitleClient openSubClient = new OpenSubtitleClient();

        public VideoMDFetcher(string movieDbKey)
        {
            TheMovieDbApiKey = movieDbKey;
        }

        private async Task<byte[]> DownloadMovieCoverFromMovieDB(string movieName)
        {
            try
            {
                var movie = await movieDbClient.GetMovieInfo(movieName);
                if (movie == null) return null;
                var result = await movieDbClient.GetMovieImages(movie);
                if (result && movie.Images != null && movie.Images.Any())
                {
                    return await movieDbClient.GetMovieImage(movie);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error getting or saving movie: {movieName} picture from MovieDB");
            }
            return null;
        }

        public async Task<byte[]> GetMovieCover(string movieName)
        {
            return await DownloadMovieCoverFromMovieDB(movieName);
        }

        public async Task<byte[]> GetMovieSubtitle(VideoItem video)
        {
            try
            {
                var movieSub = await openSubClient.GetSubtitleUrl(video);
                if (!string.IsNullOrEmpty(movieSub))
                {
                    return await openSubClient.DownloadSubtitle(video, movieSub);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error getting or saving movie: {video.Name} subtitle from opensubtitle");
            }
            return null;
        }
    }
}
