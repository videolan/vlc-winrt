/*
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"
*/

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;
using VLC_WINRT.Utility.Services.Interface;
using VLC_WINRT.Utility.Services.RunTime;
using VLC_WINRT.ViewModels.MainPage;
using VLC_WINRT.ViewModels.PlayVideo;

namespace VLC_WINRT.ViewModels
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        static ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            if (ViewModelBase.IsInDesignModeStatic)
            {
                // Create design time view services and models
                SimpleIoc.Default.Register<IThumbnailService, Utility.Services.DesignTime.ThumbnailService>();
            
            }
            else
            {
                // Create run time view services and models
                SimpleIoc.Default.Register<IThumbnailService, Utility.Services.RunTime.ThumbnailService>();
            }

            SimpleIoc.Default.Register<HistoryService>();
            SimpleIoc.Default.Register<MouseService>();

            SimpleIoc.Default.Register<PlayVideoViewModel>(true);
            SimpleIoc.Default.Register<MainPageViewModel>();
            SimpleIoc.Default.Register<ThumbnailsViewModel>();
            SimpleIoc.Default.Register<LibraryViewModel>();
        }

        public static ThumbnailsViewModel ThumbnailsVM
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ThumbnailsViewModel>();
            }
        }

        public static PlayVideoViewModel PlayVideoVM
        {
            get
            {
                return ServiceLocator.Current.GetInstance<PlayVideoViewModel>();
            }
        }

        public static MainPageViewModel MainPageVM
        {
            get
            {
                return ServiceLocator.Current.GetInstance<MainPageViewModel>();
            }
        }

        public static LibraryViewModel LibraryVM
        {
            get
            {
                return ServiceLocator.Current.GetInstance<LibraryViewModel>();
            }
        }
    }
}