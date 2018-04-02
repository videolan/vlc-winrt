
using Autofac;
using System;
using System.Linq;
using System.Threading.Tasks;
using VLC.Controls;
using VLC.Helpers;
using VLC.Model;
using VLC.UI.Views.MainPages;
using VLC.UI.Views.UserControls;
using VLC.Utils;
using VLC.ViewModels;
using VLC.ViewModels.Settings;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace VLC
{
    public sealed partial class App : Application
    {
        public static CoreDispatcher Dispatcher;
        public static IPropertySet LocalSettings = ApplicationData.Current.LocalSettings.Values;
        public static string ApiKeyLastFm = "a8eba7d40559e6f3d15e7cca1bfeaa1c";
        public static string ApiSecretLastFm = "bd9ad107438d9107296ef799703d478e";
        public static string ApiKeyMovieDb = "fdsfds";
        public static string DeezerAppID = "135671";
        public static OpenFilePickerReason OpenFilePickerReason = OpenFilePickerReason.Null;
        public static Model.Music.AlbumItem SelectedAlbumItem;
        public static IContainer Container;

        public App()
        {
            InitializeComponent();
            Suspending += OnSuspending;
            Container = AutoFacConfiguration.Configure();

            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 3) 
                && DeviceHelper.GetDeviceType() == DeviceTypeEnum.Xbox)
            {
                RequiresPointerMode = ApplicationRequiresPointerMode.WhenRequested;
                EnteredBackground += OnEnteredBackground;
                LeavingBackground += OnLeavingBackground;
            }
        }

        private void OnLeavingBackground(object sender, LeavingBackgroundEventArgs leavingBackgroundEventArgs)
        {
            ApplicationSettingsHelper.SaveSettingsValue("AppBackgrounded", false);
        }

        private void OnEnteredBackground(object sender, EnteredBackgroundEventArgs enteredBackgroundEventArgs)
        {
            ApplicationSettingsHelper.SaveSettingsValue("AppBackgrounded", true);

            if (Locator.PlaybackService.PlayingType == PlayingType.Video && Locator.PlaybackService.IsPlaying)
            {
                Locator.PlaybackService.Pause();
            }
        }

        public static Frame ApplicationFrame => RootPage?.NavigationFrame;

        public static MainPage RootPage => Window.Current?.Content as MainPage;

        public static SplitShell SplitShell => RootPage.SplitShell;

        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            if (Window.Current.Content == null)
            {
                if (args.PrelaunchActivated)
                {
                    Window.Current.VisibilityChanged += Current_VisibilityChanged;
                    await LaunchTheApp(true);
                    return;
                }
                await LaunchTheApp();
            }
            if (args.Arguments.Contains("SecondaryTile"))
            {
                await RedirectFromSecondaryTile(args.Arguments);
            }

            if (string.IsNullOrEmpty(ApiKeyMovieDb))
            {
                throw new ArgumentNullException(nameof(ApiKeyMovieDb), "VLC needs a valid MovieDB Api Key");
            }
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

                    await DispatchHelper.InvokeInUIThread(CoreDispatcherPriority.Normal, () => Locator.MusicLibraryVM.AlbumClickedCommand.Execute(id));
                }
                else if (args.Contains("Artist"))
                {
                    query = args.Replace("SecondaryTile-Artist-", "");
                    id = int.Parse(query);

                    await DispatchHelper.InvokeInUIThread(CoreDispatcherPriority.Normal, () => Locator.MusicLibraryVM.ArtistClickedCommand.Execute(id));
                }
            }
            catch (Exception e)
            {
                LogHelper.Log("Failed to open from secondary tile : " + e.ToString());
            }
        }

        protected async override void OnActivated(IActivatedEventArgs args)
        {
            base.OnActivated(args);
            if (Window.Current.Content == null)
            {
                await LaunchTheApp();
            }

            switch (args.Kind)
            {
                case ActivationKind.Protocol:
                    await HandleProtocolActivation(args);
                    break;
                case ActivationKind.VoiceCommand:
                    await CortanaHelper.HandleProtocolActivation(args);
                    break;
                case ActivationKind.ToastNotification:
                    ToastHelper.HandleProtocolActivation(args);
                    break;
            }

        }
        
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            SuspendingDeferral deferral = e.SuspendingOperation.GetDeferral();
            Locator.PlaybackService?.Trim();
            deferral.Complete();
        }

        protected override async void OnFileActivated(FileActivatedEventArgs args)
        {
            base.OnFileActivated(args);
            await ManageOpeningFiles(args);
        }

        private async Task ManageOpeningFiles(FileActivatedEventArgs args)
        {
            if (Window.Current.Content == null)
                await LaunchTheApp();
            try
            {
                await Locator.MediaPlaybackViewModel.OpenFile(args.Files[0] as StorageFile);
            }
            catch (Exception e)
            {
                LogHelper.Log($"MediaPlaybackViewModel.OpenFile failed with {e.Message}. {e.StackTrace}");
            }
        }

        private async Task HandleProtocolActivation(IActivatedEventArgs args)
        {
            var protocolArgs = (ProtocolActivatedEventArgs)args;
            var uri = protocolArgs.Uri;
            if (string.IsNullOrEmpty(uri.Query)) return;
            WwwFormUrlDecoder decoder = new WwwFormUrlDecoder(uri.Query);

            switch (uri.Host)
            {
                case "goto":
                    if (decoder[0]?.Name == "page")
                    {
                        if (decoder[0]?.Value == VLCPage.SettingsPageUI.ToString())
                            Locator.NavigationService.Go(VLCPage.SettingsPageUI);
                    }
                    break;
                case "openstream":
                    // do stuff
                    if (decoder[0]?.Name == "from")
                    {
                        switch (decoder[0]?.Value)
                        {
                            case "clipboard":
                                await Task.Delay(1000);
                                var dataPackage = Clipboard.GetContent();
                                Uri url = null;
                                if (dataPackage.Contains(StandardDataFormats.ApplicationLink))
                                {
                                    url = await dataPackage.GetApplicationLinkAsync();
                                }
                                else if (dataPackage.Contains(StandardDataFormats.WebLink))
                                {
                                    url = await dataPackage.GetWebLinkAsync();
                                }
                                else if (dataPackage.Contains(StandardDataFormats.Text))
                                {
                                    url = new Uri(await dataPackage.GetTextAsync());
                                }
                                if (url != null)
                                    await Locator.MediaPlaybackViewModel.PlayStream(url.AbsoluteUri);
                                break;
                            case "useraction":
                                Locator.MainVM.GoToStreamPanel.Execute(null);
                                break;
                            case "url":
                                if (decoder[1]?.Name == "url" )
                                {
                                    StorageFile subsFd = null;
                                    try
                                    {
                                        var subsFrom = decoder.Where((item, index) => item.Name == "subs_from").FirstOrDefault();
                                        var subsValue = decoder.Where((item, index) => item.Name == "subs").FirstOrDefault();
                                        if (!(subsFrom == default(IWwwFormUrlDecoderEntry) && subsValue == default(IWwwFormUrlDecoderEntry)))
                                        {
                                            switch (subsFrom.Value)
                                            {
                                                case "path":
                                                    //this StorageFile is like a File Descriptor from Unix
                                                    subsFd = await StorageFile.GetFileFromPathAsync(subsValue.Value);
                                                    if(subsFd == null)
                                                        ToastHelper.Basic("Failed to Load Subtitles: Couln´t find the file.");
                                                    else if(!StorageApplicationPermissions.FutureAccessList.CheckAccess(subsFd))
                                                        StorageApplicationPermissions.FutureAccessList.Add(subsFd);
                                                    break;
                                                case "url":
                                                    using (var httpClient = new System.Net.Http.HttpClient())
                                                    {
                                                        var subsContent = await httpClient.GetStringAsync(subsValue.Value);
                                                        subsFd = await ApplicationData.Current.LocalFolder.CreateFileAsync(subsContent.GetHashCode().ToString(), CreationCollisionOption.ReplaceExisting);
                                                        await FileIO.WriteTextAsync(subsFd, subsContent);
                                                    }
                                                    break;
                                                case "picker":
                                                    var openPicker = new FileOpenPicker();
                                                    openPicker.FileTypeFilter.Add(".srt");
                                                    openPicker.FileTypeFilter.Add(".txt");
                                                    openPicker.SuggestedStartLocation = PickerLocationId.Downloads;
                                                    subsFd = await openPicker.PickSingleFileAsync();
                                                    break;
                                            }
                                        }
                                    }
                                    //StorageFile.GetFileFromPath or CreateFileAsync failed
                                    catch (UnauthorizedAccessException)
                                    {
                                        ToastHelper.Basic("Failed to Load Subtitles: Access Denied to the file.");
                                    }
                                    //HttpClient usually fails with an AggregateException instead of WebException or HttpRequest...Exception
                                    catch (AggregateException)
                                    {
                                        ToastHelper.Basic("Failed to Load Subtitles: Problems downloading the subtitles");
                                    }
                                    //HttpClient fails with a WebException when there´s no connectivity
                                    catch (System.Net.WebException)
                                    {
                                        ToastHelper.Basic("Failed to Load Subtitles: No Connectivity");
                                    }
                                    catch (Exception ex)
                                    {
                                        ToastHelper.Basic("Failed to Load Subtitles: "+ ex.GetType().ToString());
                                    }

                                    await Locator.MediaPlaybackViewModel.PlayStream(decoder[1].Value);
                                    if (subsFd != null)
                                    {
                                        ToastHelper.Basic("Subtitles Loaded Successfully");
                                        Locator.MediaPlaybackViewModel.OpenSubtitleCommand.Execute(subsFd);
                                    }
                                        
                                }
                                break;
                        }
                    }
                    break;
            }
        }

        private async Task LaunchTheApp(bool disableConsumingTasks = false)
        {
            Locator.GamepadService.StartListening();
            Dispatcher = Window.Current.Dispatcher;
            Window.Current.Content = new MainPage();
            Window.Current.Activate();
            await SplitShell.TemplateApplied.Task;
            Locator.NavigationService.BindSplitShellEvents();
            SetLanguage();
            SetShellDecoration();

            if (disableConsumingTasks == false)
            {
                Locator.MediaLibrary.LoadAndCleanLibrariesAsync();
#if !Windows10
                Locator.PlaybackService.RestorePlaylistAsync();
#endif
            }

            ToggleMediaCenterMode();

            Locator.ExternalDeviceService.StartWatcher();
            Locator.RendererService.Start();

            if (DeviceHelper.GetDeviceType() == DeviceTypeEnum.Xbox)
                await Locator.HttpServer.Bind(8080).ConfigureAwait(false);
        }
        
        public static void ReloadApplicationPage()
        {
            Locator.PlaybackService.Stop();
            Locator.NavigationService.Reset();

            SetLanguage();
            SetShellDecoration();
            ToggleMediaCenterMode();
        }

        static void ToggleMediaCenterMode()
        {
            if (Locator.SettingsVM.MediaCenterMode)
            {
                Locator.MainVM.CurrentPanel = Locator.MainVM.Panels.FirstOrDefault(x => x.Target == Locator.SettingsVM.HomePage);
                Locator.MainVM.GoToHomePageMediaCenterCommand.Execute(null);

                AppViewHelper.EnterFullscreen();

                SplitShell.FooterContent = null;
            }
            else
            {
                Locator.MainVM.CurrentPanel = Locator.MainVM.Panels.FirstOrDefault(x => x.Target == Locator.SettingsVM.HomePage);

                AppViewHelper.LeaveFullscreen();

                SplitShell.FooterContent = new CommandBarBottom();
            }
            Locator.NavigationService.RefreshCurrentPage();
        }

        private void Current_VisibilityChanged(object sender, VisibilityChangedEventArgs e)
        {
            if (e.Visible)
            {
                if (Locator.MediaLibrary.AlreadyIndexedOnce)
                    return;
                Locator.MediaLibrary.LoadAndCleanLibrariesAsync();
            }
        }

        public static void SetLanguage()
        {
            var language = SettingsViewModel.GetSelectedLanguage();
            SettingsViewModel.SwitchLanguage(language);
        }

        public static void SetShellDecoration(bool forceTemporaryAppTheme = false, bool forceDark = false)
        {
            if (forceTemporaryAppTheme)
            {
                RootPage.RequestedTheme = forceDark ? ElementTheme.Dark : ElementTheme.Light;
                RootPage.SetBackground(forceTemporaryAppTheme, forceDark);
            }
            else
            {
                RootPage.SetBackground(false);
                var appTheme = SettingsViewModel.GetApplicationTheme() == ApplicationTheme.Light;
                RootPage.RequestedTheme = appTheme ? ElementTheme.Light : ElementTheme.Dark;
            }

            Current.Resources["MainColorBase"] = new SolidColorBrush(Locator.SettingsVM.AccentColor.Color);
            Current.Resources["SystemAccentColor"] = new SolidColorBrush(Locator.SettingsVM.AccentColor.Color);
            Current.Resources["MainColor"] = new SolidColorBrush(Locator.SettingsVM.AccentColor.Color);
            Current.Resources["TranslucentMainColor"] = new SolidColorBrush
            {
                Color = Locator.SettingsVM.AccentColor.Color,
                Opacity = 0.9
            };

            var lightColor = new SolidColorBrush(Color.FromArgb(179, Locator.SettingsVM.AccentColor.Color.R, 
                Locator.SettingsVM.AccentColor.Color.G, Locator.SettingsVM.AccentColor.Color.B));

            Current.Resources["SemiLightMainColorBase"] = lightColor;
            Current.Resources["SemiLightMainColor"] = lightColor;
        }
    }
}
