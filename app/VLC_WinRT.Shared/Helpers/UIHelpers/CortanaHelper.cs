using System;
using System.Collections.Generic;
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
                var vcdStorageFile = await Package.Current.InstalledLocation.GetFileAsync(@"VLCCommands.xml");
                await VoiceCommandDefinitionManager.InstallCommandDefinitionsFromStorageFileAsync(vcdStorageFile);
            }
            catch { }
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
                    case "playSongByArtist":
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
                            case "playSongByArtist":
                                var songName = voiceArgs.Result.SemanticInterpretation.Properties["songName"].FirstOrDefault();
                                var tracks = await Locator.MediaLibrary.LoadTracksByArtistId(artistItem.Id);
                                var trackItem = tracks.FirstOrDefault(x => x.Name == songName);
                                Locator.MusicLibraryVM.TrackClickedCommand.Execute(trackItem);
                                break;
                            default:
                                break;
                        }
                        break;
                    case "shuffleLibrary":
                        var success = Locator.MusicLibraryVM.PlayAllRandomCommand.Execute(null);
                        break;
                    default:
                        break;
                }
            }
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
            VoiceCommandDefinition commandSetEnUs;
            if (VoiceCommandDefinitionManager.InstalledCommandDefinitions.TryGetValue("VlcCommandSet_en-us", out commandSetEnUs))
            {
                await commandSetEnUs.SetPhraseListAsync(phraseListName, names);
            }
#endif
        }
    }
}
