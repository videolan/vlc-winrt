using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VLC_WINRT.Common;
using VLC_WINRT.Utility.Services.RunTime;
using VLC_WINRT.ViewModels.MainPage;
namespace VLC_WINRT.ViewModels.PlayMusic
{
    public class TrackCollectionViewModel : BindableBase
    {
        private ObservableCollection<MusicLibraryViewModel.TrackItem> _tracksCollection;
        private int _currentTrack = -1;

        public TrackCollectionViewModel()
        {
            _tracksCollection = new ObservableCollection<MusicLibraryViewModel.TrackItem>();
        }

        public ObservableCollection<MusicLibraryViewModel.TrackItem> TrackCollection
        {
            get { return _tracksCollection; }
            set { SetProperty(ref _tracksCollection, value); }
        }

        public int CurrentTrack
        {
            get { return _currentTrack; }
            set { SetProperty(ref _currentTrack, value); }
        }


        // Tracks Collection Manager
        public void AddTrack(MusicLibraryViewModel.TrackItem track)
        {
            TrackCollection.Add(track);
        }

        public void AddTrack(List<MusicLibraryViewModel.TrackItem> tracks)
        {
            foreach (MusicLibraryViewModel.TrackItem track in tracks)
                TrackCollection.Add(track);
        }

        public bool CanGoPrevious()
        {
            return CurrentTrack >= 0;
        }
        public bool CanGoNext()
        {
            return CurrentTrack < TrackCollection.Count;
        }
        public void ResetCollection()
        {
            TrackCollection.Clear();
            CurrentTrack = -1;
        }
    }
}
