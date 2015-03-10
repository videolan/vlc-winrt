using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using VLC_WINRT_APP.Common;
using VLC_WINRT_APP.Model.Music;
using VLC_WINRT_APP.MusicMetaFetcher;
using VLC_WINRT_APP.MusicMetaFetcher.Models.MusicEntities;
using VLC_WINRT_APP.ViewModels;
using VLC_WINRT_APP.ViewModels.MusicVM;

namespace VLC_WINRT_APP.Services.RunTime
{
    public sealed class MusicMetaService
    {
        readonly MusicMDFetcher musicMdFetcher = new MusicMDFetcher(App.DeezerAppID, App.ApiKeyLastFm);

        public async Task GetSimilarArtists(ArtistItem artist)
        {
            var artists = await musicMdFetcher.GetArtistSimilarsArtist(artist.Name);
            if (artists == null) return;
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                artist.IsOnlineRelatedArtistsLoaded = true;
                artist.OnlineRelatedArtists = artists;
            });
        }

        public async Task GetPopularAlbums(ArtistItem artist)
        {
            var albums = await musicMdFetcher.GetArtistTopAlbums(artist.Name);
            if (albums == null) return;
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                artist.IsOnlinePopularAlbumItemsLoaded = true;
                artist.OnlinePopularAlbumItems = albums;
            });
        }

        public async Task GetArtistBiography(ArtistItem artist)
        {
            var bio = await musicMdFetcher.GetArtistBiography(artist.Name);
            if (string.IsNullOrEmpty(bio)) return;
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                artist.Biography = bio;
            });
        }

        public async Task<bool> GetAlbumCover(AlbumItem album)
        {
            if (Locator.MainVM.IsInternet)
            {
                var bytes = await musicMdFetcher.GetAlbumPictureFromInternet(album.Name, album.Artist);
                if (bytes == null)
                {
                    // If we still could not find album art, flag the picture as loaded.
                    // This way, we won't keep pinging the APIs for album art that may not exist.
                    var installFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
                    var file = await installFolder.GetFileAsync("Assets\\NoCover.jpg");
                    bytes = await ConvertImage.ConvertImagetoByte(file);
                }
                var success = bytes != null && await SaveAlbumImageAsync(album, bytes);
                if (success)
                {
                    await MusicLibraryVM._albumDataRepository.Update(album);
                    return true;
                }
            }
            return false;
        }

        public async Task<bool> GetArtistPicture(ArtistItem artist)
        {
            if (Locator.MainVM.IsInternet)
            {
                var bytes = await musicMdFetcher.GetArtistPicture(artist.Name);
                if (bytes == null)
                {
                    // If we still could not find album art, flag the picture as loaded.
                    // This way, we won't keep pinging the APIs for album art that may not exist.
                    var installFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
                    var file = await installFolder.GetFileAsync("Assets\\NoCover.jpg");
                    bytes = await ConvertImage.ConvertImagetoByte(file);
                }
                var success = bytes != null && await SaveArtistImageAsync(artist, bytes);
                if (success)
                {
                    await MusicLibraryVM._artistDataRepository.Update(artist);
                    return true;
                }
            }
            return false;
        }

        public async Task<bool> SaveAlbumImageAsync(AlbumItem album, byte[] img)
        {
            if (await SaveImage(album.Id, "albumPic", img))
            {
                await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    album.IsPictureLoaded = true;
                    album.IsCoverInLocalFolder = true;
                });
                await album.ResetAlbumArt();
                return true;
            }
            return false;
        }


        public async Task<bool> SaveArtistImageAsync(ArtistItem artist, byte[] img)
        {
            if (await SaveImage(artist.Id, "artistPic", img))
            {
                await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => artist.IsPictureLoaded = true);
                await artist.ResetArtistHeader();
                return true;
            }
            return false;
        }

        public async Task<List<Artist>> GetTopArtistGenre(string genre)
        {
            throw new NotImplementedException();
        }

        public async Task GetArtistEvents(ArtistItem artist)
        {
            var shows = await musicMdFetcher.GetArtistEvents(artist.Name);
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                artist.IsUpcomingShowsLoading = false;
                if (shows == null) return; 
                artist.UpcomingShows = shows;
            });
        }

        private async Task<bool> SaveImage(int id, String folderName, byte[] img)
        {
            String fileName = String.Format("{0}.jpg", id);
            try
            {
                using (var streamWeb = new InMemoryRandomAccessStream())
                {
                    using (var writer = new DataWriter(streamWeb.GetOutputStreamAt(0)))
                    {
                        writer.WriteBytes(img);
                        await writer.StoreAsync();
                        var albumPic = await ApplicationData.Current.LocalFolder.CreateFolderAsync(folderName, CreationCollisionOption.OpenIfExists);

                        var file = await albumPic.CreateFileAsync(fileName, CreationCollisionOption.OpenIfExists);
                        Debug.WriteLine("Writing file " + folderName + " " + id);
                        using (var raStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                        {
                            using (var thumbnailStream = streamWeb.GetInputStreamAt(0))
                            {
                                using (var stream = raStream.GetOutputStreamAt(0))
                                {
                                    await RandomAccessStream.CopyAsync(thumbnailStream, stream);
                                    await stream.FlushAsync();
                                }
                            }
                            await raStream.FlushAsync();
                        }
                        await writer.FlushAsync();
                    }
                    await streamWeb.FlushAsync();
                }
                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error saving album art: " + e);
                return false;
            }
        }
    }
}
