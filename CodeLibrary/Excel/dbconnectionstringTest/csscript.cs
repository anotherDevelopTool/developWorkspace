using System;
using Microsoft.Office.Interop.Excel;
using System.IO;
using System.Data.SQLite;
using System.Data;
using System.Data.Common;
using System.Linq;
using Microsoft.CSharp;
using System.Collections.Generic;
using DevelopWorkspace.Base;
using Oracle.ManagedDataAccess;
using Oracle.ManagedDataAccess.Client;
using MySql.Data;
using MySql.Data.MySqlClient;
public class Script
{

    public static void Main(string[] args)
    {

//###
//System.Data.SQLite.SQLiteConnection
//Data Source = northwindEF.db; Pooling = true; FailIfMissing = false
//
//###
//Npgsql.NpgsqlConnection
//Host=127.0.0.1;Port=5432;Username=postgres;Password=admin;Database=gsession;Integrated Security=false;

//###
//MySql.Data.MySqlClient.MySqlConnection
//server=localhost;User Id=root;password=developworkspace;Database=test

//###
//System.Data.OracleClient.OracleConnection
//data source=sosi1c;User Id=reception;Password=reception;

//### 注意这个模式虽然不用安装oracle客户端，但是只支持：Access to Oracle Database 10g Release 2 or later
//Oracle.ManagedDataAccess.Client.OracleConnection
//con.ConnectionString = "User ID=USER1; Password=PASS2; Data Source=DS_TEST;";
DbParameter param = null;
 DbTransaction dbTran = null;
        using (OracleConnection con = new OracleConnection("Data Source=(DESCRIPTION = (ENABLE = BROKEN)(ADDRESS = (PROTOCOL=tcp)(HOST=100.67.53.23)(PORT=2050))(CONNECT_DATA =(SERVICE_NAME = sv_fstyl)));User Id=fstyluser;Password=test01;")) {
            con.Open();
            var  dbCommand = con.CreateCommand();
            dbTran = dbCommand.Connection.BeginTransaction();
//            dbCommand.CommandText = "select * from FSTYLADMIN.TBL_BRAND where rownum = 1";
//            using (DbDataReader rdr = dbCommand.ExecuteReader()){
//                while (rdr.Read())
//                {
//                    DevelopWorkspace.Base.Logger.WriteLine(rdr["BRAND_CD"].ToString());
//                }
//            }





//DevelopWorkspace.Base.Logger.WriteLine("single sql process start++++++++",Level.DEBUG);
//for(int i = 0;i< 10000;i++){
//dbCommand.CommandText  = "UPDATE FSTYLADMIN.TBL_BRAND SET BRAND_CD='1232',BRAND_NAME='####' WHERE BRAND_CD='1232'";
//dbCommand.ExecuteNonQuery();
//}
//DevelopWorkspace.Base.Logger.WriteLine("single sql process end++++++++",Level.DEBUG);




DevelopWorkspace.Base.Logger.WriteLine("multi sql process start++++++++",Level.DEBUG);

dbCommand.CommandText  = "UPDATE FSTYLADMIN.TBL_BRAND SET BRAND_CD=:1,BRAND_NAME=:2 WHERE BRAND_CD=:1";
List<string> brandcdList = new List<string>();
List<string> brandnameList = new List<string>();
for(int i = 0;i< 10000;i++){
	brandcdList.Add("1233");
	brandnameList.Add("1233");
}
	brandcdList.Add("1232");
	brandnameList.Add("#######222");
		brandcdList.Add("1233");
	brandnameList.Add("######222");
		brandcdList.Add("1234");
	brandnameList.Add("#######222");
dbCommand.ArrayBindCount = brandcdList.Count;
dbCommand.Parameters.Add(":1", OracleDbType.NChar, brandcdList.ToArray(), ParameterDirection.Input);
dbCommand.Parameters.Add(":2", DbType.String, brandnameList.ToArray(), ParameterDirection.Input);
dbCommand.ExecuteNonQuery();
			
DevelopWorkspace.Base.Logger.WriteLine("multi sql process end++++++++",Level.DEBUG);


			
dbTran.Commit();
            con.Close();
        }
    }


}
