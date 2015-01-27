﻿using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using VLC_WINRT.Common;
using System;
using System.Collections.Generic;
using System.Net;
using Windows.Storage;
using Windows.Storage.Streams;
using VLC_WINRT_APP.ViewModels;

namespace VLC_WINRT_APP.Commands.Social
{
    public class ShareNowPlayingMusicCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            if (Locator.MusicPlayerVM.CurrentTrack == null || Locator.MusicPlayerVM.CurrentAlbum == null ||
                Locator.MusicPlayerVM.CurrentAlbum == null) return;
            RegisterForShare();
            DataTransferManager.ShowShareUI();
        }

        private void RegisterForShare()
        {
            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += new TypedEventHandler<DataTransferManager,
                DataRequestedEventArgs>(this.ShareLinkHandler);
        }

        private async void ShareLinkHandler(DataTransferManager sender, DataRequestedEventArgs e)
        {
            DataRequest request = e.Request;
            string title = "#NowPlaying " + Locator.MusicPlayerVM.CurrentTrack.Name + " - " + Locator.MusicPlayerVM.CurrentArtist.Name;

            request.Data.Properties.Title = title;
            request.Data.Properties.Description = title;
            var uri = string.Format("http://www.last.fm/music/{0}/{1}", Locator.MusicPlayerVM.CurrentArtist.Name, Locator.MusicPlayerVM.CurrentAlbum.Name);
            request.Data.SetWebLink(new Uri(uri, UriKind.Absolute));
        }

    }
}
