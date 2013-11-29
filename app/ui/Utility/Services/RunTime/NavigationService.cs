using Windows.UI.Xaml;
using VLC_WINRT.Views;

namespace VLC_WINRT.Utility.Services.RunTime
{
    public class NavigationService
    {
        public static void NavigateTo(BasePage page)
        {
            var oldPage = Window.Current.Content as BasePage;
            if (oldPage != null)
            {
                oldPage.NavigateFrom();
            }

            Window.Current.Content = page;
            page.NavigateTo();
        }
    }
}