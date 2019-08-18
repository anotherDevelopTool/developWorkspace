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
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Heidesoft.Components.Controls
{
    /// <summary>
    /// If this AdornerDecorator is used to host Adorners, it will guarantee that the visual created by the BusyDecorator
    /// will appear below the Adorners.
    /// </summary>
    public class BusyAdornerDecorator : AdornerDecorator
    {
        #region BusyIndicatorHost Dependency Property
        internal static readonly DependencyProperty BusyIndicatorHostProperty = DependencyProperty.Register(
          "BusyIndicatorHost",
          typeof(FrameworkElement),
          typeof(BusyAdornerDecorator),
          new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure));

        internal FrameworkElement BusyIndicatorHost
        {
            get { return (FrameworkElement)GetValue(BusyIndicatorHostProperty); }
            set { SetValue(BusyIndicatorHostProperty, value); }
        }
        #endregion

        protected override Size MeasureOverride(Size constraint)
        {
            if (BusyIndicatorHost != null)
                BusyIndicatorHost.Measure(constraint);

            return base.MeasureOverride(constraint);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var rect = new Rect(finalSize);
            if (Child != null)
                Child.Arrange(rect);

            if (BusyIndicatorHost != null)
                BusyIndicatorHost.Arrange(rect);

            if (VisualTreeHelper.GetParent(AdornerLayer) != null)
                AdornerLayer.Arrange(rect);

            return finalSize;
        }

        protected override int VisualChildrenCount
        {
            get
            {
                var count = base.VisualChildrenCount;
                if (BusyIndicatorHost != null)
                    count++;

                return count;
            }
        }

        protected override Visual GetVisualChild(int index)
        {
            switch (index)
            {
                case 0:
                    return Child;

                case 1:
                    if (BusyIndicatorHost != null)
                        return BusyIndicatorHost;
                    else
                        return AdornerLayer;

                case 2:
                    if (BusyIndicatorHost == null)
                        throw new ArgumentOutOfRangeException("index");
                    else
                        return AdornerLayer;

                default:
                    throw new ArgumentOutOfRangeException("index");
            }
        }
    }
}
