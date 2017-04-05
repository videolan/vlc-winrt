using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;

using Newtonsoft.Json;
using VLC.MediaMetaFetcher.Models.MovieDB;
using VLC.MediaMetaFetcher.Models.VideoEntities;
using System.Diagnostics;

namespace VLC.MediaMetaFetcher.Fetchers
{
    public class MovieDbClient
    {
        private bool configured = false;
        private ConfigurationInformation configuration;

        private async Task<bool> Initialize()
        {
            try
            {
                if (string.IsNullOrEmpty(VideoMDFetcher.TheMovieDbApiKey))
                    return configured;

                var client = new HttpClient();

                var url = $"http://api.themoviedb.org/3/configuration?api_key={VideoMDFetcher.TheMovieDbApiKey}";
                var response = await client.GetStringAsync(new Uri(url));
                var config = JsonConvert.DeserializeObject<ConfigurationInformation>(response);

                if (config != null)
                {
                    this.configuration = config;
                    this.configured = true;
                }
            }
            catch (Exception)
            {
                Debug.WriteLine("Failed to get configuration information from MovieDB Client");
            }
            return configured;
        }

        public async Task<Movie> GetMovieInfo(string movieName)
        {
            if (!configured)
            {
                if (!await Initialize())
                {
                    return null;
                }
            }

            try
            {
                var client = new HttpClient();

                var url = $"http://api.themoviedb.org/3/search/movie?api_key={VideoMDFetcher.TheMovieDbApiKey}&query={movieName}";

                var response = await client.GetStringAsync(new Uri(url));

                var moviesResult = JsonConvert.DeserializeObject<MovieSearchInformation>(response);

                if (moviesResult == null) return null;
                if (moviesResult.results == null) return null;
                if (moviesResult.results.Any())
                {
                    var movie = new Movie();
                    movie.MapFrom(moviesResult.results[0]);
                    return movie;
                }

            }
            catch (Exception)
            {
                Debug.WriteLine($"Failed to get movie {movieName} info from MovieDB. Returning nothing.");
            }

            return null;
        }

        public async Task<bool> GetMovieImages(Movie movie)
        {
            try
            {
                var client = new HttpClient();

                var url = $"http://api.themoviedb.org/3/movie/{movie.MovieDbId}/images?api_key={VideoMDFetcher.TheMovieDbApiKey}";

                var response = await client.GetStringAsync(new Uri(url));

                var imgsResult = JsonConvert.DeserializeObject<MovieImagesInformation>(response);

                if (imgsResult == null) return false;
                if (imgsResult.backdrops.Any())
                {
                    movie.MapFrom(imgsResult);
                    return true;
                }
            }
            catch
            {
                Debug.WriteLine($"Failed to get movie {movie.Name} pictures from MovieDB");
            }
            return false;
        }

        public async Task<byte[]> GetMovieImage(Movie movie)
        {
            try
            {
                var clientPic = new HttpClient();
                var responsePic = await clientPic.GetAsync(new Uri(configuration.images.base_url + "w300" + movie.Images.First().Url));

                var imgBytes = await responsePic.Content.ReadAsByteArrayAsync();
                return imgBytes;
            }
            catch
            {
                Debug.WriteLine($"Failed to download movie {movie.Name} picture from MovieDB");
            }
            return null;
        }
    }
}
