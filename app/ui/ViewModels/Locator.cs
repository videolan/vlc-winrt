/*
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"
*/

using VLC_WINRT.Utility.IoC;
using VLC_WINRT.Utility.Services.Interface;
using VLC_WINRT.Utility.Services.RunTime;
using VLC_WINRT.ViewModels.MainPage;
using VLC_WINRT.ViewModels.MainPage.PlayMusic;
using VLC_WINRT.ViewModels.PlayMusic;
using VLC_WINRT.ViewModels.PlayVideo;

namespace VLC_WINRT.ViewModels
{
    /// <summary>
    ///     This class contains static references to all the view models in the
    ///     application and provides an entry point for the bindings.
    /// </summary>
    public class Locator
    {
        /// <summary>
        ///     Initializes a new instance of the Locator class.
        /// </summary>
        static Locator()
        {
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                // Create design time view services and models
                IoC.Register<IThumbnailService, Utility.Services.DesignTime.ThumbnailService>();
            }
            else
            {
                // Create run time view services and models
                IoC.Register<IThumbnailService, Utility.Services.RunTime.ThumbnailService>();
            }
           

            IoC.Register<HistoryService>();
            IoC.Register<MouseService>();
            IoC.Register<MediaPlayerService>();
            IoC.Register<ExternalDeviceService>(true);

            IoC.Register<PlayVideoViewModel>(true);
            IoC.Register<MainPageViewModel>();
            IoC.Register<MusicLibraryViewModel>();
            IoC.Register<MusicPlayerViewModel>();
        }
        public static MusicPlayerViewModel MusicPlayerVM
        {
            get { return IoC.GetInstance<MusicPlayerViewModel>(); }
        }
        public static MusicLibraryViewModel MusicLibraryVM
        {
            get { return IoC.GetInstance<MusicLibraryViewModel>(); }
        }

        public static PlayVideoViewModel PlayVideoVM
        {
            get { return IoC.GetInstance<PlayVideoViewModel>(); }
        }

        public static MainPageViewModel MainPageVM
        {
            get { return IoC.GetInstance<MainPageViewModel>(); }
        }
    }
}