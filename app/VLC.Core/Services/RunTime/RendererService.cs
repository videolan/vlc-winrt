using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Autofac;
using libVLCX;
using VLC.Commands;
using VLC.Helpers;
using VLC.Model.Events;
using VLC.Utils;
using VLC.ViewModels;

namespace VLC.Services.RunTime
{
    public class RendererService : BindableBase
    {
        public ObservableCollection<RendererItem> RendererItems { get; } = new ObservableCollection<RendererItem>();
        RendererDiscoverer _rendererDiscoverer;
        public bool IsStarted;
        public bool IsRendererSet { get; set; }
        readonly PlaybackService _playbackService = Locator.PlaybackService;

        public RendererService()
        {
            App.Container.Resolve<NetworkListenerService>().InternetConnectionChanged += OnInternetConnectionChanged;
        }

        async void OnInternetConnectionChanged(object sender, InternetConnectionChangedEventArgs internetConnectionChangedEventArgs)
        {
            if (!internetConnectionChangedEventArgs.IsConnected && IsStarted)
                await Stop();
            else if(internetConnectionChangedEventArgs.IsConnected && !IsStarted)
                Start();
        }

        readonly MenuFlyoutItem _disconnect = new MenuFlyoutItem
        {
            Text = Strings.Disconnect,
            Command = new ActionCommand(() => Locator.RendererService.DisconnectRenderer())
        };

        public void Start()
        {
            if (IsStarted || !NetworkListenerService.IsConnected) return;

            IsStarted = true;
            IsRendererSet = false;

            Task.Run(async () =>
            {
                await Locator.PlaybackService.Initialize();

                if(_rendererDiscoverer == null)
                {
                    _rendererDiscoverer = new RendererDiscoverer(Locator.PlaybackService.Instance,
                    Locator.PlaybackService.Instance.rendererDiscoverers().First().name());
                }

                _rendererDiscoverer.eventManager().OnItemAdded += OnOnItemAdded;
                _rendererDiscoverer.eventManager().OnRendererItemDeleted += OnOnRendererItemDeleted;
                _rendererDiscoverer.start();
            });
        }
        
        public async Task Stop()
        {
            if (!IsStarted) return;

            IsStarted = false;

            DisconnectRenderer();

            _rendererDiscoverer.stop();
            _rendererDiscoverer.eventManager().OnItemAdded -= OnOnItemAdded;
            _rendererDiscoverer.eventManager().OnRendererItemDeleted -= OnOnRendererItemDeleted;
            _rendererDiscoverer = null;

            RendererItems.Clear();
            await DispatchHelper.InvokeInUIThread(CoreDispatcherPriority.Normal, () => OnPropertyChanged(nameof(HasRenderer)));
        }

        async void OnOnRendererItemDeleted(RendererItem rendererItem)
        {
            var match = RendererItems.FirstOrDefault(item => item.name().Equals(rendererItem.name()));
            if (match != null)
                RendererItems.Remove(match);

            if (IsRendererSet && !RendererItems.Any())
                DisconnectRenderer();

            await DispatchHelper.InvokeInUIThread(CoreDispatcherPriority.Normal, () => OnPropertyChanged(nameof(HasRenderer)));
        }

        async void OnOnItemAdded(RendererItem rendererItem)
        {
            LogHelper.Log("Found new rendererItem " + rendererItem.name() + 
                          " can render audio " + rendererItem.canRenderAudio() +
                          " can render video " + rendererItem.canRenderVideo());

            RendererItems.Add(rendererItem);
            await DispatchHelper.InvokeInUIThread(CoreDispatcherPriority.Normal, () => OnPropertyChanged(nameof(HasRenderer)));
        }

        public Visibility HasRenderer => DeviceHelper.GetDeviceType() != DeviceTypeEnum.Xbox && RendererItems.Any()
            ? Visibility.Visible : Visibility.Collapsed;
       
        public MenuFlyout CreateRendererFlyout()
        {
            var flyout = new MenuFlyout();
            foreach (var ri in RendererItems)
            {
                flyout.Items.Add(new MenuFlyoutItem
                {
                    Text = ri.name(),
                    Command = new ActionCommand(() =>
                    {
                        Locator.PlaybackService.SetRenderer(ri.name());
                        Locator.RendererService.IsRendererSet = true;
                    })
                });
            }
            flyout.Items.Add(_disconnect);
            return flyout;
        }

        public void DisconnectRenderer()
        {
            _playbackService.DisconnectRenderer();
            IsRendererSet = false;
        }
    }
}