using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevelopWorkspace.Main
{
    public static class ExtensionHelper
    {
        public static void exportToActiveSheetOfExcel(this List<List<string>> rowdataList, int headerHeight, int schemaHeight)
        {
            DevelopWorkspace.Main.XlApp.loadDataIntoActiveSheet(headerHeight, schemaHeight, new List<List<List<string>>> { rowdataList });

        }
    }
}
