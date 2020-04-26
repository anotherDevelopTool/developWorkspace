using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DevelopWorkspace.Base.Utils;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace DevelopWorkspace.Test
{
    [TestClass]
    public class UnitTest2
    {

        [TestMethod]
        public void TestMethod1()
        {


            

            foreach (var filename in Directory.GetFiles(@"C:\wbc_sam\", "*.txt")) {
                System.Diagnostics.Debug.WriteLine(filename);
            }



            var tests = Files.ReadAllText(@"C:\wbc_sam\123.txt", Encoding.UTF8);
            var csvStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(tests));
            var csvReader = new DevelopWorkspace.Base.Utils.CsvReader(new StreamReader(csvStream), ",");
            while (csvReader.Read())
            {
                System.Diagnostics.Debug.WriteLine("+=======");
                for (int idx = 0; idx < csvReader.FieldsCount; idx++) {
                    System.Diagnostics.Debug.WriteLine(csvReader[idx]);
                }
            }



            //var testDelims = new[] { "\t", "%%%", ",", ",", "," };
            //    var testTrimFields = new[] { true, true, false, true, true };
            //    var testBufSize = new[] { 1024, 1024, 1024, 5, 5 };
            //    var expected = new string[] {
            //    "1|2|3|#5|||#6|7\"|\"|#",
            //    "1|2|3|#5|6%%6|7%|#",
            //    "1 | 2  |3 |#  4|5| 6|# \"7\"|\"\"8 |9|#",
            //    "",
            //    "1|2|3|#"
            //};
            //    for (int i = 0; i < tests.Length; i++)
            //    {
            //        var csvRdr = new CsvReader(
            //            new StreamReader(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(tests[i]))),
            //            testDelims[i]);
            //        csvRdr.TrimFields = testTrimFields[i];
            //        csvRdr.BufferSize = testBufSize[i];

            //        var sb = new StringBuilder();
            //        csvRdr.Read(); // skip header row
            //        while (csvRdr.Read())
            //        {
            //            sb.Append(csvRdr[0] + "|");
            //            sb.Append(csvRdr[1] + "|");
            //            sb.Append(csvRdr[2] + "|");
            //            sb.Append("#");
            //        }
            //    }
            //}
        }

    }
}
