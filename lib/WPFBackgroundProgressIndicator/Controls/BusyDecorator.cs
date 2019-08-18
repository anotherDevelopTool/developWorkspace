// Copyright (c) 2011 Abraham Heidebrecht
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and 
// associated documentation files (the "Software"), to deal in the Software without restriction, including 
// without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the 
// following conditions:

// The above copyright notice and this permission notice shall be included in all copies or substantial
// portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT 
// LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
// SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using Heidesoft.Components.BackgroundRendering;

namespace Heidesoft.Components.Controls
{
    /// <summary>
    /// Use this Decorator to show a busy indicator on top of a child element.
    /// 这个indicator，即BusyStyle定义的控件的显示需要在一个独立的线程内执行，否则UI线程内有其他处理时
    /// Indicattor的动画就会静止不动
    /// 在新的线程里执行的UI对象，需要新线程内的Dispatcher才可处理，而且显示的时候需要把他显示在所有对象的
    /// 最前方
    /// </summary>
    [StyleTypedProperty(Property = "BusyStyle", StyleTargetType = typeof(Control))]
    public class BusyDecorator : Decorator
    {
        private Guid backgroundChildId = Guid.Empty;

        #region IsBusyIndicatorShowing Property
        /// <summary>
        /// Identifies the IsBusyIndicatorShowing dependency property.
        /// </summary>
        public static readonly DependencyProperty IsBusyIndicatorShowingProperty = DependencyProperty.Register(
            "IsBusyIndicatorShowing",
            typeof(bool),
            typeof(BusyDecorator),
            new FrameworkPropertyMetadata(false,
                FrameworkPropertyMetadataOptions.AffectsMeasure,
                OnIsShowingChanged));

        /// <summary>
        /// Gets or sets if the BusyIndicator is being shown.
        /// </summary>
        public bool IsBusyIndicatorShowing
        {
            get { return (bool)GetValue(IsBusyIndicatorShowingProperty); }
            set { SetValue(IsBusyIndicatorShowingProperty, value); }
        }
        //进度信息显示
        public void SetIndicator(string indicatorMessage) {
            BackgroundVisualHost.SetIndicator(this, indicatorMessage);

        }
        static void OnIsShowingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var isShowing = (bool)e.NewValue;
            var decorator = (BusyDecorator)d;
            if (isShowing)
            {
                var style = decorator.BusyStyle;
                if (style != null)
                    style.Seal();

                var horizAlign = decorator.BusyHorizontalAlignment;
                var vertAlign = decorator.BusyVerticalAlignment;
                var margin = decorator.BusyMargin;
                //通过delegate把创建indicator的BusyStyle传递给BackgroundVisualHost线程，在这个线程内创建UI元素
                decorator.backgroundChildId = BackgroundVisualHost.AddChild(decorator,
                    () => new Control
                    {
                        Style = style,
                        HorizontalAlignment = horizAlign,
                        VerticalAlignment = vertAlign,
                        Margin = margin
                    });
                if (!decorator.IsEnabledWhenBusy)
                    decorator.SetCurrentValue(IsEnabledProperty, false);
            }
            else
            {
                BackgroundVisualHost.RemoveChild(decorator, decorator.backgroundChildId);

                if (!decorator.IsEnabledWhenBusy)
                    decorator.SetCurrentValue(IsEnabledProperty, true);
            }
            //针对进度指示窗背后的主窗体提示用户等待期间不要操作
            var child = decorator.Child as UIElement;
            if (child != null)
                AnimateChildOpacity(isShowing, child, decorator.FadeTime);
        }

        private static void AnimateChildOpacity(bool isShowing, UIElement child, TimeSpan fadeTime)
        {
            var busyOpacity = GetOpacityWhenBusy(child);
            if (!busyOpacity.HasValue) return;

            var animation = new DoubleAnimation();
            animation.Duration = new Duration(fadeTime);

            if (isShowing)
                animation.To = busyOpacity;

            child.BeginAnimation(UIElement.OpacityProperty, animation);
        }
        #endregion

        #region IsEnabledWhenBusy Dependency Property
        public static readonly DependencyProperty IsEnabledWhenBusyProperty = DependencyProperty.Register(
          "IsEnabledWhenBusy",
          typeof(bool),
          typeof(BusyDecorator),
          new FrameworkPropertyMetadata(true, HandleIsEnabledWhenBusyChanged));

        public bool IsEnabledWhenBusy
        {
            get { return (bool)GetValue(IsEnabledWhenBusyProperty); }
            set { SetValue(IsEnabledWhenBusyProperty, value); }
        }

        static void HandleIsEnabledWhenBusyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var decorator = d as BusyDecorator;
            // we must set this if the indicator is showing always. if already busy, and the new 
            // value is true, we want to disable the decorator. if the new value is false, then
            // the decorator has already been disabled, and we need to remove that setting.
            if (decorator.IsBusyIndicatorShowing)
                decorator.SetCurrentValue(IsEnabledProperty, e.NewValue);
        }
        #endregion

        #region BusyStyle
        ///<summary>
        /// Identifies the <see cref="BusyStyle" /> property.
        /// </summary>
        public static readonly DependencyProperty BusyStyleProperty =
            DependencyProperty.Register(
            "BusyStyle",
            typeof(Style),
            typeof(BusyDecorator),
            new FrameworkPropertyMetadata(OnBusyStyleChanged));

        /// <summary>
        /// Gets or sets the Style to apply to the Control that is displayed as the busy indication.
        /// </summary>
        public Style BusyStyle
        {
            get { return (Style)GetValue(BusyStyleProperty); }
            set { SetValue(BusyStyleProperty, value); }
        }

        static void OnBusyStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BusyDecorator bd = (BusyDecorator)d;
            var nVal = e.NewValue as Style;
            if (nVal != null)
                nVal.Seal();
            bd.SetIndicatorProperty(Control.StyleProperty, nVal);
        }
        #endregion

        #region BusyHorizontalAlignment
        ///<summary>
        /// Identifies the <see cref="BusyHorizontalAlignment" /> property.
        /// </summary>
        public static readonly DependencyProperty BusyHorizontalAlignmentProperty = DependencyProperty.Register(
          "BusyHorizontalAlignment",
          typeof(HorizontalAlignment),
          typeof(BusyDecorator),
          new FrameworkPropertyMetadata(HorizontalAlignment.Center, OnBusyPropertyChanged));

        /// <summary>
        /// Gets or sets the HorizontalAlignment to use to layout the control that contains the busy indicator control.
        /// </summary>
        public HorizontalAlignment BusyHorizontalAlignment
        {
            get { return (HorizontalAlignment)GetValue(BusyHorizontalAlignmentProperty); }
            set { SetValue(BusyHorizontalAlignmentProperty, value); }
        }

        static void OnBusyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((BusyDecorator)d).SetIndicatorProperty(e.Property, e.NewValue);
        }
        #endregion

        #region BusyVerticalAlignment
        ///<summary>
        /// Identifies the <see cref="BusyVerticalAlignment" /> property.
        /// </summary>
        public static readonly DependencyProperty BusyVerticalAlignmentProperty = DependencyProperty.Register(
          "BusyVerticalAlignment",
          typeof(VerticalAlignment),
          typeof(BusyDecorator),
          new FrameworkPropertyMetadata(VerticalAlignment.Center, OnBusyPropertyChanged));

        /// <summary>
        /// Gets or sets the the VerticalAlignment to use to layout the control that contains the busy indicator.
        /// </summary>
        public VerticalAlignment BusyVerticalAlignment
        {
            get { return (VerticalAlignment)GetValue(BusyVerticalAlignmentProperty); }
            set { SetValue(BusyVerticalAlignmentProperty, value); }
        }
        #endregion

        #region BusyMargin Dependency Property
        public static readonly DependencyProperty BusyMarginProperty = DependencyProperty.Register(
          "BusyMargin",
          typeof(Thickness),
          typeof(BusyDecorator),
          new FrameworkPropertyMetadata(new Thickness(0), OnBusyPropertyChanged));

        public Thickness BusyMargin
        {
            get { return (Thickness)GetValue(BusyMarginProperty); }
            set { SetValue(BusyMarginProperty, value); }
        }
        #endregion

        #region OpacityWhenBusy Attached Property
        public static readonly DependencyProperty OpacityWhenBusyProperty = DependencyProperty.RegisterAttached(
          "OpacityWhenBusy",
          typeof(double?),
          typeof(BusyDecorator),
          new FrameworkPropertyMetadata(.5));

        [AttachedPropertyBrowsableForChildren]
        public static double? GetOpacityWhenBusy(UIElement obj)
        {
            return (double?)obj.GetValue(OpacityWhenBusyProperty);
        }

        public static void SetOpacityWhenBusy(UIElement obj, double? value)
        {
            obj.SetValue(OpacityWhenBusyProperty, value);
        }
        #endregion

        #region FadeTime Dependency Property
        public static readonly DependencyProperty FadeTimeProperty = DependencyProperty.Register(
           "FadeTime",
           typeof(TimeSpan),
           typeof(BusyDecorator),
           new UIPropertyMetadata(TimeSpan.FromSeconds(.5)));

        /// <summary>
        /// Gets the amount of time that the fade-in/out animation takes
        /// </summary>
        public TimeSpan FadeTime
        {
            get { return (TimeSpan)GetValue(FadeTimeProperty); }
            set { SetValue(FadeTimeProperty, value); }
        } 
        #endregion

        static BusyDecorator()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(BusyDecorator),
                new FrameworkPropertyMetadata(typeof(BusyDecorator)));
        }

        public BusyDecorator()
        {
            Loaded += (o, e) => UpdateWindowPosition();
        }

        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            base.OnVisualChildrenChanged(visualAdded, visualRemoved);

            if (visualAdded != null && visualAdded is UIElement)
                AnimateChildOpacity(IsBusyIndicatorShowing, (UIElement)visualAdded, FadeTime);
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            if (backgroundChildId != Guid.Empty && IsLoaded)
            {
                // dispatch it so that the arrange pass completes
                Dispatcher.BeginInvoke(new Action(UpdateWindowPosition));
            }
            return base.ArrangeOverride(arrangeSize);
        }

        private void UpdateWindowPosition()
        {
            var root = this.VisualAncestors().OfType<UIElement>().LastOrDefault();
            if (root != null)
                BackgroundVisualHost.WindowPositionChanged(this, this.TranslatePoint(new Point(0, 0), root));
        }

        private void SetIndicatorProperty(DependencyProperty property, object value)
        {
            if (backgroundChildId != Guid.Empty)
                BackgroundVisualHost.DispatchAction(this, backgroundChildId, c => c.SetValue(property, value));
        }
    }
}
