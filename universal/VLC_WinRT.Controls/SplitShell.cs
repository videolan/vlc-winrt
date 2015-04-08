using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace VLC_WinRT.Controls
{

    [TemplatePart(Name = EdgePaneName, Type = typeof(Grid))]
    [TemplatePart(Name = SidebarGridContainerName, Type = typeof(Grid))]
    [TemplatePart(Name = SidebarContentPresenterName, Type = typeof(ContentPresenter))]
    [TemplatePart(Name = AlwaysVisibleSidebarContentPresenterName, Type = typeof(ContentPresenter))]
    [TemplatePart(Name = TopBarContentPresenterName, Type = typeof(ContentPresenter))]
    [TemplatePart(Name = InformationGridName, Type = typeof(Grid))]
    [TemplatePart(Name = ContentPresenterName, Type = typeof(ContentPresenter))]
    [TemplatePart(Name = RightFlyoutContentPresenterName, Type = typeof(ContentPresenter))]
    [TemplatePart(Name = InformationTextBlockName, Type = typeof(TextBlock))]
    [TemplatePart(Name = RightFlyoutFadeInName, Type = typeof(Storyboard))]
    [TemplatePart(Name = RightFlyoutFadeOutName, Type = typeof(Storyboard))]
    [TemplatePart(Name = RightFlyoutPlaneProjectionName, Type = typeof(PlaneProjection))]
    [TemplatePart(Name = RightFlyoutGridContainerName, Type = typeof(Grid))]
    [TemplatePart(Name = FooterContentPresenterName, Type = typeof(ContentPresenter))]
    public sealed class SplitShell : Control
    {
        public TaskCompletionSource<bool> TemplateApplied = new TaskCompletionSource<bool>();

        private const string EdgePaneName = "EdgePane";
        private const string SidebarGridContainerName = "SidebarGridContainer";
        private const string SidebarContentPresenterName = "SidebarContentPresenter";
        private const string AlwaysVisibleSidebarContentPresenterName = "AlwaysVisibleSidebarContentPresenter";
        private const string ContentPresenterName = "ContentPresenter";
        private const string TopBarContentPresenterName = "TopBarContentPresenter";
        private const string TopBarVisualStateName = "TopBarVisualState";
        private const string SideBarVisualStateName = "SideBarVisualState";
        private const string AlwaysVisibleSideBarVisualStateName = "AlwaysVisibleSideBarVisualState";
        private const string InformationGridName = "InformationGrid";
        private const string InformationTextBlockName = "InformationTextBlock";
        private const string RightFlyoutContentPresenterName = "RightFlyoutContentPresenter";
        private const string RightFlyoutFadeInName = "RightFlyoutFadeIn";
        private const string RightFlyoutFadeOutName = "RightFlyoutFadeOut";
        private const string RightFlyoutPlaneProjectionName = "RightFlyoutPlaneProjection";
        private const string RightFlyoutGridContainerName = "RightFlyoutGridContainer";
        private const string FooterContentPresenterName = "FooterContentPresenter";

        private Grid _edgePaneGrid;
        private Grid _sidebarGridContainer;
        private Grid _rightFlyoutGridContainer;
        private ContentPresenter _contentPresenter;
        private ContentPresenter _sidebarContentPresenter;
        private ContentPresenter _alwaysVisibleSidebarContentPresenter;
        private ContentPresenter _topBarContentPresenter;
        private ContentPresenter _rightFlyoutContentPresenter;
        private ContentPresenter _footerContentPresenter;

        private PlaneProjection _rightFlyoutPlaneProjection;
        private Storyboard _rightFlyoutFadeIn;
        private Storyboard _rightFlyoutFadeOut;
        private Grid _informationGrid;
        private TextBlock _informationTextBlock;
        private bool _alwaysVisibleSideBarVisualState;
        
        public async void SetContentPresenter(object contentPresenter)
        {
            await TemplateApplied.Task;
            _contentPresenter.Content = contentPresenter;
        }

        public async void SetSidebarContentPresenter(object contentPresenter)
        {
            await TemplateApplied.Task;
            _sidebarContentPresenter.Content = contentPresenter;
            _alwaysVisibleSidebarContentPresenter.Content = contentPresenter;
        }

        public async void SetTopbarContentPresenter(object contentPresenter)
        {
            await TemplateApplied.Task;
            _topBarContentPresenter.Content = contentPresenter;
        }

        public async void SetInformationText(string text)
        {
            await TemplateApplied.Task;
            _informationGrid.Visibility = string.IsNullOrEmpty(text) ? Visibility.Collapsed : Visibility.Visible;
            _informationTextBlock.Text = text;
        }

        public async void SetInformationBrush(Brush brush)
        {
            await TemplateApplied.Task;
            _informationGrid.Background = brush;
        }

        public async void SetEdgePaneColor(object color)
        {
            await TemplateApplied.Task;
            _edgePaneGrid.Background = (Brush)color;
        }

        public async void SetAlwaysVisibleSidebar(bool alwaysVisible)
        {
            await TemplateApplied.Task;
            _alwaysVisibleSideBarVisualState = alwaysVisible;
            Responsive();
        }

        public async void SetRightPaneContentPresenter(object content)
        {
            await TemplateApplied.Task;
            if(IsRightFlyoutOpen)
            {
                HideFlyout();
                await Task.Delay(400);
            }
            _rightFlyoutContentPresenter.Content = content;
            ShowFlyout();
        }

        public async void SetFooterContentPresenter(object content)
        {
            await TemplateApplied.Task;
            _footerContentPresenter.Content = content;
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
        #region AlwaysVisibleSidebar Property

        public bool AlwaysVisibleSidebar
        {
            get { return (bool)GetValue(AlwaysVisibleSidebarProperty); }
            set { SetValue(AlwaysVisibleSidebarProperty, value); }
        }

        public static readonly DependencyProperty AlwaysVisibleSidebarProperty = DependencyProperty.Register("AlwaysVisibleSidebar",
            typeof(DependencyProperty), typeof(SplitShell),
            new PropertyMetadata(default(bool), AlwaysVisibleSidebarPropertyChangedCallback));

        private static void AlwaysVisibleSidebarPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var that = (SplitShell)dependencyObject;
            that.SetAlwaysVisibleSidebar((bool)dependencyPropertyChangedEventArgs.NewValue);
        }
        #endregion

        #region EdgePaneColor Property

        public DependencyObject EdgePaneColor
        {
            get { return (DependencyObject)GetValue(EdgePaneColorProperty); }
            set { SetValue(EdgePaneColorProperty, value); }
        }

        public static readonly DependencyProperty EdgePaneColorProperty = DependencyProperty.Register("EdgePaneColor",
            typeof(DependencyProperty), typeof(SplitShell),
            new PropertyMetadata(default(DependencyObject), EdgePaneColorPropertyChangedCallback));

        private static void EdgePaneColorPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var that = (SplitShell)dependencyObject;
            that.SetEdgePaneColor(dependencyPropertyChangedEventArgs.NewValue);
        }
        #endregion

        #region SideBarContent Property
        public DependencyObject SideBarContent
        {
            get { return (DependencyObject)GetValue(SideBarContentProperty); }
            set { SetValue(SideBarContentProperty, value); }
        }

        public static readonly DependencyProperty SideBarContentProperty = DependencyProperty.Register(
            "SideBarContent", typeof(DependencyObject), typeof(SplitShell), new PropertyMetadata(default(DependencyObject), SideBarContentPropertyChangedCallback));


        private static void SideBarContentPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var that = (SplitShell)dependencyObject;
            that.SetSidebarContentPresenter(dependencyPropertyChangedEventArgs.NewValue);
        }
        #endregion

        #region TopBarContent Property
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
        public Brush InformationBackground
        {
            get { return (Brush)GetValue(InformationBackgroundProperty); }
            set { SetValue(InformationBackgroundProperty, value); }
        }

        public static readonly DependencyProperty InformationBackgroundProperty = DependencyProperty.Register("InformationBackground", typeof(DependencyObject), typeof(SplitShell), new PropertyMetadata(default(Brush), InformationBackgroundPropertyChangedCallback));

        private static void InformationBackgroundPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var that = (SplitShell)dependencyObject;
            that.SetInformationBrush((Brush)dependencyPropertyChangedEventArgs.NewValue);
        }


        public string InformationText
        {
            get { return (string)GetValue(InformationTextProperty); }
            set { SetValue(InformationTextProperty, value); }
        }

        public static readonly DependencyProperty InformationTextProperty = DependencyProperty.Register("InformationText", typeof(DependencyObject), typeof(SplitShell), new PropertyMetadata(default(string), InformationContentPresenterPropertyChangedCallback));

        private static void InformationContentPresenterPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var that = (SplitShell)dependencyObject;
            that.SetInformationText((string)dependencyPropertyChangedEventArgs.NewValue);
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
            _sidebarGridContainer = (Grid)GetTemplateChild(SidebarGridContainerName);
            _sidebarContentPresenter = (ContentPresenter)GetTemplateChild(SidebarContentPresenterName);
            _alwaysVisibleSidebarContentPresenter = (ContentPresenter)GetTemplateChild(AlwaysVisibleSidebarContentPresenterName);
            _topBarContentPresenter = (ContentPresenter)GetTemplateChild(TopBarContentPresenterName);
            _informationTextBlock = (TextBlock)GetTemplateChild(InformationTextBlockName);
            _informationGrid = (Grid)GetTemplateChild(InformationGridName);
            _edgePaneGrid = (Grid)GetTemplateChild(EdgePaneName);
            _rightFlyoutContentPresenter = (ContentPresenter)GetTemplateChild(RightFlyoutContentPresenterName);
            _rightFlyoutFadeIn = (Storyboard)GetTemplateChild(RightFlyoutFadeInName);
            _rightFlyoutFadeOut = (Storyboard)GetTemplateChild(RightFlyoutFadeOutName);
            _rightFlyoutPlaneProjection = (PlaneProjection)GetTemplateChild(RightFlyoutPlaneProjectionName);
            _rightFlyoutGridContainer = (Grid)GetTemplateChild(RightFlyoutGridContainerName);
            _footerContentPresenter = (ContentPresenter) GetTemplateChild(FooterContentPresenterName);

            TemplateApplied.SetResult(true);

            _sidebarGridContainer.Visibility = Visibility.Collapsed;
            _rightFlyoutGridContainer.Visibility = Visibility.Collapsed;
            _edgePaneGrid.Tapped += _edgePaneGrid_Tapped;
            _sidebarGridContainer.Tapped += _sidebarGridContainer_Tapped;
            _rightFlyoutGridContainer.Tapped += RightFlyoutGridContainerOnTapped;
            Window.Current.SizeChanged += Current_SizeChanged;
            Responsive();
        }

        void Current_SizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            Responsive();
        }

        private void Responsive()
        {
            VisualStateManager.GoToState(this, TopBarVisualStateName, false);
        }
        
        void _edgePaneGrid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Open();
        }

        private void RightFlyoutGridContainerOnTapped(object sender, TappedRoutedEventArgs tappedRoutedEventArgs)
        {
            HideFlyout();
        }

        void _sidebarGridContainer_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _sidebarGridContainer.Visibility = Visibility.Collapsed;
        }

        public void Open()
        {
            _sidebarGridContainer.Visibility = Visibility.Visible;
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

        public bool IsRightFlyoutOpen { get; set; }
    }
}
