// The Blank Application template is documented at http://go.microsoft.com/fwlink/?LinkId=234227
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Autofac;
using Autofac.Core;
using VLC_WINRT.Common;
using VLC_WINRT.Utility.Helpers.MusicLibrary;
using VLC_WINRT.Utility.Services.RunTime;
using VLC_WINRT.ViewModels;
using VLC_WINRT.ViewModels.MainPage;
using VLC_WINRT.Views;
using VLC_WINRT_APP.ViewModels.MainPage;
using VLC_WINRT_APP.Views;
using VLC_WINRT_APP.Views.MainPages;

namespace VLC_WINRT_APP
{
    /// <summary>
    ///     Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public sealed partial class App : Application
    {
#if WINDOWS_PHONE_APP
        private TransitionCollection transitions;
#endif

        public static CoreDispatcher Dispatcher;
        public static IPropertySet LocalSettings = ApplicationData.Current.LocalSettings.Values;
        public static string ApiKeyLastFm = "a8eba7d40559e6f3d15e7cca1bfeaa1c";
        public static string DeezerAppID = "135671";
        // If != null; open the corresponding file
        public static string TemporaryMRL;
        public static string TemporaryFileName;

        public static IContainer Container;

        /// <summary>
        ///     Initializes the singleton application object.  This is the first line of authored code
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
                var root = Window.Current.Content as MainPage;
                return root != null ? root.MainFrame : null;
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

            if (args.PreviousExecutionState == ApplicationExecutionState.Suspended ||
                  args.PreviousExecutionState == ApplicationExecutionState.Terminated)
            {
                await SuspensionManager.RestoreAsync();
            }
            await LaunchTheApp();

            var rootFrame = Window.Current.Content as Frame;
            if (rootFrame != null && rootFrame.Content == null)
            {
#if WINDOWS_PHONE_APP
    // Removes the turnstile navigation for startup.
                if (rootFrame.ContentTransitions != null)
                {
                    this.transitions = new TransitionCollection();
                    foreach (var c in rootFrame.ContentTransitions)
                    {
                        this.transitions.Add(c);
                    }
                }

                rootFrame.ContentTransitions = null;
                rootFrame.Navigated += this.RootFrame_FirstNavigated;
#endif
            }

            // Ensure the current window is active
            Window.Current.Activate();
        }

#if WINDOWS_PHONE_APP
    /// <summary>
    /// Restores the content transitions after the app has launched.
    /// </summary>
    /// <param name="sender">The object where the handler is attached.</param>
    /// <param name="args">Details about the navigation event.</param>
        private void RootFrame_FirstNavigated(object sender, NavigationEventArgs args)
        {
            var rootFrame = sender as Frame;
            rootFrame.ContentTransitions = this.transitions ?? new TransitionCollection() { new NavigationThemeTransition() };
            rootFrame.Navigated -= this.RootFrame_FirstNavigated;
        }
#endif

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

        protected override async void OnFileActivated(FileActivatedEventArgs args)
        {
            base.OnFileActivated(args);

            await ManageOpeningFiles(args);
        }

        private async Task ManageOpeningFiles(FileActivatedEventArgs args)
        {
            var file = (StorageFile)args.Files[0];
            if (file.FileType == ".mp3" || file.FileType == ".wma")
            {
                if (Window.Current.Content == null)
                {
                    await LaunchTheApp();
                }

                Locator.MusicPlayerVM.TrackCollection.TrackCollection.Clear();
                MusicLibraryViewModel.TrackItem trackItem =
                    await GetInformationsFromMusicFile.GetTrackItemFromFile(file);
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

        private async Task LaunchTheApp()
        {
            Window.Current.Content = Container.Resolve<MainPage>();
            Dispatcher = Window.Current.Content.Dispatcher;
            ApplicationFrame.Navigate(typeof(MainPage));
            Window.Current.Activate();
        }
    }
}