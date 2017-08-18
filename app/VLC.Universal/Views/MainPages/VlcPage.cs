using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Autofac;
using VLC.ViewModels;

namespace VLC.UI.Views.MainPages
{
    public abstract class VlcPage<TViewModelBase> : Page where TViewModelBase : ViewModelBase
    {
        private TViewModelBase _viewModel;
        protected virtual TViewModelBase ViewModel => _viewModel ?? (_viewModel = App.Container.Resolve<TViewModelBase>());

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ViewModel.Initialize();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            ViewModel.Stop();
            _viewModel = null;
        }
    }
}