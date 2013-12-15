using System;
using Windows.UI.Xaml;
using VLC_WINRT.Views;

namespace VLC_WINRT.Utility.Services.RunTime
{
    public class NavigationService
    {
        public static void NavigateTo(Type page)
        {
            ((RootPage)Window.Current.Content).MainFrame.Navigate(page);
        }
    }
}