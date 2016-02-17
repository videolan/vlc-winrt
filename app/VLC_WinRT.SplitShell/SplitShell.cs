using System;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using VLC_WinRT.Helpers;

namespace VLC_WinRT.Controls
{
    public delegate void FlyoutCloseRequested(object sender, EventArgs e);
    public delegate void LeftSidebarClosed(object sender, EventArgs e);
    public delegate void RightSidebarNavigated(object sender, EventArgs p);
    public delegate void RightSidebarClosed(object sender, EventArgs e);
    public delegate void ContentSizeChanged(double newWidth);
    
    [TemplatePart(Name = ContentPresenterName, Type = typeof(ContentPresenter))]
    [TemplatePart(Name = RightFlyoutContentPresenterName, Type = typeof(ContentPresenter))]
    [TemplatePart(Name = RightFlyoutFadeInName, Type = typeof(Storyboard))]
    [TemplatePart(Name = RightFlyoutFadeOutName, Type = typeof(Storyboard))]
    [TemplatePart(Name = SidePaneOpeningName, Type = typeof(Storyboard))]
    [TemplatePart(Name = SidePaneClosingName, Type = typeof(Storyboard))]
    [TemplatePart(Name = RightFlyoutPlaneProjectionName, Type = typeof(PlaneProjection))]
    [TemplatePart(Name = RightFlyoutGridContainerName, Type = typeof(Grid))]
    [TemplatePart(Name = FlyoutBackgroundGridName, Type = typeof(Grid))]
    [TemplatePart(Name = FooterContentPresenterName, Type = typeof(ContentPresenter))]
    [TemplatePart(Name = SplitPaneContentPresenterName, Type = typeof(ContentPresenter))]
    [TemplatePart(Name = SplitPaneEmptyGridName, Type = typeof(Grid))]
    [TemplatePart(Name = SplitPaneOpenerGridName, Type = typeof(Grid))]
    public sealed class SplitShell : Control
    {
        public event FlyoutCloseRequested FlyoutCloseRequested;
        public event LeftSidebarClosed LeftSidebarClosed;
        public event RightSidebarNavigated RightSidebarNavigated;
        public event RightSidebarClosed RightSidebarClosed;
        public event ContentSizeChanged ContentSizeChanged;
        public TaskCompletionSource<bool> TemplateApplied = new TaskCompletionSource<bool>();
        
        private DispatcherTimer _windowResizerTimer = new DispatcherTimer()
        {
            Interval = TimeSpan.FromMilliseconds(200)
        };

        private const string ContentPresenterName = "ContentPresenter";
        private const string RightFlyoutContentPresenterName = "RightFlyoutContentPresenter";
        private const string RightFlyoutFadeInName = "RightFlyoutFadeIn";
        private const string RightFlyoutFadeOutName = "RightFlyoutFadeOut";
        private const string TopBarFadeOutName = "TopBarFadeOut";
        private const string TopBarFadeInName = "TopBarFadeIn";
        private const string RightFlyoutPlaneProjectionName = "RightFlyoutPlaneProjection";
        private const string RightFlyoutGridContainerName = "RightFlyoutGridContainer";
        private const string FlyoutBackgroundGridName = "FlyoutBackgroundGrid";
        private const string FooterContentPresenterName = "FooterContentPresenter";
        private const string SplitPaneContentPresenterName = "SplitPaneContentPresenter";
        private const string SidePaneOpeningName = "SidePaneOpening";
        private const string SidePaneClosingName = "SidePaneClosing";
        private const string SplitPaneEmptyGridName = "SplitPaneEmptyGrid";
        private const string SplitPaneOpenerGridName = "SplitPaneOpenerGrid";

        private Grid _rightFlyoutGridContainer;
        private Grid _flyoutBackgroundGrid;
        private Grid _splitPaneEmptyGrid;
        private Grid _splitPaneOpenerGrid;
        private ContentPresenter _contentPresenter;
        private ContentPresenter _rightFlyoutContentPresenter;
        private ContentPresenter _footerContentPresenter;
        private ContentPresenter _splitPaneContentPresenter;

        private PlaneProjection _rightFlyoutPlaneProjection;
        private Storyboard _rightFlyoutFadeIn;
        private Storyboard _rightFlyoutFadeOut;
        private Storyboard _topBarFadeOut;
        private Storyboard _topBarFadeIn;
        private Storyboard _sidePaneOpening;
        private Storyboard _sidePaneClosing;

        public async void SetContentPresenter(object contentPresenter)
        {
            await TemplateApplied.Task;
            _contentPresenter.Content = contentPresenter;
        }

        public async void SetSplitPaneContentPresenter(object contentPresenter)
        {
            await TemplateApplied.Task;
            _splitPaneContentPresenter.Content = contentPresenter;
        }
        
        public async void SetRightPaneContentPresenter(object content)
        {
            await TemplateApplied.Task;
            _rightFlyoutContentPresenter.Content = content;
            ShowFlyout();
            RightSidebarNavigated?.Invoke(null, new EventArgs());
        }

        public async void SetFooterContentPresenter(object content)
        {
            await TemplateApplied.Task;
            _footerContentPresenter.Content = content;
        }
        public async void SetFooterVisibility(object visibility)
        {
            await TemplateApplied.Task;
            _footerContentPresenter.Visibility = (Visibility)visibility;
        }

        #region Content Property
        public DependencyObject Content
        {
            get { return (DependencyObject)GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        public static readonly DependencyProperty ContentProperty = DependencyProperty.Register(
            "Content", typeof(DependencyObject), typeof(SplitShell), new PropertyMetadata(default(DependencyObject), ContentPropertyChangedCallback));


        private static void ContentPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var that = (SplitShell)dependencyObject;
            that.SetContentPresenter(dependencyPropertyChangedEventArgs.NewValue);
        }
        #endregion

        #region RightPaneContent Property

        public DependencyObject RightFlyoutContent
        {
            get { return (DependencyObject)GetValue(RightFlyoutContentProperty); }
            set { SetValue(RightFlyoutContentProperty, value); }
        }

        public static readonly DependencyProperty RightFlyoutContentProperty = DependencyProperty.Register(
            nameof(RightFlyoutContent), typeof(DependencyObject), typeof(SplitShell),
            new PropertyMetadata(default(DependencyObject), RightFlyoutContentPropertyChangedCallback));

        private static void RightFlyoutContentPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var that = (SplitShell)dependencyObject;
            that.SetRightPaneContentPresenter(dependencyPropertyChangedEventArgs.NewValue);
        }
        #endregion

        #region FooterContent Property

        public Visibility FooterVisibility
        {
            get { return (Visibility)GetValue(FooterVisibilityProperty); }
            set { SetValue(FooterVisibilityProperty, value); }
        }

        public static readonly DependencyProperty FooterVisibilityProperty = DependencyProperty.Register(
            "FooterVisibility", typeof(Visibility), typeof(SplitShell), new PropertyMetadata(Visibility.Visible, FooterVisibilityPropertyChangedCallback));

        private static void FooterVisibilityPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var that = (SplitShell)dependencyObject;
            that.SetFooterVisibility(dependencyPropertyChangedEventArgs.NewValue);
        }

        public DependencyObject FooterContent
        {
            get { return (DependencyObject)GetValue(FooterContentProperty); }
            set { SetValue(FooterContentProperty, value); }
        }

        public static readonly DependencyProperty FooterContentProperty = DependencyProperty.Register(
            "FooterContent", typeof(DependencyObject), typeof(SplitShell),
            new PropertyMetadata(default(DependencyObject), FooterContentPropertyChangedCallback));

        private static void FooterContentPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var that = (SplitShell)dependencyObject;
            that.SetFooterContentPresenter(dependencyPropertyChangedEventArgs.NewValue);
        }

        public DependencyObject SplitPaneContent
        {
            get { return (DependencyObject)GetValue(SplitPaneContentProperty); }
            set { SetValue(SplitPaneContentProperty, value); }
        }

        public static readonly DependencyProperty SplitPaneContentProperty = DependencyProperty.Register("SplitPaneContent", typeof(DependencyObject), typeof(SplitShell), new PropertyMetadata(default(DependencyObject), SplitPaneContentPropertyChangedCallback));

        private static void SplitPaneContentPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var that = (SplitShell)dependencyObject;
            that.SetSplitPaneContentPresenter(dependencyPropertyChangedEventArgs.NewValue);
        }
        #endregion
        
        public SplitShell()
        {
            DefaultStyleKey = typeof(SplitShell);
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _contentPresenter = (ContentPresenter)GetTemplateChild(ContentPresenterName);
            _rightFlyoutContentPresenter = (ContentPresenter)GetTemplateChild(RightFlyoutContentPresenterName);
            _rightFlyoutFadeIn = (Storyboard)GetTemplateChild(RightFlyoutFadeInName);
            _rightFlyoutFadeOut = (Storyboard)GetTemplateChild(RightFlyoutFadeOutName);
            _topBarFadeOut = (Storyboard)GetTemplateChild(TopBarFadeOutName);
            _topBarFadeIn = (Storyboard)GetTemplateChild(TopBarFadeInName);
            _rightFlyoutPlaneProjection = (PlaneProjection)GetTemplateChild(RightFlyoutPlaneProjectionName);
            _rightFlyoutGridContainer = (Grid)GetTemplateChild(RightFlyoutGridContainerName);
            _flyoutBackgroundGrid = (Grid)GetTemplateChild(FlyoutBackgroundGridName);
            _footerContentPresenter = (ContentPresenter)GetTemplateChild(FooterContentPresenterName);
            _splitPaneContentPresenter = (ContentPresenter)GetTemplateChild(SplitPaneContentPresenterName);
            _sidePaneOpening = (Storyboard)GetTemplateChild(SidePaneOpeningName);
            _sidePaneClosing = (Storyboard)GetTemplateChild(SidePaneClosingName);
            _splitPaneEmptyGrid = (Grid)GetTemplateChild(SplitPaneEmptyGridName);
            _splitPaneOpenerGrid = (Grid)GetTemplateChild(SplitPaneOpenerGridName);

            Responsive();
            Window.Current.SizeChanged += Current_SizeChanged;
            _contentPresenter.Width = Window.Current.Bounds.Width;

            TemplateApplied.SetResult(true);

            _rightFlyoutGridContainer.Visibility = Visibility.Collapsed;
            if (_flyoutBackgroundGrid != null)
            _flyoutBackgroundGrid.Tapped += RightFlyoutGridContainerOnTapped;
            _splitPaneEmptyGrid.Tapped += _splitPaneEmptyGrid_Tapped;

            _splitPaneOpenerGrid.ManipulationMode = ManipulationModes.TranslateX;
            _splitPaneOpenerGrid.ManipulationDelta += _splitPaneOpenerGrid_ManipulationDelta;

            _windowResizerTimer.Tick += _windowResizerTimer_Tick;
        }

        private void _splitPaneOpenerGrid_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (e.Cumulative.Translation.X > 50)
            {
                if (!IsLeftPaneOpen)
                {
                    OpenLeftPane();
                    e.Complete();
                }
            }
        }

        private void Current_SizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            Responsive();
        }

        void Responsive()
        {
            if (Window.Current.Bounds.Width < 700)
            {
                _rightFlyoutContentPresenter.Height = Window.Current.Bounds.Height;
                _rightFlyoutContentPresenter.Width = Window.Current.Bounds.Width;
            }
            else
            {
                _rightFlyoutContentPresenter.Width = 700;
                _rightFlyoutContentPresenter.Height = Window.Current.Bounds.Height < 800 ? Window.Current.Bounds.Height : Window.Current.Bounds.Height * 0.8;
            }
            _windowResizerTimer.Stop();
            _windowResizerTimer.Start();
        }

        private void _windowResizerTimer_Tick(object sender, object e)
        {
            _contentPresenter.Width = Window.Current.Bounds.Width;
            _windowResizerTimer.Stop();
            ContentSizeChanged?.Invoke(_contentPresenter.Width);
        }

        private void RightFlyoutGridContainerOnTapped(object sender, TappedRoutedEventArgs tappedRoutedEventArgs)
        {
            FlyoutCloseRequested?.Invoke(null, new EventArgs());
        }

        private void _splitPaneEmptyGrid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            CloseLeftPane();
        }

        void ShowFlyout()
        {
            _rightFlyoutFadeIn.Begin();
            IsRightFlyoutOpen = true;
        }

        public void HideFlyout()
        {
            _rightFlyoutFadeOut.Begin();
            IsRightFlyoutOpen = false;
            RightSidebarClosed?.Invoke(null, new EventArgs());
        }

        public void HideTopBar()
        {
            _topBarFadeOut.Begin();
            IsTopBarOpen = false;
        }

        public void ShowTopBar()
        {
            _topBarFadeIn.Begin();
            IsTopBarOpen = true;
        }

        public void OpenLeftPane()
        {
            _sidePaneOpening.Begin();
            IsLeftPaneOpen = true;
        }

        public void CloseLeftPane()
        {
            _sidePaneClosing.Begin();
            IsLeftPaneOpen = false;
            LeftSidebarClosed?.Invoke(null, new EventArgs());
        }

        public void ToggleLeftPane()
        {
            if (IsLeftPaneOpen)
                CloseLeftPane();
            else OpenLeftPane();
        }

        public bool IsLeftPaneOpen { get; private set; }
        public bool IsRightFlyoutOpen { get; private set; }
        public bool IsTopBarOpen { get; set; }
    }
}
