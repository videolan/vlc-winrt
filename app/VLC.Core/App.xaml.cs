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
using Windows.Gaming.Input;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
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

            if (DeviceHelper.GetDeviceType() == DeviceTypeEnum.Xbox)
            {
                this.RequiresPointerMode = ApplicationRequiresPointerMode.WhenRequested;
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

            switch (args.Kind)
            {
                case ActivationKind.Protocol:
                    await HandleProtocolActivation(args);
                    break;
                case ActivationKind.VoiceCommand:
                    await CortanaHelper.HandleProtocolActivation(args);
                    break;
                case ActivationKind.ToastNotification:
                    await ToastHelper.HandleProtocolActivation(args);
                    break;
            }

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

        private async Task LaunchTheApp(bool disableConsumingTasks = false)
        {
            bool betaWarningShown = false;
            if (ApplicationSettingsHelper.Contains(nameof(betaWarningShown)))
                betaWarningShown = (bool)ApplicationSettingsHelper.ReadSettingsValue(nameof(betaWarningShown));

            if (!betaWarningShown)
            {
                showWarningDialog();
                ApplicationSettingsHelper.SaveSettingsValue(nameof(betaWarningShown), true);
            }

            Locator.GamepadService.StartListening();
            Dispatcher = Window.Current.Dispatcher;
            Window.Current.Content = new MainPage();
            Window.Current.Activate();
            await SplitShell.TemplateApplied.Task;
            Locator.NavigationService.BindSplitShellEvents();
            SetLanguage();
            SetShellDecoration();

            await LoadLibraries(disableConsumingTasks).ConfigureAwait(false);
            Locator.GamepadService.GamepadUpdated += (s, e) => Task.Run(() => ToggleMediaCenterMode());

            await ToggleMediaCenterMode();

            Locator.ExternalDeviceService.startWatcher();

            if (DeviceHelper.GetDeviceType() == DeviceTypeEnum.Xbox)
                Locator.HttpServer.bind(8080);
        }

        public static async Task reloadApplicationPage()
        {
            Locator.NavigationService.Reset();
            Window.Current.Content = new MainPage();
            await SplitShell.TemplateApplied.Task;
            Locator.NavigationService.BindSplitShellEvents();
            SetLanguage();
            SetShellDecoration();
            await ToggleMediaCenterMode();
        }

        private async void showWarningDialog()
        {
            var messageDialog = new MessageDialog(Strings.BetaWarning, Strings.BetaWarningTitle);

            // Add commands and set their callbacks; both buttons use the same callback function instead of inline event handlers
            messageDialog.Commands.Add(new UICommand(Strings.Ok));

            // Show the message dialog
            await messageDialog.ShowAsync();
        }

        async static Task ToggleMediaCenterMode()
        {
            await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (Gamepad.Gamepads.Any() || DeviceHelper.GetDeviceType() == DeviceTypeEnum.Xbox)
                {
                    Locator.SettingsVM.MediaCenterMode = true;
                    Locator.MainVM.CurrentPanel = Locator.MainVM.Panels.FirstOrDefault(x => x.Target == Locator.SettingsVM.HomePage);
                    Locator.MainVM.GoToHomePageMediaCenterCommand.Execute(null);

                    AppViewHelper.EnterFullscreen();

                    App.SplitShell.FooterContent = null;
                }
                else
                {
                    Locator.SettingsVM.MediaCenterMode = false;
                    Locator.MainVM.CurrentPanel = Locator.MainVM.Panels.FirstOrDefault(x => x.Target == Locator.SettingsVM.HomePage);

                    AppViewHelper.LeaveFullscreen();

                    App.SplitShell.FooterContent = new CommandBarBottom();
                }
                Locator.NavigationService.RefreshCurrentPage();
            });
        }

        private async void Current_VisibilityChanged(object sender, VisibilityChangedEventArgs e)
        {
            if (e.Visible)
            {
                if (Locator.MediaLibrary.AlreadyIndexedOnce)
                    return;
                await LoadLibraries(false);
            }
        }

        private Task LoadLibraries(bool disableConsumingTask)
        {
            return Task.Run(async () =>
            {
                if (!disableConsumingTask)
                {
                    Locator.MediaLibrary.DropTablesIfNeeded();
                    await Locator.MediaLibrary.CleanMediaLibrary().ConfigureAwait(false);
                    await Locator.MediaLibrary.Initialize().ConfigureAwait(false);
                }
            });
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
