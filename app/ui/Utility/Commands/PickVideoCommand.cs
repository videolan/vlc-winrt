using System;
using System.Diagnostics;
using System.Windows.Input;
using VLC_WINRT.Utility.Services.RunTime;
using VLC_WINRT.ViewModels;
using VLC_WINRT.Views;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace VLC_WINRT.Utility.Commands
{
    public class PickVideoCommand : ICommand
    {
        private static readonly object Locker = new object();
        private bool _canExecute = true;

        public bool CanExecute(object parameter)
        {
            lock (Locker)
            {
                return _canExecute;
            }
        }

        public async void Execute(object parameter)
        {
            lock (Locker)
            {
                _canExecute = false;
                CanExecuteChanged(this, new EventArgs());
            }

            var picker = new FileOpenPicker
                             {
                                 ViewMode = PickerViewMode.Thumbnail,
                                 SuggestedStartLocation = PickerLocationId.VideosLibrary
                             };

            //TODO: add more supported types
            picker.FileTypeFilter.Add(".avi");
            picker.FileTypeFilter.Add(".mp4");
            picker.FileTypeFilter.Add(".mkv");

            StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                var history = new HistoryService();
                string token = history.Add(file);

                Debug.WriteLine("Opening file: " + file.Path);
                Locator.PlayVideoVM.SetActiveVideoInfo(token, file.Name);
                NavigationService.NavigateTo(new PlayVideo());
            }
            else
            {
                Debug.WriteLine("Cancelled");
            }

            lock (Locker)
            {
                _canExecute = true;
                CanExecuteChanged(this, new EventArgs());
            }
        }

        public event EventHandler CanExecuteChanged;
    }
}