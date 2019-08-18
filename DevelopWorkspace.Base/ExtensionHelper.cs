using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.UI;
using Microsoft.VisualBasic;

namespace DevelopWorkspace.Base
{
    //在这里的FormatWith是扩展了系统类的一个实践
    //http://james.newtonking.com/archive/2008/03/29/formatwith-2-0-string-formatting-with-named-variables
    //可以通过这个方式缩短代码量
    public static  class ExtensionHelper
    {
        public static string FormatWith(this string format, object source)
        {
            return FormatWith(format, null, source);
        }

        public static string FormatWith(this string format, IFormatProvider provider, object source)
        {
            if (format == null)
                throw new ArgumentNullException("format");

            Regex r = new Regex(@"(?<start>\{)+(?<property>[\w\.\[\]]+)(?<format>:[^}]+)?(?<end>\})+",
              RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

            List<object> values = new List<object>();
            string rewrittenFormat = r.Replace(format, delegate (Match m)
            {
                Group startGroup = m.Groups["start"];
                Group propertyGroup = m.Groups["property"];
                Group formatGroup = m.Groups["format"];
                Group endGroup = m.Groups["end"];

                values.Add((propertyGroup.Value == "0")
                  ? source
                  : DataBinder.Eval(source, propertyGroup.Value));

                return new string('{', startGroup.Captures.Count) + (values.Count - 1) + formatGroup.Value
                  + new string('}', endGroup.Captures.Count);
            });

            return string.Format(provider, rewrittenFormat, values.ToArray());
        }

        /// <summary>
        /// camel表记的转换，通常数据库字段到Java或者C#代码时需要这样的转化
        /// </summary>
        /// <param name="original"></param>
        /// <param name="splitChar"></param>
        /// <returns></returns>
        public static string CamelCase(this string original, bool firstUpper =false,char splitChar = '_')
        {
            string[] originalArray = original.Split(splitChar);
            string camel = "";
            for (int i = 0; i < originalArray.Length; i++)
            {
                if (i == 0 && firstUpper == false)
                {
                    camel += originalArray[i].ToLower();
                }
                else {
                    camel += Strings.StrConv(originalArray[i], VbStrConv.ProperCase);
                }
            }
            return camel;
        }


        //改进的Aggerate扩展（示例代码，实际使用请添加空值检查）
        public static TSource Aggregate<TSource>(this IEnumerable<TSource> source, Func<TSource, TSource, int, TSource> func)
        {
            int index = 0;
            using (IEnumerator<TSource> enumerator = source.GetEnumerator())
            {
                enumerator.MoveNext();
                index++;
                TSource current = enumerator.Current;
                while (enumerator.MoveNext())
                    current = func(current, enumerator.Current, index++);
                return current;
            }
        }

    }
}
