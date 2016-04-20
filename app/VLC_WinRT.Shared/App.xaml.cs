using VLC_WinRT.Model.Music;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Autofac;
using VLC_WinRT.Helpers;
using VLC_WinRT.Model;
using VLC_WinRT.Services.RunTime;
using VLC_WinRT.ViewModels;
using VLC_WinRT.Views.MainPages;
using VLC_WinRT.ViewModels.MusicVM;
using System.Linq;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Storage.AccessCache;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Media;
using VLC_WinRT.BackgroundHelpers;
using VLC_WinRT.Utils;
using VLC_WinRT.Controls;
using VLC_WinRT.Views.UserControls;
using VLC_WinRT.ViewModels.Settings;
using WinRTXamlToolkit.IO.Serialization;
using VLC_WinRT.Helpers.VideoLibrary;
using Windows.System.Threading;

namespace VLC_WinRT
{
    public sealed partial class App : Application
    {
        public static CoreDispatcher Dispatcher;
        public static IPropertySet LocalSettings = ApplicationData.Current.LocalSettings.Values;
        public static string ApiKeyLastFm = "a8eba7d40559e6f3d15e7cca1bfeaa1c";
        public static string ApiSecretLastFm = "bd9ad107438d9107296ef799703d478e"; 
        public static string DeezerAppID = "135671";
        public static OpenFilePickerReason OpenFilePickerReason = OpenFilePickerReason.Null;
        public static Model.Music.AlbumItem SelectedAlbumItem;
        public static IContainer Container;
        public static BackgroundAudioHelper BackgroundAudioHelper = new BackgroundAudioHelper();
        
        public App()
        {
            InitializeComponent();
            Suspending += OnSuspending;
            Container = AutoFacConfiguration.Configure();
        }

        public static Frame ApplicationFrame => RootPage?.NavigationFrame;

        public static MainPage RootPage => Window.Current?.Content as MainPage;

        public static SplitShell SplitShell => RootPage.SplitShell;
        
        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            if (Window.Current.Content == null)
            {
                await LaunchTheApp();
            }
            if (args.Arguments.Contains("SecondaryTile"))
            {
                await RedirectFromSecondaryTile(args.Arguments);
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

                    await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () => Locator.MusicLibraryVM.AlbumClickedCommand.Execute(id));
                }
                else if (args.Contains("Artist"))
                {
                    query = args.Replace("SecondaryTile-Artist-", "");
                    id = int.Parse(query);

                    await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () => Locator.MusicLibraryVM.ArtistClickedCommand.Execute(id));
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
            if (args.Kind == ActivationKind.Protocol)
            {
                await HandleProtocolActivation(args);
            }

#if WINDOWS_PHONE_APP
            if (args.Kind == ActivationKind.FileOpenPicker)
            {
                try
                {
                    var continueArgs = args as FileOpenPickerContinuationEventArgs;
                    if (continueArgs != null && continueArgs.Files.Any())
                    {
                        switch (OpenFilePickerReason)
                        {
                            case OpenFilePickerReason.OnOpeningVideo: 
                                await Locator.MediaPlaybackViewModel.OpenFile(continueArgs.Files[0]);
                                break;
                            case OpenFilePickerReason.OnOpeningSubtitle:
                                {
                                    Locator.MediaPlaybackViewModel.OpenSubtitleCommand.Execute(continueArgs.Files[0]);
                                }
                                break;
                            case OpenFilePickerReason.OnPickingAlbumArt:
                                if (continueArgs.Files == null) return;
                                var file = continueArgs.Files.First();
                                if (file == null) return;
                                var byteArray = await ConvertImage.ConvertImagetoByte(file);
                                await Locator.MusicMetaService.SaveAlbumImageAsync(SelectedAlbumItem, byteArray);
                                await Locator.MediaLibrary.Update(SelectedAlbumItem);
                                SelectedAlbumItem = null;
                                break;
                        }
                    }
                    OpenFilePickerReason = OpenFilePickerReason.Null;
                }
                catch (Exception e)
                {
                    LogHelper.Log(StringsHelper.ExceptionToString(e));
                }
            }
#endif
        }
        
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            SuspendingDeferral deferral = e.SuspendingOperation.GetDeferral();
            Locator.VLCService?.Trim();
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
            await Locator.MediaPlaybackViewModel.OpenFile(args.Files[0] as StorageFile);
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
#if WINDOWS_PHONE_APP
#else
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
#endif
                                break;
                            case "useraction":
                                Locator.MainVM.OpenStreamFlyout();
                                break;
                            case "url":
                                if (decoder[1]?.Name == "url")
                                {
                                    if (!string.IsNullOrEmpty(decoder[1]?.Value))
                                    {
                                        await Locator.MediaPlaybackViewModel.PlayStream(decoder[1].Value);
                                    }
                                }
                                break;
                        }
                    }
                    break;
            }
        }

        private async Task LaunchTheApp()
        {
            Dispatcher = Window.Current.Dispatcher;
            Window.Current.Content = new MainPage();
            Window.Current.Activate();
            await SplitShell.TemplateApplied.Task;
            SetLanguage();
            SetShellDecoration();
#if WINDOWS_PHONE_APP
            await StatusBarHelper.Initialize();
#endif
            await Task.Run(async () =>
            {
                Locator.MediaLibrary.DropTablesIfNeeded();
                Locator.MediaLibrary.DropTablesIfNeeded();
                await Task.Factory.StartNew(async () => await Locator.MediaLibrary.Initialize()).ConfigureAwait(false);
                await Task.Factory.StartNew(async () => await Locator.MediaLibrary.Initialize()).ConfigureAwait(false);
            });
            await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () => Locator.NavigationService.Go(Locator.SettingsVM.HomePage));
        }

        public static void SetLanguage()
        {
            var language = SettingsViewModel.GetSelectedLanguage();
            SettingsViewModel.SwitchLanaguage(language);
        }

        public static void SetShellDecoration(bool forceTemporaryAppTheme = false, bool forceDark = false)
        {
            if (forceTemporaryAppTheme)
            {
                RootPage.RequestedTheme = forceDark ? ElementTheme.Dark : ElementTheme.Light;
                App.RootPage.SetBackground(forceTemporaryAppTheme, forceDark);
            }
            else
            {
                App.RootPage.SetBackground(false);
                var appTheme = SettingsViewModel.GetApplicationTheme() == ApplicationTheme.Light;
                RootPage.RequestedTheme = appTheme ? ElementTheme.Light : ElementTheme.Dark;
            }
            App.Current.Resources["MainColorBase"] = Locator.SettingsVM.AccentColor.Color;
            App.Current.Resources["SystemAccentColor"] = Locator.SettingsVM.AccentColor.Color;
            App.Current.Resources["MainColor"] = new SolidColorBrush()
            {
                Color = Locator.SettingsVM.AccentColor.Color
            };
            App.Current.Resources["TranslucentMainColor"] = new SolidColorBrush()
            {
                Color = Locator.SettingsVM.AccentColor.Color,
                Opacity = 0.9
            };
            var lightColor = Color.FromArgb(179, Locator.SettingsVM.AccentColor.Color.R, Locator.SettingsVM.AccentColor.Color.G, Locator.SettingsVM.AccentColor.Color.B);
            App.Current.Resources["SemiLightMainColorBase"] = lightColor;
            App.Current.Resources["SemiLightMainColor"] = new SolidColorBrush() { Color = lightColor };
        }
    }
}