using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using libVLCX;
using VLC.Commands;
using VLC.Helpers;
using VLC.Utils;
using VLC.ViewModels;

namespace VLC.Services.RunTime
{
    public class RendererService
    {
        public ObservableCollection<RendererItem> RendererItems { get; } = new ObservableCollection<RendererItem>();
        RendererDiscoverer _rendererDiscoverer;
        public bool IsStarted;

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
                Debug.Assert(_rendererDiscoverer.start());
            });
        }
        
        public void StopRendererDiscoverer()
        {
            if (!IsStarted) return;

            IsStarted = false;

            _rendererDiscoverer.stop();
            _rendererDiscoverer.eventManager().OnItemAdded -= OnOnItemAdded;
            _rendererDiscoverer.eventManager().OnRendererItemDeleted -= OnOnRendererItemDeleted;
            _rendererDiscoverer = null;

            RendererItems.Clear();
        }

        void OnOnRendererItemDeleted(RendererItem rendererItem)
        {
            RendererItems.Remove(rendererItem);
        }

        void OnOnItemAdded(RendererItem rendererItem)
        {
            RendererItems.Add(rendererItem);
            Debug.WriteLine("Found new rendererItem " + rendererItem.name() + " can render audio " + rendererItem.canRenderAudio() + " can render video " + rendererItem.canRenderVideo());
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
                    Command = new ActionCommand(() => Locator.PlaybackService.SetRenderer(ri.name()))

                });
            }

            return flyout;
        }
    }
}