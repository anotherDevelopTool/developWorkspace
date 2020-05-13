using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevelopWorkspace.Main
{
    public static class ExtensionHelper
    {
        public static void exportToActiveSheetOfExcel(this List<List<string>> rowdataList,  int headerHeight=1, int schemaHeight=2, int _startRow = 1, int _startCol = 1, bool _isOverwritten=false,bool _isFormatted=true)
        {
            DevelopWorkspace.Main.XlApp.loadDataIntoActiveSheet(_startRow, _startCol,headerHeight, schemaHeight, _isOverwritten, _isFormatted,new List<List<List<string>>> { rowdataList });

        }
    }
}
