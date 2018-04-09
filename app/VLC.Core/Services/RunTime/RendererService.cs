using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using libVLCX;
using VLC.Commands;
using VLC.Helpers;
using VLC.Utils;
using VLC.ViewModels;
using WinRTXamlToolkit.Tools;

namespace VLC.Services.RunTime
{
    public class RendererService : BindableBase
    {
        public ObservableCollection<RendererItem> RendererItems { get; } = new ObservableCollection<RendererItem>();
        RendererDiscoverer _rendererDiscoverer;
        public bool IsStarted;
        public bool IsRendererSet { get; set; }

        readonly MenuFlyoutItem _disconnect = new MenuFlyoutItem
        {
            Text = Strings.Disconnect,
            Command = new ActionCommand(() =>
            {
                Locator.PlaybackService.DisconnectRenderer();
                Locator.RendererService.IsRendererSet = false;
            })
        };

        public void Start()
        {
            if (IsStarted) return;

            IsStarted = true;

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
        
        public void Stop()
        {
            if (!IsStarted) return;

            IsStarted = false;

            _rendererDiscoverer.stop();
            _rendererDiscoverer.eventManager().OnItemAdded -= OnOnItemAdded;
            _rendererDiscoverer.eventManager().OnRendererItemDeleted -= OnOnRendererItemDeleted;
            _rendererDiscoverer = null;

            RendererItems.Clear();
        }

        async void OnOnRendererItemDeleted(RendererItem rendererItem)
        {
            var match = RendererItems.FirstOrDefault(item => item.name().Equals(rendererItem.name()));
            if (match != null)
                RendererItems.Remove(match);

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
    }
}