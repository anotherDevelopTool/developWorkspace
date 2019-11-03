using Microsoft.VisualStudio.TestTools.UnitTesting;
using DevelopWorkspace.Base.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace DevelopWorkspace.Base.Utils.Tests
{
    [TestClass()]
    public class SqlParserWrapperTests
    {
        [TestMethod()]
        public void ParseTest()
        {
            string sqlText = @"SELECT
CASE status 
WHEN 'a1' THEN 'Active'
WHEN 'a2' THEN 'Active'
WHEN 'a3' THEN 'Active'
WHEN 'i' THEN 'Inactive'
WHEN 't' THEN 'Terminated'
END AS StatusText,

    NVL(C.OrderNumber, 0) as number,
    C.TotalAmount,
    to_char(O.FirstName, 'yyyy/mm/dd') /*aaaa*/ as datefrom,
    O.LastName as lastname,
    O.City,
    C.Country

FROM Customer C
LEFT JOIN[Order] O
      ON O.CustomerId = C.Id
Left JOIN(select a, b, c from Customer  where a = /*orderid*/ 1 and Country='jp') P
        ON O.a = P.a
where TotalAmount = 1 and  city in /*city*/ ('aaa','bbb')

ORDER BY TotalAmount
";
            SqlParserWrapper wrapper = new SqlParserWrapper();
            wrapper.Parse(sqlText);




        }
    }
}