using System;
using System.IO;
using Microsoft.CSharp;
using System.Collections.Generic;
using System.Dynamic;
using System.Xml;
using System.Xml.Xsl;
using System.Xml.Linq;
using System.Linq;
namespace DevelopWorkspace.Base.Utils
{
    /// <summary>
    ///    usage:注意动态对象在vecility模板里无法展开
    ///    -------xml sample ----------------------
    ///    <? xml version="1.0" encoding="utf-8" ?>
    ///    <contacts>
    ///      <contact id = '1' >
    ///        < firstName > Michael </ firstName >
    ///        < lastName > Jordan </ lastName >
    ///        < age > 40 </ age >
    ///        < dob > 1965 </ dob >
    ///        < salary > 100.35 </ salary >
    ///        < columns >
    ///        < column > 1 </ column >
    ///        < column > 2 </ column >
    ///        < column > 3 </ column >
    ///        </ columns >
    ///      </ contact >
    ///      < contact id='2'>
    ///        <firstName>Scottie</firstName>
    ///        <lastName>Pippen</lastName>
    ///        <age>38</age>
    ///        <dob>1967</dob>
    ///        <salary>55.28</salary>
    ///        <columns>
    ///        <column>column111</column>
    ///        <column>2</column>
    ///        <column>3</column>
    ///        </columns>
    ///      </contact>
    ///    </contacts>
    ///
    ///    -------C# code ----------------------
    ///	    System.IO.File.WriteAllText(@"C:\Users\Public\contacts.xml", view[1,1]);
    ///     var xDoc = XDocument.Load(new StreamReader(@"C:\Users\Public\contacts.xml"));
    ///     dynamic root = new ExpandoObject();
    ///
    ///    XmlToDynamic.Parse(root, xDoc.Elements().First());
    ///    DevelopWorkspace.Base.Logger.WriteLine(root.contacts.contact.Count.ToString());
    ///    DevelopWorkspace.Base.Logger.WriteLine(root.contacts.contact[0].firstName.ToString());
    ///    DevelopWorkspace.Base.Logger.WriteLine(root.contacts.contact[0].id.ToString());
    ///    DevelopWorkspace.Base.Logger.WriteLine(root.contacts.contact[1].firstName.ToString());
    ///    DevelopWorkspace.Base.Logger.WriteLine(root.contacts.contact[1].columns.column[0].ToString());
    /// 
    /// </summary>
    class XmlToDynamic
    {
        public static void Parse(dynamic parent, XElement node)
        {
            if (node.HasElements)
            {
                if (node.Elements(node.Elements().First().Name.LocalName).Count() > 1)
                {
                    //list
                    var item = new ExpandoObject();
                    var list = new List<dynamic>();
                    foreach (var element in node.Elements())
                    {
                        Parse(list, element);
                    }

                    AddProperty(item, node.Elements().First().Name.LocalName, list);
                    AddProperty(parent, node.Name.ToString(), item);
                }
                else
                {
                    var item = new ExpandoObject();

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
            if (parent is List<dynamic>)
            {
                (parent as List<dynamic>).Add(value);
            }
            else
            {
                (parent as IDictionary<String, object>)[name] = value;
            }
        }

    }
}
