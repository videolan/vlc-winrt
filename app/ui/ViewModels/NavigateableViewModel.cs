using VLC_WINRT.Common;

namespace VLC_WINRT.ViewModels
{
    public class NavigateableViewModel : BindableBase
    {
        public virtual void OnNavigatedTo(){}
        public virtual void OnNavigatedFrom(){}
    }
}