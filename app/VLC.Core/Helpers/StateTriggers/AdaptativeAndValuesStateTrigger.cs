using System;
using System.Collections.Generic;
using System.Text;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml;
using WinRTXamlToolkit.Common;

namespace VLC.Helpers.StateTriggers
{
    public class AdaptativeAndValuesStateTrigger : StateTriggerBase
    {
        #region state value
        public bool StateValue
        {
            get { return (bool)GetValue(StateValueProperty); }
            set { SetValue(StateValueProperty, value); }
        }
        
        public static readonly DependencyProperty StateValueProperty = DependencyProperty.Register(nameof(StateValue), typeof(bool), typeof(AdaptativeAndValuesStateTrigger), new PropertyMetadata(null, StateValuePropertyChangedCallback));

        private static void StateValuePropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            var that = d as AdaptativeAndValuesStateTrigger;
            that.ComputeIsActive();
        }
        #endregion
        #region ctor
        public AdaptativeAndValuesStateTrigger()
        {
            //this.RegisterPropertyChangedCallback(MinWindowWidthProperty, OnMinWindowWidthPropertyChanged);

            var window = CoreApplication.GetCurrentView()?.CoreWindow;
            if (window != null)
            {
                var weakEvent = new WeakEventListener<AdaptativeAndValuesStateTrigger, CoreWindow, WindowSizeChangedEventArgs>(this)
                {
                    OnEventAction = (instance, s, e) => OnCoreWindowOnSizeChanged(s, e),
                    OnDetachAction = (instance) => window.SizeChanged -= instance.OnEvent
                };
                window.SizeChanged += weakEvent.OnEvent;
            }
        }
        #endregion
        #region window value


        public int MinWindowWidth
        {
            get { return (int)GetValue(MinWindowWidthProperty); }
            set { SetValue(MinWindowWidthProperty, value); }
        }
        
        public static readonly DependencyProperty MinWindowWidthProperty = DependencyProperty.Register(nameof(MinWindowWidth), typeof(int), typeof(AdaptativeAndValuesStateTrigger), new PropertyMetadata(0));


        private void OnCoreWindowOnSizeChanged(CoreWindow sender, WindowSizeChangedEventArgs args)
        {
            ComputeIsActive();
        }

        private void OnMinWindowHeightPropertyChanged(DependencyObject sender, DependencyProperty dp)
        {
            var window = CoreApplication.GetCurrentView()?.CoreWindow;
            if (window != null)
            {
                ComputeIsActive();
            }
        }

        private void OnMinWindowWidthPropertyChanged(DependencyObject sender, DependencyProperty dp)
        {
            var window = CoreApplication.GetCurrentView()?.CoreWindow;
            if (window != null)
            {
                ComputeIsActive();
            }
        }
        #endregion

        void ComputeIsActive()
        {
            var meetsWindowSizeThreshold = Window.Current.Bounds.Width >= MinWindowWidth;
            IsActive = meetsWindowSizeThreshold && StateValue;
        }

        #region ITriggerValue

        private bool _isActive;

        /// <summary>
        /// Gets a value indicating whether this trigger is active.
        /// </summary>
        /// <value><c>true</c> if this trigger is active; otherwise, <c>false</c>.</value>
        public bool IsActive
        {
            get { return _isActive; }
            private set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    this.SetActive(value);
                    IsActiveChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Occurs when the <see cref="IsActive" /> property has changed.
        /// </summary>
        public event EventHandler IsActiveChanged;

        #endregion ITriggerValue
    }
}
