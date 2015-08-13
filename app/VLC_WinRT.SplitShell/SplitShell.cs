using System;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace VLC_WinRT.Controls
{
    public delegate void FlyoutCloseRequested(object sender, EventArgs e);

    [TemplatePart(Name = TitleBarContentPresenterName, Type = typeof(ContentPresenter))]
    [TemplatePart(Name = TopBarContentPresenterName, Type = typeof(ContentPresenter))]
    [TemplatePart(Name = InformationContentPresenterName, Type = typeof(ContentPresenter))]
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
        public TaskCompletionSource<bool> TemplateApplied = new TaskCompletionSource<bool>();
        
        private const string ContentPresenterName = "ContentPresenter";
        private const string TopBarContentPresenterName = "TopBarContentPresenter";
        private const string TitleBarContentPresenterName = "TitleBarContentPresenter";
        private const string InformationContentPresenterName = "InformationContentPresenter";
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
        private ContentPresenter _topBarContentPresenter;
        private ContentPresenter _titleBarContentPresenter;
        private ContentPresenter _rightFlyoutContentPresenter;
        private ContentPresenter _footerContentPresenter;

        private PlaneProjection _rightFlyoutPlaneProjection;
        private Storyboard _rightFlyoutFadeIn;
        private Storyboard _rightFlyoutFadeOut;
        private Storyboard _topBarFadeOut;
        private Storyboard _topBarFadeIn;
        private ContentPresenter _informationGrid;
        private TextBlock _informationTextBlock;
        
        public async void SetContentPresenter(object contentPresenter)
        {
            await TemplateApplied.Task;
            _contentPresenter.Content = contentPresenter;
        }

        public async void SetTitleBarContentPresenter(object contentPresenter)
        {
            await TemplateApplied.Task;
            _titleBarContentPresenter.Content = contentPresenter;
        }

        public async void SetTitleBarHeight(double h)
        {
            await TemplateApplied.Task;
            if (h < 0)
                h = 0;
            _titleBarContentPresenter.Height = h;
        }

        public async void SetTopbarContentPresenter(object contentPresenter)
        {
            await TemplateApplied.Task;
            _topBarContentPresenter.Content = contentPresenter;
        }

        public async void SetTopbarVisibility(object visibility)
        {
            await TemplateApplied.Task;
            _topBarContentPresenter.Visibility = (Visibility) visibility;
        }
        

        public async void SetInformationContent(object contentPresenter)
        {
            await TemplateApplied.Task;
            _informationGrid.Content = contentPresenter;
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

        #region TopBarContent Property
        public Visibility TopBarVisibility
        {
            get { return (Visibility)GetValue(TopBarVisibilityProperty); }
            set { SetValue(TopBarVisibilityProperty, value); }
        }
        
        public static readonly DependencyProperty TopBarVisibilityProperty = DependencyProperty.Register(
            "TopBarVisibility", typeof(Visibility), typeof(SplitShell), new PropertyMetadata(Visibility.Visible, TopbarVisibilityPropertyChangedCallback));

        private static void TopbarVisibilityPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var that = (SplitShell) dependencyObject;
            that.SetTopbarVisibility(dependencyPropertyChangedEventArgs.NewValue);
        }

        public DependencyObject TopBarContent
        {
            get { return (DependencyObject)GetValue(TopBarContentProperty); }
            set { SetValue(TopBarContentProperty, value); }
        }

        public static readonly DependencyProperty TopBarContentProperty = DependencyProperty.Register(
            "TopBarContent", typeof(DependencyObject), typeof(SplitShell), new PropertyMetadata(default(DependencyObject), TopBarContentPropertyChangedCallback));


        private static void TopBarContentPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var that = (SplitShell)dependencyObject;
            that.SetTopbarContentPresenter(dependencyPropertyChangedEventArgs.NewValue);
        }
        #endregion

        #region RightPaneContent Property

        public DependencyObject RightFlyoutContent
        {
            get { return (DependencyObject)GetValue(RightFlyoutContentProperty); }
            set { SetValue(RightFlyoutContentProperty, value); }
        }

        public static readonly DependencyProperty RightFlyoutContentProperty = DependencyProperty.Register(
            "RightFlyoutContent", typeof(DependencyObject), typeof(SplitShell),
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

        #region InformationContent Property

        public DependencyObject InformationText
        {
            get { return (DependencyObject)GetValue(InformationTextProperty); }
            set { SetValue(InformationTextProperty, value); }
        }

        public static readonly DependencyProperty InformationTextProperty = DependencyProperty.Register("InformationText", typeof(DependencyObject), typeof(SplitShell), new PropertyMetadata(default(DependencyObject), InformationContentPresenterPropertyChangedCallback));

        private static void InformationContentPresenterPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var that = (SplitShell)dependencyObject;
            that.SetInformationContent(dependencyPropertyChangedEventArgs.NewValue);
        }
        #endregion

        #region TitleBar Property

        public DependencyObject TitleBarContent
        {
            get { return (DependencyObject)GetValue(TitleBarContentProperty); }
            set { SetValue(TitleBarContentProperty, value); }
        }

        public static readonly DependencyProperty TitleBarContentProperty = DependencyProperty.Register(
            "TitleBarContent", typeof(DependencyObject), typeof(SplitShell), new PropertyMetadata(default(DependencyObject), TitleBarContentPropertyChangedCallback));


        private static void TitleBarContentPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
#if WINDOWS_PHONE_APP
            return;
#endif
            var that = (SplitShell)dependencyObject;
            that.SetTitleBarContentPresenter(dependencyPropertyChangedEventArgs.NewValue);
        }



        public double TitleBarHeight
        {
            get { return (int)GetValue(TitleBarHeightProperty); }
            set { SetValue(TitleBarHeightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TitleBarHeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleBarHeightProperty =
            DependencyProperty.Register("TitleBarHeight", typeof(int), typeof(SplitShell), new PropertyMetadata(32, TitleBarHeightPropertyChangedCallback));

        private static void TitleBarHeightPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
#if WINDOWS_PHONE_APP
            return;
#endif
            var that = (SplitShell)dependencyObject;
            that.SetTitleBarHeight((double)dependencyPropertyChangedEventArgs.NewValue);
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
            _topBarContentPresenter = (ContentPresenter)GetTemplateChild(TopBarContentPresenterName);
            _informationGrid = (ContentPresenter)GetTemplateChild(InformationContentPresenterName);
            _rightFlyoutContentPresenter = (ContentPresenter)GetTemplateChild(RightFlyoutContentPresenterName);
            _rightFlyoutFadeIn = (Storyboard)GetTemplateChild(RightFlyoutFadeInName);
            _rightFlyoutFadeOut = (Storyboard)GetTemplateChild(RightFlyoutFadeOutName);
            _topBarFadeOut = (Storyboard) GetTemplateChild(TopBarFadeOutName);
            _topBarFadeIn = (Storyboard) GetTemplateChild(TopBarFadeInName);
            _rightFlyoutPlaneProjection = (PlaneProjection)GetTemplateChild(RightFlyoutPlaneProjectionName);
            _rightFlyoutGridContainer = (Grid)GetTemplateChild(RightFlyoutGridContainerName);
            _flyoutBackgroundGrid = (Grid)GetTemplateChild(FlyoutBackgroundGridName);
            _footerContentPresenter = (ContentPresenter) GetTemplateChild(FooterContentPresenterName);
            _titleBarContentPresenter = (ContentPresenter) GetTemplateChild(TitleBarContentPresenterName);

            Responsive();
            Window.Current.SizeChanged += Current_SizeChanged;
            TemplateApplied.SetResult(true);
            
            _rightFlyoutGridContainer.Visibility = Visibility.Collapsed;
            _flyoutBackgroundGrid.Tapped += RightFlyoutGridContainerOnTapped;
        }

        private void Current_SizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            Responsive();
        }

        void Responsive()
        {
            _rightFlyoutContentPresenter.Width = Window.Current.Bounds.Width < 450 ? Window.Current.Bounds.Width : 450;
        }

        private void RightFlyoutGridContainerOnTapped(object sender, TappedRoutedEventArgs tappedRoutedEventArgs)
        {
            if (FlyoutCloseRequested != null)
                FlyoutCloseRequested.Invoke(null, new EventArgs());
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

        public bool IsRightFlyoutOpen { get; set; }
        public bool IsTopBarOpen { get; set; }
    }
}
