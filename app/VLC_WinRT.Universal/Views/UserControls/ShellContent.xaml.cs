using System;
using System.Linq;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.System;
using Windows.UI.ApplicationSettings;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using VLC_WinRT.Model;
using VLC_WinRT.ViewModels;
using VLC_WinRT.Utils;

namespace VLC_WinRT.Views.UserControls
{
    public sealed partial class ShellContent : UserControl
    {
        public ShellContent()
        {
            this.InitializeComponent();
        }

        private void MainFrame_OnNavigated(object sender, NavigationEventArgs e)
        {
#if WINDOWS_APP
            SettingsPane pane = SettingsPane.GetForCurrentView();
            pane.CommandsRequested += SettingsCommandRequested;
#endif
        }

#if WINDOWS_APP
        private void SettingsCommandRequested(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs args)
        {
            var privacyCommand = new SettingsCommand("privacy", Strings.PrivacyStatement,
                async h => await Launcher.LaunchUriAsync(new Uri("http://videolan.org/vlc/privacy.html")));

            var specialThanks = new SettingsCommand("specialThanks", Strings.SpecialThanks,
                command =>
                {
                    Locator.NavigationService.Go(VLCPage.SpecialThanksPage);
                });

            var settings = new SettingsCommand("settings", Strings.Settings,
                command =>
                {
                    Locator.NavigationService.Go(VLCPage.SettingsPage);
                });
            var license = new SettingsCommand("license", Strings.License, command =>
            {
                Locator.NavigationService.Go(VLCPage.LicensePage);
            });
            args.Request.ApplicationCommands.Clear();
            args.Request.ApplicationCommands.Add(privacyCommand);
            args.Request.ApplicationCommands.Add(specialThanks);
            args.Request.ApplicationCommands.Add(settings);
            args.Request.ApplicationCommands.Add(license);
        }
#endif

        private void NavigationFrame_OnDragOver(object sender, DragEventArgs e)
        {
            // Allow linking to the file location, so we can play it back.
            e.AcceptedOperation = DataPackageOperation.Link;
            e.Handled = true;
        }

        private async void NavigationFrame_OnDrop(object sender, DragEventArgs e)
        {
            var d = e.GetDeferral();
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var files = await e.DataView.GetStorageItemsAsync();
                // TODO: Add playlist support for videos, so if you drag more than one, it will add them all to a list.
                var file = files.First();
                d.Complete();
                await Locator.MediaPlaybackViewModel.OpenFile(file as StorageFile);
            }
        }
    }
}
