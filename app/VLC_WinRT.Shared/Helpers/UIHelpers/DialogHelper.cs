using System;
using System.Collections.Generic;
using System.Text;
using libVLCX;
using VLC_WinRT.UI.UWP.Views.UserControls.Shell;
using System.Threading.Tasks;
using System.Threading;

namespace VLC_WinRT.Helpers.UIHelpers
{
    public static class DialogHelper
    {
        static SemaphoreSlim DialogDisplaySemaphoreSlim = new SemaphoreSlim(1);
        private static void Dialog_Closed(Windows.UI.Xaml.Controls.ContentDialog sender, Windows.UI.Xaml.Controls.ContentDialogClosedEventArgs args)
        {
            DialogDisplaySemaphoreSlim.Release();
        }

        public static async Task DisplayDialog(string title, string desc)
        {
            await DialogDisplaySemaphoreSlim.WaitAsync();
            try
            {
                var dialog = new VLCDialog();
                dialog.Closed += Dialog_Closed;
                dialog.Initialize(title, desc);
                await dialog.ShowAsync();
            }
            catch { }
        }

        public static async Task DisplayDialog(string title, string desc, Dialog d, string username = null, bool askStore = false)
        {
            await DialogDisplaySemaphoreSlim.WaitAsync();
            try
            {
                var dialog = new VLCDialog();
                dialog.Closed += Dialog_Closed;
                dialog.Initialize(title, desc, d, username, askStore);
                await dialog.ShowAsync();
            }
            catch { }
        }

        public static async Task DisplayDialog(string title, string desc, Dialog d, Question questionType, string cancel, string action1, string action2)
        {
            await DialogDisplaySemaphoreSlim.WaitAsync();
            try
            {
                if (questionType == Question.warning)
                {
                    d.postAction(1);
                    return;
                }

                var dialog = new VLCDialog();
                dialog.Closed += Dialog_Closed;
                dialog.Initialize(title, desc, d, questionType, cancel, action1, action2);
                await dialog.ShowAsync();
            }
            catch { }
        }
    }
}