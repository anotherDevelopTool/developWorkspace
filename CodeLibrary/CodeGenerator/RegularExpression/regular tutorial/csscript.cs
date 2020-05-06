using System;
using Microsoft.Office.Interop.Excel;
using System.IO;
using System.Text.RegularExpressions;
using DevelopWorkspace.Base;
public class Script
{
    public static void Main(string[] args)
    {

        string sunQuerySql_1 = @"select p.product_name, p.supplier_name, 
(select order_id from order_items where product_id = 101) as order_id 
from product p where p.product_id = 101";

        string sunQuerySql_2 = @"SELECT p.product_name FROM product p 
WHERE p.product_id = (SELECT o.product_id FROM order_items o 
WHERE o.product_id = p.product_id)";

        DevelopWorkspace.Base.Logger.WriteLine("Process called");
        string source = "abcd";
        Regex regex = new Regex("(.)+");
        Match m = regex.Match(source);

        DevelopWorkspace.Base.Logger.WriteLine(DevelopWorkspace.Base.Dump.ToDump(m));
    }
}
