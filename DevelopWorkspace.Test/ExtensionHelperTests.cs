using Microsoft.VisualStudio.TestTools.UnitTesting;
using DevelopWorkspace.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevelopWorkspace.Base.Tests
{
    [TestClass()]
    public class ExtensionHelperTests
    {
        [TestMethod()]
        public void FormatWithTest()
        {
            
            System.Diagnostics.Debug.WriteLine("strftime('%Y-%m-%d',{ColumnName}) as {ColumnName}".FormatWith(new { ColumnName = "employee_birthday" }));
            Assert.Fail();
        }
        [TestMethod()]
        public void CamelCaseTest()
        {

            System.Diagnostics.Debug.WriteLine("yo_me_12e_NAME".CamelCase());
            System.Diagnostics.Debug.WriteLine("yo".CamelCase());
            System.Diagnostics.Debug.WriteLine("DEL_FLG".CamelCase());
            System.Diagnostics.Debug.WriteLine("SOUKO_CD".CamelCase());
            Assert.Fail();
        }
        [TestMethod()]
        public void AggregateTest()
        {
            List<string> list = new List<string> { "1","2","3"};
            System.Diagnostics.Debug.WriteLine(list.Aggregate((i,j,idx) => i+j));
            Assert.Fail();
        }
    }
}