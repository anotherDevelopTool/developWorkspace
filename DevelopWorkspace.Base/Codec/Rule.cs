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

namespace DevelopWorkspace.Base.Codec
{
    public class ConvertRule
    {
        public string SchemaKey { set; get; }
        public string SchemaAlias { set; get; }
        public Func<string, string> ConvertFunc { set; get; }
    }
    public class RenameRule
    {
        public string SchemaKey { set; get; }
        public string SchemaAlias { set; get; }
    }
    public class ConvertFunc
    {
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
    }
}
