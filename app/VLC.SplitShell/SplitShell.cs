using System;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.ViewManagement;
using VLC.UI.Views.UserControls.Shell;
using VLC.UI.Views.UserControls;
using VLC.UI.Views;

namespace VLC.Controls
{
    public delegate void FlyoutCloseRequested(object sender, EventArgs e);
    public delegate void FlyoutNavigated(object sender, EventArgs p);
    public delegate void FlyoutClosed(object sender, EventArgs e);
    public delegate void ContentSizeChanged(double newWidth);
    
    [TemplatePart(Name = ContentPresenterName, Type = typeof(ContentPresenter))]
    [TemplatePart(Name = FlyoutContentPresenterName, Type = typeof(Frame))]
    [TemplatePart(Name = FlyoutFadeInName, Type = typeof(Storyboard))]
    [TemplatePart(Name = FlyoutFadeOutName, Type = typeof(Storyboard))]
    [TemplatePart(Name = FlyoutPlaneProjectionName, Type = typeof(PlaneProjection))]
    [TemplatePart(Name = FlyoutGridContainerName, Type = typeof(Grid))]
    [TemplatePart(Name = FlyoutBackgroundGridName, Type = typeof(Grid))]
    public sealed class SplitShell : Control
    {
        public event FlyoutCloseRequested FlyoutCloseRequested;
        public event FlyoutNavigated FlyoutNavigated;
        public event FlyoutClosed FlyoutClosed;
        public event ContentSizeChanged ContentSizeChanged;
        public TaskCompletionSource<bool> TemplateApplied = new TaskCompletionSource<bool>();
        
        private DispatcherTimer _windowResizerTimer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(200) };

        private const string PageName = "Page";
        private const string ContentPresenterName = "ContentPresenter";
        private const string FlyoutContentPresenterName = "FlyoutContentPresenter";
        private const string FlyoutFadeInName = "FlyoutFadeIn";
        private const string FlyoutFadeOutName = "FlyoutFadeOut";
        private const string TopBarFadeOutName = "TopBarFadeOut";
        private const string TopBarFadeInName = "TopBarFadeIn";
        private const string FlyoutPlaneProjectionName = "FlyoutPlaneProjection";
        private const string FlyoutGridContainerName = "FlyoutGridContainer";
        private const string FlyoutBackgroundGridName = "FlyoutBackgroundGrid";
        private const string BackdropGridName = "backDrop";

        private Page _page;
        private Grid _flyoutGridContainer;
        private Grid _flyoutBackgroundGrid;
        private ContentPresenter _contentPresenter;
        private Frame _flyoutContentPresenter;
        private BackDrop _backdrop;

        private PlaneProjection _flyoutPlaneProjection;
        private Storyboard _flyoutFadeIn;
        private Storyboard _flyoutFadeOut;
        private Storyboard _topBarFadeOut;
        private Storyboard _topBarFadeIn;

        public async void SetContentPresenter(object contentPresenter)
        {
            await TemplateApplied.Task;
            _contentPresenter.Content = contentPresenter;
        }
        
        public async Task SetFlyoutContentPresenter(object content)
        {
            await TemplateApplied.Task;
            _flyoutContentPresenter.Navigate((Type)content);
            ShowFlyout();
        }

        public async void SetFooterContentPresenter(object content)
        {
            await TemplateApplied.Task;
            _page.BottomAppBar = content as CommandBar;
            if (content == null)
                return;
        }

        public async void SetFooterVisibility(object visibility)
        {
            await TemplateApplied.Task;
            if (_page.BottomAppBar != null)
                _page.BottomAppBar.ClosedDisplayMode = (AppBarClosedDisplayMode)visibility;
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

        #region RFlyoutContent Property

        public Type FlyoutContent
        {
            get { return (Type)GetValue(FlyoutContentProperty); }
            set { SetValue(FlyoutContentProperty, value); }
        }

        public static readonly DependencyProperty FlyoutContentProperty = DependencyProperty.Register(
            nameof(FlyoutContent), typeof(Type), typeof(SplitShell),
            new PropertyMetadata(default(Type), FlyoutContentPropertyChangedCallback));

        private static async void FlyoutContentPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var that = (SplitShell)dependencyObject;
            await that.SetFlyoutContentPresenter(dependencyPropertyChangedEventArgs.NewValue);
            that.Responsive();
        }

        public bool FlyoutAsHeader
        {
            get { return (bool)GetValue(FlyoutAsHeaderProperty); }
            set { SetValue(FlyoutAsHeaderProperty, value); }
        }

        public static readonly DependencyProperty FlyoutAsHeaderProperty = DependencyProperty.Register(
            nameof(FlyoutAsHeader), typeof(bool), typeof(SplitShell),
            new PropertyMetadata(default(bool), FlyoutAsHeaderPropertyChangedCallback));

        private static void FlyoutAsHeaderPropertyChangedCallback(DependencyObject dO, DependencyPropertyChangedEventArgs args)
        {
            var that = (SplitShell)dO;
            that.Responsive();
        }
        #endregion

        #region FooterContent Property

        public AppBarClosedDisplayMode FooterVisibility
        {
            get { return (AppBarClosedDisplayMode)GetValue(FooterVisibilityProperty); }
            set { SetValue(FooterVisibilityProperty, value); }
        }

        public static readonly DependencyProperty FooterVisibilityProperty = DependencyProperty.Register(
            nameof(FooterVisibility), typeof(AppBarClosedDisplayMode), typeof(SplitShell), new PropertyMetadata(AppBarClosedDisplayMode.Compact, FooterVisibilityPropertyChangedCallback));

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
            nameof(FooterContent), typeof(DependencyObject), typeof(SplitShell),
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
            _page = (Page)GetTemplateChild(PageName);
            _contentPresenter = (ContentPresenter)GetTemplateChild(ContentPresenterName);
            _flyoutContentPresenter = (Frame)GetTemplateChild(FlyoutContentPresenterName);
            _flyoutFadeIn = (Storyboard)GetTemplateChild(FlyoutFadeInName);
            _flyoutFadeOut = (Storyboard)GetTemplateChild(FlyoutFadeOutName);
            _topBarFadeOut = (Storyboard)GetTemplateChild(TopBarFadeOutName);
            _topBarFadeIn = (Storyboard)GetTemplateChild(TopBarFadeInName);
            _flyoutPlaneProjection = (PlaneProjection)GetTemplateChild(FlyoutPlaneProjectionName);
            _flyoutGridContainer = (Grid)GetTemplateChild(FlyoutGridContainerName);
            _flyoutBackgroundGrid = (Grid)GetTemplateChild(FlyoutBackgroundGridName);
            _backdrop = (BackDrop)GetTemplateChild(BackdropGridName);

            Responsive();
            Window.Current.SizeChanged += Current_SizeChanged;
            _contentPresenter.Width = Window.Current.Bounds.Width;

            TemplateApplied.SetResult(true);

            _flyoutGridContainer.Visibility = Visibility.Collapsed;
            if (_flyoutBackgroundGrid != null)
                _flyoutBackgroundGrid.Tapped += FlyoutGridContainerOnTapped;

            _windowResizerTimer.Tick += _windowResizerTimer_Tick;

            _flyoutFadeOut.Completed += _flyoutFadeOut_Completed;
            _flyoutFadeIn.Completed += _flyoutFadeIn_Completed;

            _topBarFadeIn.Completed += _topBarFadeIn_Completed;
        }

        private void _flyoutFadeIn_Completed(object sender, object e)
        {
            // In case the flyout has been hidden in the meantime, stop now to
            // avoid an invalid state
            if (!IsFlyoutOpen)
                return;
            FlyoutNavigated?.Invoke(null, new EventArgs());
        }

        private void _flyoutFadeOut_Completed(object sender, object e)
        {
            // In case the flyout has been displayed back, stop now to
            // avoid an invalid state
            if (IsFlyoutOpen)
                return;
            _flyoutContentPresenter.Navigate(typeof(BlankPage));
        }
        
        private void Current_SizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            Responsive();
        }

        void Responsive()
        {
            var bottomBarHeight = (_page.BottomAppBar == null) ? 0 : _page.BottomAppBar.ActualHeight;
            var navBarHeight = ApplicationView.GetForCurrentView().VisibleBounds.Height - 16;

            if (FlyoutAsHeader)
            {
                _flyoutContentPresenter.VerticalAlignment = VerticalAlignment.Top;
                _flyoutContentPresenter.Height = double.NaN;
                _flyoutContentPresenter.Width = Window.Current.Bounds.Width;
            }
            else
            {
                _flyoutContentPresenter.VerticalAlignment = VerticalAlignment.Center;
                if (Window.Current.Bounds.Width < 650)
                {
                    _flyoutContentPresenter.Height = navBarHeight - bottomBarHeight;
                    _flyoutContentPresenter.Width = Window.Current.Bounds.Width;
                }
                else
                {
                    _flyoutContentPresenter.Width = 650;
                    _flyoutContentPresenter.Height = (navBarHeight < 900 * 0.7 ? navBarHeight : navBarHeight * 0.7) - bottomBarHeight;
                }
            }

            // Test if we have a specific property to fit the flyout height or not.
            if (IsCurrentFlyoutModal())
                _flyoutContentPresenter.Height = double.NaN;

            _windowResizerTimer.Stop();
            _windowResizerTimer.Start();
        }

        private void _windowResizerTimer_Tick(object sender, object e)
        {
            _contentPresenter.Width = Window.Current.Bounds.Width;
            _windowResizerTimer.Stop();
            ContentSizeChanged?.Invoke(_contentPresenter.Width);
        }

        private void FlyoutGridContainerOnTapped(object sender, TappedRoutedEventArgs tappedRoutedEventArgs)
        {
            if (IsCurrentFlyoutModal())
                return;
            FlyoutCloseRequested?.Invoke(null, new EventArgs());
        }

        void ShowFlyout()
        {
            if (IsFlyoutOpen)
                return;
            _flyoutFadeIn.Begin();
            IsFlyoutOpen = true;
            var mainControl = _contentPresenter.Content as Control;
            if (mainControl != null)
                mainControl.IsEnabled = false;
            _backdrop.Show(6);
        }

        public void HideFlyout()
        {
            if (!IsFlyoutOpen)
                return;
            _backdrop.Hide();
            _flyoutFadeOut.Begin();
            _flyoutContentPresenter.Navigate(typeof(BlankPage));
            var mainControl = _contentPresenter.Content as Control;
            if (mainControl != null)
                mainControl.IsEnabled = true;
            IsFlyoutOpen = false;
            FlyoutClosed?.Invoke(null, new EventArgs());
        }

        public void HideTopBar()
        {
            _topBarFadeOut.Begin();
            _contentPresenter.Margin = new Thickness(0, 0, 0, -_page.BottomAppBar?.ActualHeight ?? 0);
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

        public bool IsFlyoutOpen { get; private set; }
        public bool IsTopBarOpen { get; set; }

        public bool IsCurrentFlyoutModal()
        {
            var fo = _flyoutContentPresenter.Content as IVLCModalFlyout;
            if (fo == null)
                return false;
            return fo.ModalMode;
        }
    }
}
