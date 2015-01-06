using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Autofac;
using Autofac.Core;
using VLC_WINRT.Common;
using VLC_WINRT_APP.Commands.Music;
using VLC_WINRT_APP.Helpers;
using VLC_WINRT_APP.Helpers.MusicLibrary;
using VLC_WINRT_APP.Model;
using VLC_WINRT_APP.Services.Interface;
using VLC_WINRT_APP.Services.RunTime;
using VLC_WINRT_APP.ViewModels;
using VLC_WINRT_APP.Views;
using VLC_WINRT_APP.Views.MainPages;
using VLC_WINRT_APP.Common;
using VLC_WINRT_APP.ViewModels.MusicVM;
using VLC_WINRT_APP.Views.VideoPages;
using WinRTXamlToolkit.Controls.Extensions;
#if WINDOWS_PHONE_APP
using Windows.Phone.Management.Deployment;
using Windows.Media.Playback;
using VLC_WINRT_APP.BackgroundHelpers;
#endif

namespace VLC_WINRT_APP
{
    /// <summary>
    ///     Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public sealed partial class App : Application
    {
        public static CoreDispatcher Dispatcher;
        public static IPropertySet LocalSettings = ApplicationData.Current.LocalSettings.Values;
        public static string ApiKeyLastFm = "a8eba7d40559e6f3d15e7cca1bfeaa1c";
        public static string DeezerAppID = "135671";
        public static OpenFilePickerReason OpenFilePickerReason = OpenFilePickerReason.Null;

        public static IContainer Container;
#if WINDOWS_PHONE_APP
        public static BackgroundAudioHelper BackgroundAudioHelper = new BackgroundAudioHelper();
#endif
        /// <summary>
        ///     Initializes the singleton application object.  This is the first line of authored code
        ///     executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
            Suspending += OnSuspending;
            UnhandledException += ExceptionHelper.ExceptionStringBuilder;
            Container = AutoFacConfiguration.Configure();
        }

        public static Frame ApplicationFrame
        {
            get
            {
                return RootPage != null ? RootPage.MainFrame : null;
            }
        }

        public static MainPage RootPage
        {
            get { return Window.Current.Content as MainPage; }
        }

        /// <summary>
        ///     Invoked when the application is launched normally by the end user.  Other entry points
        ///     will be used when the application is launched to open a specific file, to display
        ///     search results, and so forth.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
#if DEBUG
            if (Debugger.IsAttached)
            {
                DebugSettings.EnableFrameRateCounter = true;
            }
#endif
            if (Window.Current.Content == null)
            {
                LaunchTheApp();
#if WINDOWS_PHONE_APP
                ApplicationFrame.Navigated += this.RootFrame_FirstNavigated;
#endif
                ApplicationFrame.Navigate(typeof(MainPageHome));

                try
                {
                    await ExceptionHelper.ExceptionLogCheckup();
                }
                catch
                {
                }
            }
            if (args.Arguments.Contains("SecondaryTile"))
            {
                await RedirectFromSecondaryTile(args.Arguments);
            }
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                bool isinternet = System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable();
                Locator.MainVM.IsInternet = isinternet;
            });
#if WINDOWS_PHONE_APP
            BackgroundAudioHelper.InitBackgroundAudio();
#endif
        }

        private async Task RedirectFromSecondaryTile(string args)
        {
            try
            {
                var query = "";
                int id;
                if (args.Contains("Album"))
                {
                    query = args.Replace("SecondaryTile-Album-", "");
                    id = int.Parse(query);
                    if (Locator.MusicLibraryVM.LoadingState == LoadingState.Loaded)
                    {
                        await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    Locator.MusicLibraryVM.AlbumClickedCommand.Execute(id));
                    }
                    else
                    {
                        MusicLibraryVM.MusicCollectionLoaded += async (sender, eventArgs) =>
                        {
                            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                            Locator.MusicLibraryVM.AlbumClickedCommand.Execute(id));
                        };
                    }
                }
                else if (args.Contains("Artist"))
                {
                    query = args.Replace("SecondaryTile-Artist-", "");
                    id = int.Parse(query);
                    if (Locator.MusicLibraryVM.LoadingState == LoadingState.Loaded)
                    {
                        await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                            () => Locator.MusicLibraryVM.ArtistClickedCommand.Execute(id));
                    }
                    else
                    {
                        MusicLibraryVM.MusicCollectionLoaded += async (sender, value) =>
                        {
                            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                                Locator.MusicLibraryVM.ArtistClickedCommand.Execute(id));
                        };
                    }
                }
            }
            catch (Exception e)
            {
                LogHelper.Log("Failed to open from secondary tile : " + e.ToString());
            }
        }

#if WINDOWS_PHONE_APP
        /// <summary>
        /// Restores the content transitions after the app has launched.
        /// </summary>
        /// <param name="sender">The object where the handler is attached.</param>
        /// <param name="args">Details about the navigation event.</param>
        private void RootFrame_FirstNavigated(object sender, NavigationEventArgs args)
        {
            StatusBarHelper.SetDefaultForPage(args.SourcePageType);
            AppBarHelper.SetHomeButtonVisibleOrNot();
            AppBarHelper.UpdateAppBar(args.SourcePageType);
            AppBarHelper.UpdateSecondaryAppBarButtons();
            Locator.MainVM.CurrentPage = args.SourcePageType;
        }

        protected async override void OnActivated(IActivatedEventArgs args)
        {
            base.OnActivated(args);
            try
            {
                var continueArgs =
                    args as FileOpenPickerContinuationEventArgs;
                if (continueArgs != null && continueArgs.Files.Any())
                {
                    switch (OpenFilePickerReason)
                    {
                        case OpenFilePickerReason.OnOpeningVideo:
                            await OpenFile(continueArgs.Files[0]);
                            break;
                        case OpenFilePickerReason.OnOpeningSubtitle:
                            {
                                string mru = StorageApplicationPermissions.FutureAccessList.Add(continueArgs.Files[0]);
                                string mrl = "file://" + mru;
                                Locator.VideoVm.OpenSubtitle(mrl);
                            }
                            break;
                    }
                }
                OpenFilePickerReason = OpenFilePickerReason.Null;
            }
            catch (Exception e)
            {
                ExceptionHelper.CreateMemorizedException("App.cs.OnActivated", e);
            }
        }

#endif

        /// <summary>
        ///     Invoked when application execution is being suspended.  Application state is saved
        ///     without knowing whether the application will be terminated or resumed with the contents
        ///     of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            SuspendingDeferral deferral = e.SuspendingOperation.GetDeferral();
            Container.Resolve<IMediaService>().Trim();
            deferral.Complete();
        }

        protected override async void OnFileActivated(FileActivatedEventArgs args)
        {
            base.OnFileActivated(args);
            await ManageOpeningFiles(args);
        }

        private Task ManageOpeningFiles(FileActivatedEventArgs args)
        {
            return OpenFile(args.Files[0] as StorageFile);
        }

        private async Task OpenFile(StorageFile file)
        {
            if (file == null) return;
            if (Window.Current.Content == null)
            {
                LaunchTheApp();
            }
            if (VLCFileExtensions.FileTypeHelper(file.FileType) ==
                VLCFileExtensions.VLCFileType.Video)
            {
                await MediaService.PlayVideoFile(file as StorageFile);
            }
            else
            {
                await MediaService.PlayAudioFile(file as StorageFile);
            }
        }

        private void LaunchTheApp()
        {
            Window.Current.Content = Container.Resolve<MainPage>();
            Dispatcher = Window.Current.Content.Dispatcher;
            Window.Current.Activate();
        }
    }
}