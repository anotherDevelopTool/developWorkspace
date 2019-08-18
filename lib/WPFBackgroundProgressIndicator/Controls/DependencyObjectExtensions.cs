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

using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace Heidesoft.Components.Controls
{
    public static class DependencyObjectExtensions
    {
        public static IEnumerable<DependencyObject> VisualDescendants(this DependencyObject d)
        {
            var tree = new Queue<DependencyObject>();
            tree.Enqueue(d);

            while (tree.Count > 0)
            {
                var item = tree.Dequeue();
                var count = VisualTreeHelper.GetChildrenCount(item);
                for (int i = 0; i < count; ++i)
                {
                    var child = VisualTreeHelper.GetChild(item, i);
                    tree.Enqueue(child);
                    yield return child;
                }
            }
        }

        public static IEnumerable<DependencyObject> VisualAncestors(this DependencyObject d)
        {
            var parent = VisualTreeHelper.GetParent(d);
            while (parent != null)
            {
                yield return parent;
                parent = VisualTreeHelper.GetParent(parent);
            }
        }
    }
}
