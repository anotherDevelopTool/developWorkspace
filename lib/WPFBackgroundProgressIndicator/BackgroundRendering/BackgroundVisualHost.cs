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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;
using Heidesoft.Components.Controls;
using Heidesoft.Components.Windows;
using System.Globalization;

namespace Heidesoft.Components.BackgroundRendering
{
    /// <summary>
    /// This class provides helper methods to track background visuals
    /// </summary>
    internal static class BackgroundVisualHost
    {
        private static readonly Dictionary<FrameworkElement, List<Guid>> childMap = new Dictionary<FrameworkElement, List<Guid>>();
        private static readonly Dictionary<Guid, AddedChildHolder> childHolders = new Dictionary<Guid, AddedChildHolder>();

        #region ElementId Attached Property
        private static readonly DependencyPropertyKey ElementIdPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
          "ElementId",
          typeof(Guid),
          typeof(BackgroundVisualHost),
          new FrameworkPropertyMetadata(Guid.Empty));

        private static readonly DependencyProperty ElementIdProperty = ElementIdPropertyKey.DependencyProperty;

        [AttachedPropertyBrowsableForType(typeof(UIElement))]
        private static Guid GetElementId(UIElement obj)
        {
            return (Guid)obj.GetValue(ElementIdProperty);
        }

        private static void SetElementId(UIElement obj, Guid value)
        {
            obj.SetValue(ElementIdPropertyKey, value);
        }
        #endregion

        #region BackgroundHost Attached Property
        private static readonly DependencyPropertyKey BackgroundHostPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
          "BackgroundHost",
          typeof(WindowBackgroundVisualHost),
          typeof(WindowBackgroundVisualHost),
          new FrameworkPropertyMetadata(null));

        private static readonly DependencyProperty BackgroundHostProperty = BackgroundHostPropertyKey.DependencyProperty;

        [AttachedPropertyBrowsableForType(typeof(UIElement))]
        private static WindowBackgroundVisualHost GetBackgroundHost(UIElement obj)
        {
            return (WindowBackgroundVisualHost)obj.GetValue(BackgroundHostProperty);
        }

        private static void SetBackgroundHost(UIElement obj, WindowBackgroundVisualHost value)
        {
            obj.SetValue(BackgroundHostPropertyKey, value);
        }
        #endregion

        //进度信息显示
        internal static void SetIndicator(UIElement element, string indicatorMessage)
        {
            var root = element.VisualAncestors().OfType<UIElement>().LastOrDefault();
            if (root == null) return;

            var host = GetBackgroundHost(root);
            if (host == null) return;
            host.SetIndicator(indicatorMessage);
        }
        //进度信息显示
        internal static void Shutdown(UIElement element)
        {
            var root = element.VisualAncestors().OfType<UIElement>().LastOrDefault();
            if (root == null) return;

            var host = GetBackgroundHost(root);
            if (host == null) return;
            host.Shutdown();
        }
        //从VisualTree中寻找特定的UIelement
        public static T FindChild<T>(DependencyObject parent, string childName) where T : DependencyObject
        {
            // Confirm parent and childName are valid. 
            if (parent == null) return null;

            T foundChild = null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child
                T childType = child as T;
                if (childType == null)
                {
                    // recursively drill down the tree
                    foundChild = FindChild<T>(child, childName);

                    // If the child is found, break so we do not overwrite the found child. 
                    if (foundChild != null) break;
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;
                    // If the child's name is set for search
                    if (frameworkElement != null && frameworkElement.Name == childName)
                    {
                        // if the child's name is of the request name
                        foundChild = (T)child;
                        break;
                    }
                }
                else
                {
                    // child element found.
                    foundChild = (T)child;
                    break;
                }
            }

            return foundChild;
        }





        static WindowBackgroundVisualHost GetHost(UIElement element)
        {
            var root = element.VisualAncestors().OfType<UIElement>().LastOrDefault();
            if (root == null) return null;

            var host = GetBackgroundHost(root);
            if (host == null)
            {
                var decorator = root.VisualDescendants().OfType<AdornerDecorator>().FirstOrDefault();
                if (decorator != null && decorator.Child != null)
                {
                    host = new WindowBackgroundVisualHost(decorator.Child);
                    if (decorator is BusyAdornerDecorator)
                        ((BusyAdornerDecorator)decorator).BusyIndicatorHost = host;
                    else
                        decorator.AdornerLayer.Add(host);

                    SetBackgroundHost(root, host);
                }
            }

            return host;
        }

        public static Guid AddChild(FrameworkElement parent, Func<UIElement> createElement)
        {
            var id = Guid.NewGuid();

            // this will either add the child now, or wait until it is loaded
            childHolders[id] = new AddedChildHolder(parent, createElement, id);

            return id;
        }

        public static void RemoveChild(FrameworkElement parent, Guid elementId)
        {
            AddedChildHolder holder;
            if (childHolders.TryGetValue(elementId, out holder))
            {
                holder.Detach();
                childHolders.Remove(elementId);
            }
        }

        public static void WindowPositionChanged(FrameworkElement parent, Point point)
        {
            var host = GetHost(parent);
            if (host == null)
                return;

            List<Guid> children;
            if (childMap.TryGetValue(parent, out children))
            {
                foreach (var id in children)
                    host.Move(id, point);
            }
        }

        public static void DispatchAction(FrameworkElement parent, Guid id, Action<UIElement> action)
        {
            var host = GetHost(parent);
            if (host != null)
                host.PerformAction(id, action);
        }

        private static void HandleAdornedSizeChanged(object sender, SizeChangedEventArgs e)
        {
            var elem = sender as FrameworkElement;
            var host = GetHost(elem);
            if (host == null)
                return;

            var size = elem.RenderSize;
            foreach (var id in childMap[elem])
                host.Resize(id, size);
        }

        private class AddedChildHolder : IWeakEventListener
        {
            private Guid id;
            private Func<UIElement> createElement;
            private FrameworkElement parent;

            public AddedChildHolder(FrameworkElement parent, Func<UIElement> createElement, Guid id)
            {
                this.parent = parent;
                this.createElement = createElement;
                this.id = id;
                                
                IsVisibleWeakEventManager.AddListener(parent, this);
                LoadedWeakEventManager.AddListener(parent, this);

                if (parent.IsVisible)
                    AddChild();
            }

            private void Loaded(object sender, EventArgs e)
            {
                if (parent.IsVisible)
                    AddChild();
            }

            private void IsVisibleChanged(object sender, EventArgs<bool> e)
            {
                if (e.Value)
                {
                    if (parent.IsLoaded)
                        AddChild();
                }
                else
                {
                    SizeChangedWeakEventManager.RemoveListener(parent, this);
                    RemoveChild();
                }
            }

            public void Detach()
            {
                RemoveChild();
                IsVisibleWeakEventManager.RemoveListener(parent, this);
                SizeChangedWeakEventManager.RemoveListener(parent, this);
                LoadedWeakEventManager.RemoveListener(parent, this);
            }

            public void InvalidateArrange()
            {
                parent.InvalidateArrange();
            }

            private void AddChild()
            {
                var host = GetHost(parent);
                //UI主线程中取得VisualTree中的祖先，通常是最外层的Window
                var root = parent.VisualAncestors().OfType<UIElement>().LastOrDefault();
                if (root == null) return;
                //取得坐标以及显示矩形区域坐标
                var xForm = parent.TransformToAncestor(root);
                var bounds = xForm.TransformBounds(new Rect(parent.RenderSize));

                if (host != null)
                    host.AddChild(createElement, id, bounds);

                List<Guid> children;
                if (!childMap.TryGetValue(parent, out children))
                {
                    children = new List<Guid>();
                    childMap.Add(parent, children);
                }

                SizeChangedWeakEventManager.AddListener(parent, this);
                children.Add(id);
            }

            private void RemoveChild()
            {
                var host = GetHost(parent);
                if (host != null)
                    host.RemoveChild(id);

                List<Guid> children;
                if (childMap.TryGetValue(parent, out children))
                {
                    children.Remove(id);
                    if (children.Count == 0)
                        childMap.Remove(parent);
                }
            }

            bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
            {
                if (managerType == typeof(IsVisibleWeakEventManager))
                {
                    IsVisibleChanged(sender, (EventArgs<bool>)e);
                    return true;
                }
                else if (managerType == typeof(SizeChangedWeakEventManager))
                {
                    HandleAdornedSizeChanged(sender, (SizeChangedEventArgs)e);
                    return true;
                }
                else if (managerType == typeof(LoadedWeakEventManager))
                {
                    Loaded(sender, e);
                    return true;
                }

                return false;
            }
        }

        private class WindowBackgroundVisualHost : Adorner
        {
            private readonly HashSet<Guid> addedChildren = new HashSet<Guid>();
            private readonly HostVisual child = new HostVisual();
            private readonly AutoResetEvent sync = new AutoResetEvent(false);
            private readonly Thread backgroundThread;
            private Dispatcher backgroundDispatcher;
            private Canvas root;

            public WindowBackgroundVisualHost(UIElement adorned)
                : base(adorned)
            {
                // make sure this is on top of everything else
                Panel.SetZIndex(this, Int32.MaxValue);
                backgroundThread = new Thread(CreateAndRun);
                backgroundThread.SetApartmentState(ApartmentState.STA);
                backgroundThread.Name = "BackgroundVisualHostThread";
                backgroundThread.IsBackground = true;
                backgroundThread.Start();

                AddLogicalChild(child);
                AddVisualChild(child);

                sync.WaitOne();
            }
            //进度信息显示
            internal void SetIndicator(string indicatorMessage) {
                BeginInvoke(() =>
                {
                    TextBlock blk = FindChild<TextBlock>(root, "indicator");
                    //Button cancel = FindChild<Button>(root, "cancel");
                    //System.Diagnostics.Debug.WriteLine("cancel button:" + cancel.IsEnabled.ToString());
                    //cancel.Visibility = Visibility.Visible;
                    //cancel.IsEnabled = true;
                    if (blk == null) return;
                    //if(MeasureString(indicatorMessage,blk).Width >360 ) blk.Width = MeasureString(indicatorMessage, blk).Width;
                    //else blk.Width = 360;
                    blk.Text = indicatorMessage;
                });
            }
            internal void Shutdown()
            {
                backgroundDispatcher.InvokeShutdown();
            }
            //TODO 2019/3/14 根据字符串的长度自动调整宽度
            private Size MeasureString(string candidate, TextBlock blk)
            {
                var formattedText = new FormattedText(
                    candidate,
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface(blk.FontFamily, blk.FontStyle, blk.FontWeight, blk.FontStretch),
                    blk.FontSize,
                    Brushes.Black,
                    new NumberSubstitution());

                return new Size(formattedText.Width, formattedText.Height);
            }
            public void AddChild(Func<UIElement> createElement, Guid id, Rect bounds)
            {
                BeginInvoke(
                    () =>
                    {
                        if (!addedChildren.Add(id))
                            return;

                        var child = createElement();
                        SetElementId(child, id);
                        root.Children.Add(child);
                        Canvas.SetLeft(child, bounds.X);
                        Canvas.SetTop(child, bounds.Y);
                        child.SetCurrentValue(FrameworkElement.WidthProperty, bounds.Width);
                        child.SetCurrentValue(FrameworkElement.HeightProperty, bounds.Height);
                    });
            }

            public void RemoveChild(Guid id)
            {
                PerformAction(id, child =>
                {
                    addedChildren.Remove(id);
                    root.Children.Remove(child);
                });
            }

            public void Resize(Guid id, Size s)
            {
                PerformAction(id,
                    child =>
                    {
                        child.SetCurrentValue(FrameworkElement.WidthProperty, s.Width);
                        child.SetCurrentValue(FrameworkElement.HeightProperty, s.Height);
                    });
            }

            public void Move(Guid id, Point p)
            {
                PerformAction(id,
                    child =>
                    {
                        Canvas.SetLeft(child, p.X);
                        Canvas.SetTop(child, p.Y);
                    });
            }

            public void PerformAction(Guid id, Action<UIElement> action)
            {
                BeginInvoke(() =>
                {
                    var child = root.Children
                                    .OfType<UIElement>()
                                    .FirstOrDefault(c => GetElementId(c) == id);

                    if (child != null)
                        action(child);
                });
            }

            protected override Size ArrangeOverride(Size finalSize)
            {
                foreach (var id in addedChildren)
                {
                    AddedChildHolder holder;
                    if (childHolders.TryGetValue(id, out holder))
                        holder.InvalidateArrange();
                }

                return base.ArrangeOverride(finalSize);
            }

            protected override Visual GetVisualChild(int index)
            {
                if (index == 0)
                    return child;

                throw new IndexOutOfRangeException();
            }

            protected override System.Collections.IEnumerator LogicalChildren
            {
                get { yield return child; }
            }

            protected override int VisualChildrenCount
            {
                get { return 1; }
            }

            internal void BeginInvoke(Action act)
            {
                backgroundDispatcher.BeginInvoke(act);
            }
            //新创建的UI线程，BusyIndicator的UIElement都在这个线程内创建，那么访问它需要使用属于本线程的Dispatcher.CurrentDispatcher
            private void CreateAndRun()
            {
                backgroundDispatcher = Dispatcher.CurrentDispatcher;
                var source = new VisualTargetPresentationSource(child);
                root = new Canvas();

                sync.Set();
                source.RootVisual = root;

                Dispatcher.Run();
                source.Dispose();
            }
        }
    }
}
