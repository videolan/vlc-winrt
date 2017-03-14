/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using VLC.Commands.MediaLibrary;
using VLC.Commands.Navigation;
using VLC.Commands.VideoLibrary;
using VLC.Commands.VideoPlayer;
using VLC.Helpers;
using VLC.Model;
using VLC.Model.Video;
using VLC.Utils;
using Windows.UI.Core;

namespace VLC.ViewModels.VideoVM
{
    public class VideoLibraryVM : BindableBase
    {
        #region private fields
        #endregion

        #region private props
        private VideoView _videoView;
        private TvShow _currentShow;
        private List<VideoView> _videoViewCollection;
        #endregion

        #region public fields
        public List<VideoView> VideoViewCollection
        {
            get
            {
                if (_videoViewCollection == null)
                {
                    _videoViewCollection = new List<VideoView>()
                    {
                        VideoView.Videos,
                        VideoView.Shows,
                    };
                    if (DeviceHelper.GetDeviceType() != DeviceTypeEnum.Xbox)
                        _videoViewCollection.Add(VideoView.CameraRoll);
                }
                return _videoViewCollection;
            }
        }

        public ObservableCollection<VideoItem> Videos
        {
            get { return Locator.MediaLibrary.Videos; }
        }

        public ObservableCollection<TvShow> Shows
        {
            get { return Locator.MediaLibrary.Shows; }
        }

        public ObservableCollection<VideoItem> CameraRoll
        {
            get { return Locator.MediaLibrary.CameraRoll; }
        }
        #endregion

        #region public props
        public VideoView VideoView
        {
            get
            {
                var videoView = ApplicationSettingsHelper.ReadSettingsValue(nameof(VideoView), false);
                if (videoView == null)
                {
                    _videoView = VideoView.Videos;
                }
                else
                {
                    _videoView = (VideoView)videoView;
                }
                return _videoView;
            }
            set
            {
                ApplicationSettingsHelper.SaveSettingsValue(nameof(VideoView), (int)value, false);
                SetProperty(ref _videoView, value);
            }
        }

        public TvShow CurrentShow
        {
            get { return _currentShow; }
            set { SetProperty(ref _currentShow, value); }
        }

        public static TVShowClickedCommand TVShowClickedCommand { get; private set; } = new TVShowClickedCommand();
        public PlayVideoCommand OpenVideo { get; private set; } = new PlayVideoCommand();

        public CloseFlyoutAndPlayVideoCommand CloseFlyoutAndPlayVideoCommand { get; private set; } = new CloseFlyoutAndPlayVideoCommand();
        public DeleteFromLibraryCommand DeleteFromLibraryCommand { get; private set; } = new DeleteFromLibraryCommand();
        public ChangeVideoViewCommand ChangeVideoViewCommand { get; private set; } = new ChangeVideoViewCommand();
        #endregion

        #region methods
        public void OnNavigatedTo()
        {
            CurrentShow = null;
        }

        public void OnNavigatedFrom()
        {
            CurrentShow = null;
        }
        #endregion
    }
}