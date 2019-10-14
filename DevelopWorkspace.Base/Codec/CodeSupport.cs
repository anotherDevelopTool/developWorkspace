using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.VisualBasic.FileIO;
using System.ComponentModel;
using System.Collections;
using System.Globalization;
using System.Xml.Linq;
using unvell.ReoGrid;

namespace DevelopWorkspace.Base.Codec
{
    public class SchemaRange
    {
        public object parent;
        public object current;
        public string key;
        public int row;
        public int col;
    }
    public static class CodeSupport
    {
        //JSON：通过JSON映射成的JOBJECT转换成Dictionary，以方便vecolity使用
        public static object ToCollections(object o)
        {
            var jo = o as JObject;
            if (jo != null) return jo.ToObject<IDictionary<string, object>>().ToDictionary(k => k.Key, v => ToCollections(v.Value));
            var ja = o as JArray;
            if (ja != null) return ja.ToObject<List<object>>().Select(ToCollections).ToList();
            return o;
        }

        //XML：XML文件转换成dictonary对象
        public static void Parse(object parent, XElement node)
        {
            if (node.HasElements)
            {
                if (node.Elements(node.Elements().First().Name.LocalName).Count() > 1)
                {
                    //list
                    //var item = new ExpandoObject();
                    //var list = new List<object>();
                    var item = new Dictionary<string, object>();
                    var list = new List<object>();
                    foreach (var element in node.Elements())
                    {
                        Parse(list, element);
                    }

                    //AddProperty(item, node.Elements().First().Name.LocalName, list);
                    AddProperty(parent, node.Name.ToString(), list);
                }
                else
                {
                    var item = new Dictionary<string, object>();

                    foreach (var attribute in node.Attributes())
                    {
                        AddProperty(item, attribute.Name.ToString(), attribute.Value.Trim());
                    }

                    //element
                    foreach (var element in node.Elements())
                    {
                        Parse(item, element);
                    }

                    AddProperty(parent, node.Name.ToString(), item);
                }
            }
            else
            {
                AddProperty(parent, node.Name.ToString(), node.Value.Trim());
            }
        }

        private static void AddProperty(dynamic parent, string name, object value)
        {
            if (parent is List<object>)
            {
                (parent as List<object>).Add(value);
            }
            else
            {
                (parent as IDictionary<String, object>)[name] = value;
            }
        }
        //CSV：文本转换成dictionary,适合excel的的table定义等拷贝时使用
        public static Dictionary<string, object> ReadCsvFileTextFieldParser(string csvText, string delimiter = "\t")
        {

            var list = new List<Dictionary<string, string>>();
            var fieldDict = new Dictionary<int, string>();

            using (TextFieldParser parser = new TextFieldParser(new StringReader(csvText)))
            {
                parser.SetDelimiters(delimiter);

                bool headerParsed = false;

                while (!parser.EndOfData)
                {
                    //Processing row
                    string[] rowFields = parser.ReadFields();
                    if (!headerParsed)
                    {
                        for (int i = 0; i < rowFields.Length; i++)
                        {
                            fieldDict.Add(i, rowFields[i]);
                        }
                        headerParsed = true;
                    }
                    else
                    {
                        var rowData = new Dictionary<string, string>();
                        for (int i = 0; i < rowFields.Length; i++)
                        {
                            rowData[fieldDict[i]] = rowFields[i];
                        }
                        list.Add(rowData);
                    }
                }
            }
            Dictionary<string, object> ret = new Dictionary<string, object>();
            ret["parent"] = list;
            return ret;
        }

        //如何将POCO对象变换成Dictionary<string,object>对象
        public class ObjectConvertInfo
        {
            public object ConvertObject { set; get; }
            public IList<Type> IgnoreTypes { set; get; }
            public IList<string> IgnoreProperties { set; get; }
            public int MaxDeep { set; get; } = 5;
        }
        private static bool ContainIgnoreCase(this IEnumerable<string> list, string value)
        {
            if (list == null || !list.Any())
                return false;

            if (value == null)
                return false;

            return list.Any(item => item.Equals(value, StringComparison.OrdinalIgnoreCase));
        }

        //
        static public Dictionary<string, object> ConvertObjectToDictionary(object targetobject)
        {

            var result = ConvertObjectToDictionaryInternal(new ObjectConvertInfo
            {
                ConvertObject = targetobject is IEnumerable ? new { parent = targetobject } : targetobject,
                IgnoreProperties = new List<string> { "PropertyA", "PropertyB" },
                IgnoreTypes = new List<Type> { typeof(IntPtr), typeof(Delegate), typeof(Type) },
                MaxDeep = 5
            });
            return result;
        }
        static private Dictionary<string, object> ConvertObjectToDictionaryInternal(ObjectConvertInfo objectConvertInfo)
        {
            try
            {
                var dictionary = new Dictionary<string, object>();
                MapToDictionaryInternal(dictionary, objectConvertInfo, 0);
                return dictionary;
            }
            catch (Exception ex)
            {
                DevelopWorkspace.Base.Logger.WriteLine(ex.Message, Level.ERROR);
                return null;
            }
        }
        static private void MapToDictionaryInternal(IEnumerable dictionary, ObjectConvertInfo objectConvertInfo, int deep)
        {
            try
            {
                if (deep > objectConvertInfo.MaxDeep)
                    return;

                if (dictionary is IDictionary)
                {
                    var properties = objectConvertInfo.ConvertObject.GetType().GetProperties();
                    foreach (var propertyInfo in properties)
                    {
                        if (objectConvertInfo.IgnoreProperties.ContainIgnoreCase(propertyInfo.Name))
                            continue;

                        var key = propertyInfo.Name;
                        var value = propertyInfo.GetValue(objectConvertInfo.ConvertObject, null);
                        if (value == null)
                            continue;

                        var valueType = value.GetType();

                        if (objectConvertInfo.IgnoreTypes.Contains(valueType))
                            continue;

                        if (valueType.IsPrimitive || valueType == typeof(String))
                        {
                            ((IDictionary)dictionary)[key] = value.ToString();
                        }
                        else if (value is IEnumerable)
                        {
                            var list = new List<object>();
                            ((IDictionary)dictionary)[propertyInfo.Name] = list;
                            foreach (var data in (IEnumerable)value)
                            {
                                MapToDictionaryInternal(list, new ObjectConvertInfo
                                {
                                    ConvertObject = data,
                                    IgnoreTypes = objectConvertInfo.IgnoreTypes,
                                    IgnoreProperties = objectConvertInfo.IgnoreProperties,
                                    MaxDeep = objectConvertInfo.MaxDeep
                                }, deep + 1);
                            }
                        }
                    }
                }
                else
                {

                    var subdictionary = new Dictionary<string, object>();
                    ((IList)dictionary).Add(subdictionary);
                    var properties = objectConvertInfo.ConvertObject.GetType().GetProperties();
                    foreach (var propertyInfo in properties)
                    {
                        if (objectConvertInfo.IgnoreProperties.ContainIgnoreCase(propertyInfo.Name))
                            continue;

                        var key = propertyInfo.Name;
                        var value = propertyInfo.GetValue(objectConvertInfo.ConvertObject, null);
                        if (value == null)
                            continue;

                        var valueType = value.GetType();

                        if (objectConvertInfo.IgnoreTypes.Contains(valueType))
                            continue;

                        if (valueType.IsPrimitive || valueType == typeof(String))
                        {
                            subdictionary[key] = value.ToString();
                        }
                        else if (value is IEnumerable)
                        {
                            var list = new List<object>();
                            subdictionary[key] = list;
                            foreach (var data in (IEnumerable)value)
                            {
                                MapToDictionaryInternal(list, new ObjectConvertInfo
                                {
                                    ConvertObject = data,
                                    IgnoreTypes = objectConvertInfo.IgnoreTypes,
                                    IgnoreProperties = objectConvertInfo.IgnoreProperties,
                                    MaxDeep = objectConvertInfo.MaxDeep
                                }, deep + 1);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DevelopWorkspace.Base.Logger.WriteLine(ex.Message, Level.ERROR);
            }
        }
        public static Dictionary<string, object> getSchemaDictionary(ReoGridControl reogrid)
        {
            //Directory.CreateDirectory(@"C:\workspace\csharp\WPF Extended DataGrid 2015\1\2");
            var worksheet = reogrid.GetWorksheetByName("sheet1");
            Dictionary<string, object> retDictonary = new Dictionary<string, object>();
            // fill data into worksheet
            var selectedRange = worksheet.Ranges["A1:CV200"];

            for (int row = 0; row < selectedRange.Rows - 1; row++)
            {
                for (int col = 0; col < selectedRange.Cols - 1; col++)
                {
                    if (selectedRange.Cells[row, col].Data != null && selectedRange.Cells[row, col].Data.ToString().EndsWith("{}"))
                    {
                        string nameCellString = selectedRange.Cells[row, col].Data.ToString();
                        Dictionary<string, object>  parent = new Dictionary<string, object>();
                        retDictonary[nameCellString.Substring(0, nameCellString.Length - 2)] = parent;
                        col++;
                        row++;
                        int subRow;
                        for (subRow = row; subRow < selectedRange.Rows - 1; subRow++)
                        {
                            //如果碰到sibling则跳出
                            if (selectedRange.Cells[subRow, col - 1].Data != null && selectedRange.Cells[subRow, col - 1].Data.ToString() != "") break;
                            if (selectedRange.Cells[subRow, col].Data != null)
                            {
                                string keyCellString = selectedRange.Cells[subRow, col].Data.ToString();
                                if (keyCellString.EndsWith("[]"))
                                {
                                    SchemaRange schemmaRange = new SchemaRange
                                    {
                                        parent = parent,
                                        key = keyCellString.Substring(0, keyCellString.Length - 2),
                                        row = subRow,
                                        col = col
                                    };
                                    reverseListObject(selectedRange, schemmaRange);
                                    subRow = schemmaRange.row;
                                }
                                else
                                {
                                    parent[keyCellString] = selectedRange.Cells[subRow, col + 1].Data.ToString();
                                }
                            }
                            else {
                                //进入下一个结构判断
                                break;
                            }
                        }

                        row = subRow;
                        col = 0;

                    }

                }
            }

            return retDictonary;

        }

        private static void reverseListObject(ReferenceRange selectedRange, SchemaRange schemmaRange)
        {
            schemmaRange.current = new List<Dictionary<string, object>>();
            if (typeof(IDictionary).IsAssignableFrom(schemmaRange.parent.GetType()))
            {

                ((IDictionary)schemmaRange.parent)[schemmaRange.key] = schemmaRange.current;
            }
            else
            {
            }

            schemmaRange.col++;
            List<string> schemaList = new List<string>();
            for (int idx = schemmaRange.col; idx < selectedRange.Cols - 1; idx++)
            {
                if (selectedRange.Cells[schemmaRange.row, idx].Data != null)
                {
                    schemaList.Add(selectedRange.Cells[schemmaRange.row, idx].Data.ToString());
                }
            }
            schemmaRange.row++;
            int subRow, subCol;
            for (subRow = schemmaRange.row; subRow < selectedRange.Rows -1 ; subRow++)
            {
                if (selectedRange.Cells[subRow, schemmaRange.col - 1].Data != null && selectedRange.Cells[subRow, schemmaRange.col - 1].Data.ToString() != "") break;
                Dictionary<string, object> column = new Dictionary<string, object>();
                bool isEmptyRow = true;
                for (subCol = schemmaRange.col; subCol < schemmaRange.col + schemaList.Count; subCol++)
                {
                    if (selectedRange.Cells[subRow, subCol].Data != null)
                    {
                        System.Diagnostics.Debug.WriteLine(selectedRange.Cells[subRow, subCol].Data);
                        column[schemaList[subCol - schemmaRange.col]] = selectedRange.Cells[subRow, subCol].Data.ToString();
                        isEmptyRow = false;
                    }
                    else
                    {
                        column[schemaList[subCol - schemmaRange.col]] = "";
                    }

                }
                if (isEmptyRow) break;
                else
                    ((List<Dictionary<string, object>>)schemmaRange.current).Add(column);
            }

            schemmaRange.row = subRow;
            schemmaRange.row--;


        }

        //利用Dicionary可以对key进行任意追加的特性，而且velocity引擎可以按照key的名字进行取值的特性
        //对IDictionary进行遍历，针对特定属性可以自定义规则已提供给代码引擎方便获取数据
        static public IDictionary<string, object> ApplyRuleToDictionary(IDictionary<string, object> originalDic, List<ConvertRule> convertRules, List<RenameRule> renameRules)
        {
            var convert = new Dictionary<string, object>();
            ApplyRuleToDictionaryInternal(originalDic, convert, convertRules, renameRules);
            return convert;
        }
        //目前版本不是很严谨，元素的类型只允许Dictionary和List，如果含有其他复合类型，则需要fix
        static private void ApplyRuleToDictionaryInternal(object originalData, object convertedData, List<ConvertRule> convertRules, List<RenameRule> renameRules)
        {
            try
            {
                if (typeof(IDictionary).IsAssignableFrom(originalData.GetType())) {
                    var originalDic = (IDictionary)originalData;
                    var convertedDic = ((IDictionary<string, object>)convertedData);

                    foreach (string key in originalDic.Keys)
                    {
                        var renamedKey = key;
                        //自定义规则适应
                        var renameRule = renameRules.FirstOrDefault(rule => rule.SchemaKey.Equals(key));
                        if (renameRule != null)
                        {
                            renamedKey = renameRule.SchemaAlias;
                        }
                        var valueType = originalDic[key].GetType();
                        if (valueType.IsPrimitive || valueType == typeof(String))
                        {
                            convertedDic[renamedKey] = originalDic[key];
                            //自定义规则适应
                            convertRules.Where(rule => rule.SchemaKey.Equals(key)).ToList().ForEach(rule => { convertedDic[renamedKey + rule.SchemaAlias] = rule.ConvertFunc(originalDic[key].ToString()); });
                        }
                        else if (typeof(IDictionary).IsAssignableFrom(valueType))
                        {
                            var subdictionary = new Dictionary<string, object>();
                            convertedDic[renamedKey] = subdictionary;
                            ApplyRuleToDictionaryInternal(originalDic[key], subdictionary, convertRules, renameRules);
                        }
                        else if (typeof(IList).IsAssignableFrom(valueType))
                        {
                            var valuelist = new List<object>();
                            convertedDic[renamedKey] = valuelist;
                            ApplyRuleToDictionaryInternal(originalDic[key], valuelist, convertRules, renameRules);
                        }
                    }
                }
                else if (typeof(IList).IsAssignableFrom(originalData.GetType()))
                {
                    var originalList = (IEnumerable)originalData;
                    var convertedList = ((IList<object>)convertedData);

                    foreach (var item in originalList)
                    {
                        if (typeof(IDictionary).IsAssignableFrom(item.GetType()))
                        {
                            var subdictionary = new Dictionary<string, object>();
                            convertedList.Add(subdictionary);
                            ApplyRuleToDictionaryInternal(item, subdictionary, convertRules, renameRules);
                        }
                        else if (typeof(IList).IsAssignableFrom(item.GetType()))
                        {
                            var valuelist = new List<object>();
                            convertedList.Add(valuelist);
                            ApplyRuleToDictionaryInternal(item, valuelist, convertRules, renameRules);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                DevelopWorkspace.Base.Logger.WriteLine(ex.Message, Level.ERROR);
            }
        }
        // sale_report -> saleReport
        // sale_report -> SaleReport
        public static Func<string, string> CONVERT_FIELD = (originalString) =>
        {
            string convertString = originalString;
            TextInfo txtInfo = new CultureInfo("en-us", false).TextInfo;
            convertString = txtInfo.ToTitleCase(convertString).Replace("_", string.Empty).Replace(" ", string.Empty);
            convertString = $"{convertString.First().ToString().ToLowerInvariant()}{convertString.Substring(1)}";
            return convertString;
        };
        public static Func<string, string> CONVERT_PROPERTY = (originalString) =>
        {
            string convertString = originalString;
            TextInfo txtInfo = new CultureInfo("en-us", false).TextInfo;
            convertString = txtInfo.ToTitleCase(convertString).Replace("_", string.Empty).Replace(" ", string.Empty);
            return convertString;
        };
        public static Func<string, string> CONVERT_UPPERCASE = (originalString) => originalString.ToUpper();
        public static Func<string, string> CONVERT_LOWERCASE = (originalString) => originalString.ToLower();

        //var newoject = new Clazz { name = "123_abc_efg", methods = new List<Method>() { new Method() { name = "m1", type = "string", parameteres = new List<Parameter>() { new Parameter() { name = "p1" } } } } };
        //var result = ConvertObjectToDictionary(new ObjectConvertInfo
        //{
        //    ConvertObject = newoject,
        //    IgnoreProperties = new List<string> { "PropertyA", "PropertyB" },
        //    IgnoreTypes = new List<Type> { typeof(IntPtr), typeof(Delegate), typeof(Type) },
        //    MaxDeep = 3
        //});

        //Func<string, string> CameralFunc = new Func<string, string>(getCameralString);

        //List<ConvertRule> convertRules = new List<ConvertRule>()
        //    {
        //        new ConvertRule(){ Key ="name",Alias ="field",ConvertFunc = CONVERT_FIELD },
        //        new ConvertRule(){ Key ="name",Alias ="property",ConvertFunc = CONVERT_PROPERTY },
        //        new ConvertRule(){ Key ="name",Alias ="uppercase",ConvertFunc = CONVERT_UPPERCASE },
        //        new ConvertRule(){ Key ="name",Alias ="lowercase",ConvertFunc = CONVERT_LOWERCASE }

        //    };
        //List<RenameRule> renameRules = new List<RenameRule>()
        //    {
        //        new RenameRule(){ Key ="type",Alias ="typeString" }

        //    };
        //var convert = ApplyRuleToDictionary(result, convertRules, renameRules);




    }
}
