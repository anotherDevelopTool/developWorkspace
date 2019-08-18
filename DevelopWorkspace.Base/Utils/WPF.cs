using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace DevelopWorkspace.Base.Utils
{
    public class WPF
    {
        //为了显示子窗口显示在TOP窗口的中心位置等使用，如何找到TOP窗口（理论依据不足，需要揣摩验证）
        public static Window GetTopWindow(FrameworkElement obj)
        {
            return Application.Current.MainWindow;
            //上边这个方法也是偶然看到了，明显不熟练导致的哈哈
            //FrameworkElement parent = null;
            //parent = (obj.Parent == null ? obj.TemplatedParent : obj.Parent) as FrameworkElement;
            //if (parent == null && obj is Window) return obj as Window;
            //if (parent == null && !(obj is Window)) return null;
            //return GetTopWindow(parent as FrameworkElement);
        }

        //逻辑树
        public static T FindLogicaChild<T>(DependencyObject parent, string childName) where T : DependencyObject
        {
            T foundChild = null;

            foreach (object logicalChild in LogicalTreeHelper.GetChildren(parent))
            {
                if (logicalChild is T && (logicalChild as FrameworkElement).Name == childName)
                {
                    foundChild = logicalChild as T;
                    break;
                }
                //DevelopWorkspace.Base.Logger.WriteLine(logicalChild.ToString());
                if(logicalChild is DependencyObject)
                {
                    foundChild = FindLogicaChild<T>(logicalChild as DependencyObject, childName);
                    if(foundChild != null) break;
                }
                
            }
            return foundChild;
        }


        public static T FindChild<T>(DependencyObject parent, string childName)
    where T : DependencyObject
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
    }
}
