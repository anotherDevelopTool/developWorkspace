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
using Npgsql.NpgsqlConnection
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

        using (OracleConnection con = new OracleConnection("data source=(DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP)(HOST = 172.16.8.70)(port = 1816)) ) (CONNECT_DATA = (SERVICE_NAME = sosi1c) ) );User Id=reception;Password=RECEPTION;")) {
            con.Open();
            DbCommand dbCommand = con.CreateCommand();
            dbCommand.CommandText = "";
            using (DbDataReader rdr = dbCommand.ExecuteReader()){
                while (rdr.Read())
                {
                    DevelopWorkspace.Base.Logger.WriteLine(rdr["TableName"].ToString());
                }
            }


            con.Close();
        }
    }


}