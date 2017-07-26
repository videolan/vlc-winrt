using Windows.Foundation.Metadata;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using VLC.Helpers;

namespace VLC.UI.Views.VideoPages.VideoNowPlayingControls
{
    public sealed partial class VideoPlayerOptionsPanel
    {
        public VideoPlayerOptionsPanel()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (DeviceHelper.GetDeviceType() == DeviceTypeEnum.Xbox &&
                ApiInformation.IsPropertyPresent("Windows.UI.Xaml.Controls.Control", nameof(XYFocusDown)))
            {
                ZoomCombobox.XYFocusDown = SpeedResetButton;
                SpeedRateSlider.XYFocusDown = AudioDelayResetButton;
                SpeedRateSlider.XYFocusUp = SpeedResetButton;
                AudioDelaySlider.XYFocusDown = SubtitleDelayResetButton;
                AudioDelaySlider.XYFocusUp = AudioDelayResetButton;
                SpuDelaySlider.XYFocusUp = SubtitleDelayResetButton;
            }
        }
    }
}
