using System;
using System.IO;
using Microsoft.CSharp;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Xsl;
using System.Xml.Linq;
using System.Linq;
namespace DevelopWorkspace.Base.Utils
{
    /// <summary>
    /// 这个类是通过Dictionary以及LIST把XML的各个要素装载进来，以供Velocity使用
    /// </summary>
    class XmlToGenericObject
    {
        public static void Parse(object parent, XElement node)
        {
            if (node.HasElements)
            {
                if (node.Elements(node.Elements().First().Name.LocalName).Count() > 1)
                {
                    //list
                    var list = new List<object>();
                    foreach (var element in node.Elements())
                    {
                        Parse(list, element);
                    }

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
    }
}
