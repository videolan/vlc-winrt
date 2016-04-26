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
    public delegate void RightSidebarNavigated(object sender, EventArgs p);
    public delegate void RightSidebarClosed(object sender, EventArgs e);
    public delegate void ContentSizeChanged(double newWidth);
    
    [TemplatePart(Name = ContentPresenterName, Type = typeof(ContentPresenter))]
    [TemplatePart(Name = RightFlyoutContentPresenterName, Type = typeof(ContentPresenter))]
    [TemplatePart(Name = RightFlyoutFadeInName, Type = typeof(Storyboard))]
    [TemplatePart(Name = RightFlyoutFadeOutName, Type = typeof(Storyboard))]
    [TemplatePart(Name = RightFlyoutPlaneProjectionName, Type = typeof(PlaneProjection))]
    [TemplatePart(Name = RightFlyoutGridContainerName, Type = typeof(Grid))]
    [TemplatePart(Name = FlyoutBackgroundGridName, Type = typeof(Grid))]
    [TemplatePart(Name = FooterContentPresenterName, Type = typeof(ContentPresenter))]
    public sealed class SplitShell : Control
    {
        public event FlyoutCloseRequested FlyoutCloseRequested;
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

        private Grid _rightFlyoutGridContainer;
        private Grid _flyoutBackgroundGrid;
        private ContentPresenter _contentPresenter;
        private ContentPresenter _rightFlyoutContentPresenter;
        private ContentPresenter _footerContentPresenter;

        private PlaneProjection _rightFlyoutPlaneProjection;
        private Storyboard _rightFlyoutFadeIn;
        private Storyboard _rightFlyoutFadeOut;
        private Storyboard _topBarFadeOut;
        private Storyboard _topBarFadeIn;

        public async void SetContentPresenter(object contentPresenter)
        {
            await TemplateApplied.Task;
            _contentPresenter.Content = contentPresenter;
        }
        
        public async void SetRightPaneContentPresenter(object content)
        {
            await TemplateApplied.Task;
            _rightFlyoutContentPresenter.Content = content;
            ShowFlyout();
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

            Responsive();
            Window.Current.SizeChanged += Current_SizeChanged;
            _contentPresenter.Width = Window.Current.Bounds.Width;

            TemplateApplied.SetResult(true);

            _rightFlyoutGridContainer.Visibility = Visibility.Collapsed;
            if (_flyoutBackgroundGrid != null)
            _flyoutBackgroundGrid.Tapped += RightFlyoutGridContainerOnTapped;

            _windowResizerTimer.Tick += _windowResizerTimer_Tick;

            _rightFlyoutFadeOut.Completed += _rightFlyoutFadeOut_Completed;
            _rightFlyoutFadeIn.Completed += _rightFlyoutFadeIn_Completed;

            _topBarFadeIn.Completed += _topBarFadeIn_Completed;
        }

        private void _rightFlyoutFadeIn_Completed(object sender, object e)
        {
            RightSidebarNavigated?.Invoke(null, new EventArgs());
        }

        private void _rightFlyoutFadeOut_Completed(object sender, object e)
        {
            _rightFlyoutContentPresenter.Content = null;
        }
        
        private void Current_SizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            Responsive();
        }

        void Responsive()
        {
            if (Window.Current.Bounds.Width < 650)
            {
                _rightFlyoutContentPresenter.Height = Window.Current.Bounds.Height;
                _rightFlyoutContentPresenter.Width = Window.Current.Bounds.Width;
            }
            else
            {
                _rightFlyoutContentPresenter.Width = 650;
                _rightFlyoutContentPresenter.Height = Window.Current.Bounds.Height < 800 * 0.9 ? Window.Current.Bounds.Height : Window.Current.Bounds.Height * 0.9;
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
            _contentPresenter.Margin = new Thickness(0, 0, 0, - _footerContentPresenter.ActualHeight);
            IsTopBarOpen = false;
        }

        public void ShowTopBar()
        {
            _topBarFadeIn.Begin();
            IsTopBarOpen = true;
        }
        
        private void _topBarFadeIn_Completed(object sender, object e)
        {
            _contentPresenter.Margin = new Thickness(0);
        }

        public bool IsRightFlyoutOpen { get; private set; }
        public bool IsTopBarOpen { get; set; }
    }
}
