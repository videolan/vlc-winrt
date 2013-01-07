using System;
using System.Diagnostics;
using System.Windows.Input;
using VLC_WINRT.Common;
using VLC_WINRT.Views;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace VLC_WINRT.Utility.Commands
{
    public class PickVideoCommand : ICommand
    {
        private static readonly object locker = new object();
        private bool _canExecute = true;

        public bool CanExecute(object parameter)
        {
            lock (locker)
            {
                return _canExecute;
            }
        }

        public async void Execute(object parameter)
        {
            lock (locker)
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

            StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                Debug.WriteLine("Opening file: " + file.Path);
                PlayVideo.CurrentFile = file;
                ((Frame) Window.Current.Content).Navigate(typeof (PlayVideo));
            }
            else
            {
                Debug.WriteLine("Cancelled");
            }

            lock (locker)
            {
                _canExecute = true;
                CanExecuteChanged(this, new EventArgs());
            }
        }

        public event EventHandler CanExecuteChanged;
    }
}