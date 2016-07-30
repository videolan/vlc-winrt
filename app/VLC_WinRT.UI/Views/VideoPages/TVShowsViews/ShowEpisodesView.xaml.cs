using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using VLC.Model.Video;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace VLC.UI.Legacy.Views.VideoPages.TVShowsViews
{
    public sealed partial class ShowEpisodesView : Page
    {
        public ShowEpisodesView()
        {
            this.InitializeComponent();
        }

        private void VideosWrapGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            TemplateSizer.ComputeCompactVideo(sender as ItemsWrapGrid, this.ActualWidth);
        }
    }
}
