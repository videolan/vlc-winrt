/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using VLC.ViewModels;
using Microsoft.Xaml.Interactivity;
using VLC.Model.Video;
using VLC.UI.Views.MainPages.MainVideoControls;
using VLC.ViewModels.VideoVM;
using VLC.Helpers;
using System.Diagnostics;
using System.Linq;

namespace VLC.UI.Views.MainPages
{
    public sealed partial class MainPageVideos : Page
    {
        public MainPageVideos()
        {
            InitializeComponent();    
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            Locator.VideoLibraryVM.OnNavigatedTo();
            if (MainPageVideoContentPresenter.Content == null)
            {
                Switch(Locator.VideoLibraryVM.VideoView);
            }
            
            Locator.VideoLibraryVM.PropertyChanged += VideoLibraryVM_PropertyChanged;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            Locator.VideoLibraryVM.OnNavigatedFrom();
            Locator.VideoLibraryVM.PropertyChanged -= VideoLibraryVM_PropertyChanged;
        }

        private void VideoLibraryVM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(VideoLibraryVM.VideoView))
            {
                Switch(Locator.VideoLibraryVM.VideoView);
            }
        }

        private void Switch(VideoView view)
        {
            switch (view)
            {
                case VideoView.Videos:
                    if (!(MainPageVideoContentPresenter.Content is AllVideosPivotItem))
                        MainPageVideoContentPresenter.Content = new AllVideosPivotItem();
                    break;
                case VideoView.Shows:
                    if (!(MainPageVideoContentPresenter.Content is ShowsPivotItem))
                        MainPageVideoContentPresenter.Content = new ShowsPivotItem();
                    break;
                case VideoView.CameraRoll:
                    Debug.Assert(DeviceHelper.GetDeviceType() != DeviceTypeEnum.Xbox);
                    if (!(MainPageVideoContentPresenter.Content is CameraRollPivotItem))
                        MainPageVideoContentPresenter.Content = new CameraRollPivotItem();
                    break;
            }
        }
    }
}