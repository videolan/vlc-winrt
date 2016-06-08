using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VLC_WinRT.ViewModels;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.VoiceCommands;

namespace VLC_WinRT.Helpers
{
    public static class CortanaHelper
    {
        public static async Task Initialize()
        {
#if WINDOWS_UWP
            try
            {
#if STARTS
                var vcdStorageFile = await Package.Current.InstalledLocation.GetFileAsync(@"VLCCommandsFRonly.xml");
#else
                var vcdStorageFile = await Package.Current.InstalledLocation.GetFileAsync(@"VLCCommands.xml");
#endif
                await VoiceCommandDefinitionManager.InstallCommandDefinitionsFromStorageFileAsync(vcdStorageFile);
            }
            catch { }
#endif
            }

        /// <summary>
        /// It takes a VERY LONG TIME to set the phrase list, something like 10 seconds. This is too much
        /// </summary>
        /// <param name="phraseListName"></param>
        /// <param name="names"></param>
        /// <returns></returns>
        public static async Task SetPhraseList(string phraseListName, IEnumerable<string> names)
        {
#if WINDOWS_UWP
            try
            {
                VoiceCommandDefinition commandSetEnUs;
                var cortanaLanguageSet = "VlcCommandSet_en-us";
                if (!VoiceCommandDefinitionManager.InstalledCommandDefinitions.TryGetValue(cortanaLanguageSet, out commandSetEnUs))
                {
                    await Initialize();
                }
                else if (commandSetEnUs != null || VoiceCommandDefinitionManager.InstalledCommandDefinitions.TryGetValue(cortanaLanguageSet, out commandSetEnUs))
                {
                    await commandSetEnUs.SetPhraseListAsync(phraseListName, names);
                }
            }
            catch(Exception e)
            {
                Debug.WriteLine($"CortanaHelper: {e.ToString()}");
            }
#endif
        }
        

        public static async Task HandleProtocolActivation(IActivatedEventArgs args)
        {
            var voiceArgs = (VoiceCommandActivatedEventArgs)args;
            if (voiceArgs.Result.Status == Windows.Media.SpeechRecognition.SpeechRecognitionResultStatus.Success)
            {
                var commandName = voiceArgs.Result.RulePath[0];
                var commandText = voiceArgs.Result.Text;

                switch (commandName)
                {
                    case "playArtist":
                    case "showArtist":
                    case "playAlbumByArtist":
                    case "createArtistPlaylist":
                        var artistName = voiceArgs.Result.SemanticInterpretation.Properties["artistName"].FirstOrDefault();
                        var artistItem = await Locator.MediaLibrary.LoadViaArtistName(artistName);
                        switch (commandName)
                        {
                            case "playArtist":
                                Locator.MusicLibraryVM.PlayArtistAlbumsCommand.Execute(artistItem);
                                break;
                            case "showArtist":
                                Locator.MusicLibraryVM.ArtistClickedCommand.Execute(artistItem);
                                break;
                            case "playAlbumByArtist":
                                var albumName = voiceArgs.Result.SemanticInterpretation.Properties["albumName"].FirstOrDefault();
                                var albumItem = await Locator.MediaLibrary.LoadAlbums(x=>x.Artist == artistItem.Name && x.Name == albumName);
                                if (albumItem?.FirstOrDefault() != null)
                                {
                                    Locator.MusicLibraryVM.AlbumClickedCommand.Execute(albumItem);
                                }
                                break;
                            case "createArtistPlaylist":
                                Locator.NavigationService.Go(Model.VLCPage.MainPageMusic);
                                Locator.MusicLibraryVM.MusicView = Model.Music.MusicView.Playlists;
                                var playlist = await Locator.MediaLibrary.AddNewPlaylist(artistItem.Name);
                                if (playlist == null)
                                    return;
                                Locator.MusicLibraryVM.CurrentTrackCollection = playlist;
                                await Locator.MediaLibrary.AddToPlaylist(artistItem);
                                Locator.NavigationService.Go(Model.VLCPage.PlaylistPage);
                                break;
                            default:
                                break;
                        }
                        break;
                    case "shuffleLibrary":
                        Locator.MusicLibraryVM.PlayAllRandomCommand.Execute(null);
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
