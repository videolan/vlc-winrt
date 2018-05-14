using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using libVLCX;
using VLC.Helpers;
using VLC.Model;
using VLC.Utils;
using VLC.ViewModels;

namespace VLC.Universal.Views.UserControls.Shell
{
    public sealed partial class TranscodingDialog : ContentDialog
    {
        MediaPlayer _mediaPlayer;
        Instance _instance;
        string _currentTranscodedFileName;
        string _transcodedFilePathRoaming;
        string _selectedProfile;
        const string Mkv = ".mkv";
        const string Webm = ".webm";
        string Extension => _selectedProfile == "VP9" ? Webm : Mkv;
        string TranscodingMaxWidth => _selectedProfile == "720p" ? "1280" : "1920";
        string TranscodingMaxHeight => _selectedProfile == "720p" ? "720" : "1080";
        public IMediaItem Media { get; set; }

        public TranscodingDialog()
        {
            InitializeComponent();
        }
        
        // needs to be accessed from UI thread
        string TranscodedFileName => Media.Name + Locator.SettingsVM.VLCTranscoded + Profiles.SelectedItem + Extension;

        string GetTranscodedFilePathAsync()
        {          
            var roamingFolderPath = ApplicationData.Current.RoamingFolder.Path;          
            _currentTranscodedFileName = TranscodedFileName;
            return Path.Combine(roamingFolderPath, _currentTranscodedFileName);
        }

        async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            args.Cancel = true;

            _selectedProfile = Profiles.SelectedItem as string;

            var options = new List<string>{"-vv"};

            _instance = new Instance(options);

            Media.VlcMedia = new Media(_instance, Media.Path, FromType.FromPath);
            _transcodedFilePathRoaming = GetTranscodedFilePathAsync();
          
            var transcodeOption = GetTranscodeOptionString(_transcodedFilePathRoaming, _selectedProfile);
            if (string.IsNullOrEmpty(transcodeOption)) //abort
            {
                LogHelper.Log("could not setup transcode options with path " + _transcodedFilePathRoaming + " and selectedProfile " + _selectedProfile);
                return;
            } 
            
            Media.VlcMedia.addOption(transcodeOption);

            _mediaPlayer = new MediaPlayer(Media.VlcMedia);

            _mediaPlayer.eventManager().OnPositionChanged += OnOnPositionChanged;
            _mediaPlayer.eventManager().OnEndReached += OnOnEndReached;
            _mediaPlayer.eventManager().OnEncounteredError += OnOnEncounteredError;

            _mediaPlayer.play();
            
            await DispatchHelper.InvokeInUIThread(CoreDispatcherPriority.Normal, () =>
            {
                ProgressRing.Visibility = Visibility.Visible;
                Progress.Visibility = Visibility.Visible;
                IsPrimaryButtonEnabled = false;
            });
        }

        async void OnOnEncounteredError()
        {
            LogHelper.Log($"error while transcoding file dst {_transcodedFilePathRoaming}");
            await DispatchHelper.InvokeInUIThread(CoreDispatcherPriority.Normal, () =>
            {
                ProgressRing.Visibility = Visibility.Collapsed;
            });
            await CleanupAndHide();
        }

        async void OnOnPositionChanged(float transcodingProgress)
        {
            var progress = (int)(transcodingProgress * 100) + "%";
            await DispatchHelper.InvokeInUIThread(CoreDispatcherPriority.Normal, () =>
            {
                Progress.Text = progress;
            });
        }

        async void OnOnEndReached()
        {
            var videoLibrary = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Videos);
            var file = await StorageFile.GetFileFromPathAsync(_transcodedFilePathRoaming);

            // copy transcoded file from sandbox to video fodler
            await file.CopyAsync(videoLibrary.SaveFolder, _currentTranscodedFileName, NameCollisionOption.GenerateUniqueName);

            await CleanupAndHide();           
        }

        string GetTranscodeOptionString(string transcodedFilePath, string selectedProfile)
        {
            if (string.IsNullOrEmpty(selectedProfile) || string.IsNullOrEmpty(selectedProfile)) return string.Empty;

            switch (selectedProfile)
            {
                case "VP9":
                    return
                        ":sout=#transcode{vcodec=VP90,vb=2000,acodec=vorb}" +
                        ":std{access=file,mux=avformat{mux=webm},dst='" + transcodedFilePath + "'}'";
                case "720p":
                    return
                        ":sout=#transcode{venc=qsv{rc-method=vbr,bitrate-max=40000000,gop-size=24,target-usage=speed}," +
                        "vcodec=h264,acodec=mp4a,maxwidth=" + TranscodingMaxWidth + ",maxheight=" + TranscodingMaxHeight + "}" +
                        ":std{access=file,mux=mkv,dst='" + transcodedFilePath + "'}'";
                case "1080p":
                    return
                        ":sout=#transcode{venc=qsv{rc-method=vbr,bitrate-max=40000000,gop-size=24,target-usage=speed}," +
                        "vcodec=h264,acodec=mp4a,maxwidth=" + TranscodingMaxWidth + ",maxheight=" + TranscodingMaxHeight + "}" +
                        ":std{access=file,mux=mkv,dst='" + transcodedFilePath + "'}'";
                default:
                    return string.Empty;
            }
        }

        async void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (_mediaPlayer == null)
            {
                Hide();
                return;
            }

            await CleanupAndHide();
        }

        async Task CleanupAndHide()
        {
            if (_mediaPlayer == null)
            {
                Hide();
                return;
            }

            if (_mediaPlayer.isPlaying())
                _mediaPlayer.stop();

            await DispatchHelper.InvokeInUIThread(CoreDispatcherPriority.Normal, () =>
            {
                ProgressRing.IsActive = false;
                ProgressRing.Visibility = Visibility.Collapsed;
            });

            _mediaPlayer.eventManager().OnPositionChanged -= OnOnPositionChanged;
            _mediaPlayer.eventManager().OnEndReached -= OnOnEndReached;
            _mediaPlayer.eventManager().OnEncounteredError -= OnOnEncounteredError;

            Media.VlcMedia = null;
            _mediaPlayer = null;
            _instance = null;

            await DispatchHelper.InvokeInUIThread(CoreDispatcherPriority.Normal, Hide);
        }
    }
}