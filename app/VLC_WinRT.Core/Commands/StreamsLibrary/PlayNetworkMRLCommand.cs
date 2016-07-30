/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System;
using Windows.UI.Xaml.Controls;
using VLC.Helpers;
using VLC.Model.Stream;
using VLC.ViewModels;
using VLC.Model;
using VLC.Utils;

namespace VLC.Commands.StreamsLibrary
{
    public class PlayNetworkMRLCommand : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            string mrl = null;
            if (parameter is string)
            {
                mrl = (string)parameter;
            }
            else if (parameter is ItemClickEventArgs)
            {
                mrl = (((ItemClickEventArgs) parameter).ClickedItem as StreamMedia)?.Path;
            }
            if (string.IsNullOrEmpty(mrl))
            {
                return;
            }
            await Locator.MediaPlaybackViewModel.PlayStream(mrl);
        }
    }
}
