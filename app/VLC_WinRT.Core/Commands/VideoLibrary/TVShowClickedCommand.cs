using System;
using System.Collections.Generic;
using System.Text;
using VLC.Model.Video;
using VLC.Utils;
using VLC.ViewModels;
using Windows.UI.Xaml.Controls;

namespace VLC.Commands.VideoLibrary
{
    public class TVShowClickedCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            TvShow show = null;
            if (parameter is TvShow)
                show = parameter as TvShow;
            else if (parameter is ItemClickEventArgs)
                show = ((ItemClickEventArgs)parameter).ClickedItem as TvShow;

            if (show == null) return;

            Locator.VideoLibraryVM.CurrentShow = show;
            Locator.NavigationService.Go(Model.VLCPage.TvShowView);
        }
    }
}
