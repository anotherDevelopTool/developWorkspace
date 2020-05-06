using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.UI;
using Microsoft.VisualBasic;
using System.Dynamic;
using System.Collections;

namespace DevelopWorkspace.Base
{
    //在这里的FormatWith是扩展了系统类的一个实践
    //http://james.newtonking.com/archive/2008/03/29/formatwith-2-0-string-formatting-with-named-variables
    //可以通过这个方式缩短代码量
    public static  class ExtensionHelper
    {
        /// <summary>
        /// 
        /// @"{javac} -cp {cp} {g4}".FormatWith(setting)
        /// </summary>
        /// <param name="format"></param>
        /// <param name="source"></param>
        /// <returns></returns>
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


        /// <summary>
        /// Extension method that turns a dictionary of string and object to an ExpandoObject
        /// </summary>
        public static ExpandoObject ToExpando(this IDictionary<string, object> dictionary)
        {
            var expando = new ExpandoObject();
            var expandoDic = (IDictionary<string, object>)expando;

            // go through the items in the dictionary and copy over the key value pairs)
            foreach (var kvp in dictionary)
            {
                // if the value can also be turned into an ExpandoObject, then do it!
                if (kvp.Value is IDictionary<string, object>)
                {
                    var expandoValue = ((IDictionary<string, object>)kvp.Value).ToExpando();
                    expandoDic.Add(kvp.Key, expandoValue);
                }
                else if (kvp.Value is ICollection)
                {
                    // iterate through the collection and convert any strin-object dictionaries
                    // along the way into expando objects
                    var itemList = new List<object>();
                    foreach (var item in (ICollection)kvp.Value)
                    {
                        if (item is IDictionary<string, object>)
                        {
                            var expandoItem = ((IDictionary<string, object>)item).ToExpando();
                            itemList.Add(expandoItem);
                        }
                        else
                        {
                            itemList.Add(item);
                        }
                    }

                    expandoDic.Add(kvp.Key, itemList);
                }
                else
                {
                    expandoDic.Add(kvp);
                }
            }

            return expando;
        }

        public static void Dump<T>(this T o)
        {
            Ilogger Inner = AppDomain.CurrentDomain.GetData("logger") as Ilogger;
            if (Inner == null) return;
            if (Inner.level == Level.DEBUG || Inner.level == Level.TRACE)
            {
                Inner.WriteLine(DevelopWorkspace.Base.Dump.ToDump(o), Level.DEBUG);
            }
            return;
        }

        public static T[,] To2dArray<T>(this List<List<T>> list) {
            return DevelopWorkspace.Base.Utils.DataConvert.To2dArray<T>(list);
        }
        public static List<List<T>>  ToListWithList<T>(this T[,] value2_copy)
        {
            List<List<T>> table = new List<List<T>>();
            for (int i = 0; i < value2_copy.GetLength(0); i++)
            {
                List<T> temp = new List<T>();
                for (int j = 0; j < value2_copy.GetLength(1); j++)
                {
                    temp.Add(value2_copy[i, j]);
                }
                table.Add(temp);
            }
            return table;
        }

    }
}
