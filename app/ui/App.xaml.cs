/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VLC_WINRT.Common;
using VLC_WINRT.Utility.Helpers;
using VLC_WINRT.Utility.Helpers.MusicLibrary;
using VLC_WINRT.Utility.Services.RunTime;
using VLC_WINRT.ViewModels;
using VLC_WINRT.ViewModels.MainPage;
using VLC_WINRT.Views;

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

        /// <summary>
        ///     Initializes the singleton Application object.  This is the first line of authored code
        ///     executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();

            Suspending += OnSuspending;
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
        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            LaunchTheApp();
        }

        void LaunchTheApp()
        {
            Window.Current.Content = new RootPage();
            Dispatcher = Window.Current.Content.Dispatcher;
            NavigationService.NavigateTo(typeof(MainPage));
            Window.Current.Activate();
            LoadBackers.Get();
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

        protected async override void OnFileActivated(FileActivatedEventArgs args)
        {
            base.OnFileActivated(args);

            if (Window.Current.Content == null)
            {
                LaunchTheApp();
            }
            StorageFile file = (StorageFile)args.Files[0];
            if (file.FileType == ".mp3" || file.FileType == ".wma")
            {
                Locator.MusicPlayerVM.TrackCollection.TrackCollection.Clear();
                MusicLibraryViewModel.TrackItem trackItem = await GetInformationsFromMusicFile.GetTrackItemFromFile(file);
                Locator.MusicPlayerVM.TrackCollection.TrackCollection.Add(trackItem);
                Locator.MusicPlayerVM.PlayFromExplorer(file);
            }
        }
    }
}
