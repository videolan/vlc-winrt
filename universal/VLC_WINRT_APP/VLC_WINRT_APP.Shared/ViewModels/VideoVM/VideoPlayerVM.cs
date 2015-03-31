/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using VLC_WinRT.Common;
using Windows.Storage;
using System;
using System.Threading.Tasks;
using VLC_WinRT.Model.Video;
using VLC_WinRT.Helpers;
using VLC_WinRT.Model;

namespace VLC_WinRT.ViewModels.VideoVM
{
    public class VideoPlayerVM : BindableBase
    {
        #region private props
        private VideoItem _currentVideo;
        #endregion

        #region private fields
        #endregion

        #region public props
        public VideoItem CurrentVideo
        {
            get { return _currentVideo; }
            set { SetProperty(ref _currentVideo, value); }
        }
        #endregion

        #region public fields
        #endregion

        #region constructors
        #endregion

        #region methods
        #endregion
    }
}
