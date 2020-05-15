using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace DevelopWorkspace.Base.Utils
{
    public class DataConvert
    {
        public static T[,] To2dArray<T>(List<List<T>> list)
        {
            if (list.Count == 0 || list[0].Count == 0)
                throw new ArgumentException("The list must have non-zero dimensions.");

            var result = new T[list.Count, list[0].Count];
            for (int i = 0; i < list.Count; i++)
            {
                //为了在有问题的文件时也可以程度的显示，即使不一致也强行变换
                for (int j = 0; j < list[i].Count && j< list[0].Count; j++)
                {
                    result[i, j] = list[i][j];
                    if (j == 0) {
                        if (list[i].Count != list[0].Count)
                        {
                            Type typeParameterType = typeof(T);
                            if (typeParameterType == typeof(string))
                            {
                                 result[i, j] = (T)Convert.ChangeType("!error!", typeof(T));
                                //throw new InvalidOperationException("The list cannot contain elements (lists) of different sizes.");
                            }
                        }
                    }
                }
            }
            return result;
        }

    }

}
