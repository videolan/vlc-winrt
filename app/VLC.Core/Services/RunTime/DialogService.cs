using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VLC.UI.Views.UserControls.Shell;
using VLC.Utils;

namespace VLC.Services.RunTime
{
    public class DialogService
    {
        private VLCDialog _currentDialog;

        public async Task ShowErrorDialog(string title, string text)
        {
            await VLCDialog.WaitForDialogLock();
            await DispatchHelper.InvokeInUIThread(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                _currentDialog = new VLCDialog(title, text);
                await _currentDialog.ShowAsync();
            });
        }

        public async Task ShowLoginDialog(libVLCX.Dialog dialog, string title, string text, string defaultUserName, bool askToStore)
        {
            await VLCDialog.WaitForDialogLock();
            await DispatchHelper.InvokeInUIThread(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                _currentDialog = new VLCDialog(title, text, dialog, defaultUserName, askToStore);
                await _currentDialog.ShowAsync();
            });
        }

        public async Task ShowQuestionDialog(libVLCX.Dialog dialog, string title, string text, libVLCX.Question qType, string cancel, string action1, string action2)
        {
            if (qType == libVLCX.Question.warning)
            {
                dialog.postAction(1);
                return;
            }
            await VLCDialog.WaitForDialogLock();
            await DispatchHelper.InvokeInUIThread(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                _currentDialog = new VLCDialog(title, text, dialog, qType, cancel, action1, action2);
                await _currentDialog.ShowAsync();
            });
        }

        public async Task CancelCurrentDialog()
        {
            await DispatchHelper.InvokeInUIThread(Windows.UI.Core.CoreDispatcherPriority.Normal, () => _currentDialog.Cancel());
        }
    }
}
