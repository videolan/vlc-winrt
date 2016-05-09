using System;
using System.Collections.Generic;
using System.Text;
using libVLCX;
using VLC_WinRT.UI.UWP.Views.UserControls.Shell;
using System.Threading.Tasks;

namespace VLC_WinRT.Helpers.UIHelpers
{
    public static class DialogHelper
    {
        public static async Task DisplayDialog(string title, string desc)
        {
            var dialog = new VLCDialog();
            dialog.Initialize(title, desc);
            await dialog.ShowAsync();
        }

        public static async Task DisplayDialog(string title, string desc, Dialog d, string username = null, bool askStore = false)
        {
            var dialog = new VLCDialog();
            dialog.Initialize(title, desc, d, username, askStore);
            await dialog.ShowAsync();
        }

        public static async Task DisplayDialog(string title, string desc, Dialog d, Question questionType, string cancel, string action1, string action2)
        {
            var dialog = new VLCDialog();
            dialog.Initialize(title, desc, d, questionType, cancel, action1, action2);
            await dialog.ShowAsync();
        }
    }
}