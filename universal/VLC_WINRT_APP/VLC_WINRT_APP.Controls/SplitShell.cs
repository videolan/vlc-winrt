using System;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace VLC_WINRT_APP.Controls
{

    [TemplatePart(Name = EdgePaneName, Type = typeof(Grid))]
    [TemplatePart(Name = SidebarGridContainerName, Type = typeof(Grid))]
    [TemplatePart(Name = SidebarContentPresenterName, Type = typeof(ContentPresenter))]
    [TemplatePart(Name = AlwaysVisibleSidebarContentPresenterName, Type = typeof(ContentPresenter))]
    [TemplatePart(Name = TopBarContentPresenterName, Type = typeof(ContentPresenter))]
    [TemplatePart(Name = InformationGridName, Type = typeof(Grid))]
    [TemplatePart(Name = NavigationFrameName, Type = typeof(Frame))]
    [TemplatePart(Name = InformationTextBlockName, Type = typeof(TextBlock))]
    public sealed class SplitShell : Control
    {
        public TaskCompletionSource<bool> TemplateApplied = new TaskCompletionSource<bool>();
        private const string EdgePaneName = "EdgePane";
        private const string SidebarGridContainerName = "SidebarGridContainer";
        private const string SidebarContentPresenterName = "SidebarContentPresenter";
        private const string AlwaysVisibleSidebarContentPresenterName = "AlwaysVisibleSidebarContentPresenter";
        private const string NavigationFrameName = "NavigationFrame";
        private const string TopBarContentPresenterName = "TopBarContentPresenter";
        private const string TopBarVisualStateName = "TopBarVisualState";
        private const string SideBarVisualStateName = "SideBarVisualState";
        private const string AlwaysVisibleSideBarVisualStateName = "AlwaysVisibleSideBarVisualState";
        private const string InformationGridName = "InformationGrid";
        private const string InformationTextBlockName = "InformationTextBlock";

        private Grid _edgePaneGrid;
        private Grid _sidebarGridContainer;
        private ContentPresenter _sidebarContentPresenter;
        private ContentPresenter _alwaysVisibleSidebarContentPresenter;
        private ContentPresenter _topBarContentPresenter;
        private Grid _informationGrid;
        private TextBlock _informationTextBlock;
        private Frame _navigationFrame;

        private bool _alwaysVisibleSideBarVisualState;

        public Frame NavigationFrame
        {
            get { return _navigationFrame; }
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
            _navigationFrame = (Frame)GetTemplateChild(NavigationFrameName);
            _sidebarGridContainer = (Grid)GetTemplateChild(SidebarGridContainerName);
            _sidebarContentPresenter = (ContentPresenter)GetTemplateChild(SidebarContentPresenterName);
            _alwaysVisibleSidebarContentPresenter = (ContentPresenter)GetTemplateChild(AlwaysVisibleSidebarContentPresenterName);
            _topBarContentPresenter = (ContentPresenter)GetTemplateChild(TopBarContentPresenterName);
            _informationTextBlock = (TextBlock)GetTemplateChild(InformationTextBlockName);
            _informationGrid = (Grid)GetTemplateChild(InformationGridName);
            _edgePaneGrid = (Grid)GetTemplateChild(EdgePaneName);

            TemplateApplied.SetResult(true);

            _sidebarGridContainer.Visibility = Visibility.Collapsed;
            _edgePaneGrid.Tapped += _edgePaneGrid_Tapped;
            _sidebarGridContainer.Tapped += _sidebarGridContainer_Tapped;
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

        void _edgePaneGrid_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            Open();
        }

        void _sidebarGridContainer_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            _sidebarGridContainer.Visibility = Visibility.Collapsed;
        }

        public void Open()
        {
            _sidebarGridContainer.Visibility = Visibility.Visible;
        }
    }
}
