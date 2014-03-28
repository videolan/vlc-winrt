/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VLC_WINRT.Common;
using VLC_WINRT.Utility.Helpers.MusicLibrary;
using VLC_WINRT.Utility.Services.RunTime;
using VLC_WINRT.ViewModels;
using VLC_WINRT.ViewModels.MainPage;
using VLC_WINRT.Views;
using Autofac;

// The Split App template is documented at http://go.microsoft.com/fwlink/?LinkId=234228

namespace VLC_WINRT
{
    /// <summary>
    ///     Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        public static CoreDispatcher Dispatcher;
        public static IPropertySet LocalSettings = ApplicationData.Current.LocalSettings.Values;
        public static string ApiKeyLastFm = "a8eba7d40559e6f3d15e7cca1bfeaa1c";

        // If != null; open the corresponding file
        public static string TemporaryMRL;
        public static string TemporaryFileName;

        public static IContainer Container;

        /// <summary>
        ///     Initializes the singleton Application object.  This is the first line of authored code
        ///     executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
            Suspending += OnSuspending;

            Container = AutoFacConfiguration.Configure();
        }
        public static Frame ApplicationFrame
        {
            get
            {
                var root = Window.Current.Content as RootPage;
                return root != null ? root.MainFrame : null;
            }
        }

        public static RootPage RootPage
        {
            get { return Window.Current.Content as RootPage; }
        }

        /// <summary>
        ///     Invoked when the application is launched normally by the end user.  Other entry points
        ///     will be used when the application is launched to open a specific file, to display
        ///     search results, and so forth.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected async override void OnLaunched(LaunchActivatedEventArgs args)
        {
            if (args.PreviousExecutionState == ApplicationExecutionState.Suspended ||
                args.PreviousExecutionState == ApplicationExecutionState.Terminated)
            {
                await SuspensionManager.RestoreAsync();
            }
            await LaunchTheApp();
        }

        async Task LaunchTheApp()
        {
            await (Container.Resolve<HistoryService>()).RestoreHistory();

            Window.Current.Content = Container.Resolve<RootPage>();
            Dispatcher = Window.Current.Content.Dispatcher;
            NavigationService.NavigateTo(typeof(MainPage));
            Window.Current.Activate();
        }

        protected override async void OnFileActivated(FileActivatedEventArgs args)
        {
            base.OnFileActivated(args);

            await ManageOpeningFiles(args);
        }

        async Task ManageOpeningFiles(FileActivatedEventArgs args)
        {
            var mediaServce = Container.Resolve<MediaService>();
            StorageFile file = (StorageFile)args.Files[0];
            if (file.FileType == ".mp3" || file.FileType == ".wma")
            {
                if (Window.Current.Content == null)
                {
                    await LaunchTheApp();
                }

                Locator.MusicPlayerVM.TrackCollection.TrackCollection.Clear();
                MusicLibraryViewModel.TrackItem trackItem = await GetInformationsFromMusicFile.GetTrackItemFromFile(file);
                Locator.MusicPlayerVM.TrackCollection.TrackCollection.Add(trackItem);
                await Locator.MusicPlayerVM.PlayFromExplorer(file);
            }
            else if (file.FileType == ".mkv"
                || file.FileType == ".avi"
                || file.FileType == ".mp4"
                || file.FileType == ".wmv"
                || file.FileType == ".mov")
            {
                TemporaryFileName = file.Name;
                TemporaryMRL = StorageApplicationPermissions.FutureAccessList.Add(file);
                if (Window.Current.Content == null)
                {
                    await LaunchTheApp();
                }
                else
                {
                    RootPage.MainFrame.Navigate(typeof(MainPage));
                    (ApplicationFrame.Content as MainPage).OpenVideoFromFileExplorer();
                }
            }
        }


        /// <summary>
        ///     Invoked when application execution is being suspended.  Application state is saved
        ///     without knowing whether the application will be terminated or resumed with the contents
        ///     of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            SuspendingDeferral deferral = e.SuspendingOperation.GetDeferral();
            await SuspensionManager.SaveAsync();
            deferral.Complete();
        }

    }
}
