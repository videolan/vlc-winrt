using System;
using Windows.System;
using Windows.UI.ApplicationSettings;
using Windows.UI.Xaml.Controls;

namespace VLC_WINRT.Views
{
    public class BasePage : Page, IDisposable
    {
        public BasePage()
        {
            SettingsPane.GetForCurrentView().CommandsRequested += SettingsCommandRequested;
        }

        public void Dispose()
        {
            SettingsPane.GetForCurrentView().CommandsRequested -= SettingsCommandRequested;
        }

        private void SettingsCommandRequested(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs args)
        {
            var privacyCommand = new SettingsCommand("privacy", "Privacy Statement",
                h => Launcher.LaunchUriAsync(new Uri("http://videolan.org/vlc/privacy.html")));

            args.Request.ApplicationCommands.Clear();
            args.Request.ApplicationCommands.Add(privacyCommand);
        }
    }
}