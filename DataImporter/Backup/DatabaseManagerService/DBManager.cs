using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;
using System.Data.SqlClient;
using System.Collections;

namespace DatabaseManagerService
{
    static public class DBManager
    {
        //    static List<KeyValuePair<SymbolKeyStockExchange, List<DBConnectionProperties>>> symbolDBLookUp = new List<KeyValuePair<SymbolKeyStockExchange, List<DBConnectionProperties>>>();

        //    static private Queue taskQueue = new Queue();

        //    static private Dictionary<DatabaseType, DataTable> dataTables = new Dictionary<DatabaseType, DataTable>();

        //    static public void PopulateLookUp()
        //    {
        //        // Will read from the Configuration database instead which also have the table name field

        //        //List<DBConnectionProperties> dbConnectinStr = new List<DBConnectionProperties>();

        //        //for (int index = 1; index < System.Configuration.ConfigurationManager.ConnectionStrings.Count; index++)
        //        //{
        //        //    String connectionStr = System.Configuration.ConfigurationManager.ConnectionStrings[index].ConnectionString;

        //        //    String[] StrArray = connectionStr.Split(';');


        //        //    DBConnectionProperties dbProp = new DBConnectionProperties();
        //        //    dbProp.ConnectionStr = connectionStr;

        //        //    String[] tempFirst = StrArray[0].Split('=');
        //        //    dbProp.DataSource = tempFirst.Last();

        //        //    String[] tempSecond = StrArray[1].Split('=');
        //        //    dbProp.Database = tempSecond.Last();

        //        //    dbProp.CreationStoredProcedure = "usp_CreateDatabaseExtension_StockExchange";
        //        //    dbProp.SCConfigUpdateSP = "usp_SCConfigUpdate_StockExchange";

        //        //    String[] temp = StrArray[2].Split('=');

        //        //    dbProp.IntegratedSecurity = true;
        //        //    // dbProp.TableName = tableName;

        //        //    dbConnectinStr.Add(dbProp);
        //        //}

        //        //SymbolKeyStockExchange it = new SymbolKeyStockExchange(); it.SymbolKey = "APP"; it.StockExchange = "AMEX";
        //        //SymbolKeyStockExchange itV = new SymbolKeyStockExchange(); itV.SymbolKey = "GLU"; itV.StockExchange = "AMEX";
        //        //SymbolKeyStockExchange itB = new SymbolKeyStockExchange(); itB.SymbolKey = "ACU"; itB.StockExchange = "AMEX";

        //        //symbolDBLookUp.Add(new KeyValuePair<SymbolKeyStockExchange, List<DBConnectionProperties>>(it, dbConnectinStr));
        //        //symbolDBLookUp.Add(new KeyValuePair<SymbolKeyStockExchange, List<DBConnectionProperties>>(itV, dbConnectinStr));
        //        //symbolDBLookUp.Add(new KeyValuePair<SymbolKeyStockExchange, List<DBConnectionProperties>>(itB, dbConnectinStr));



        //        ////test queue population//

        //        //DataTable dt = null;
        //        //using (SqlConnection c = new SqlConnection("Data Source=DENNIS-HP\\SQLEXPRESS;Initial Catalog=FMDA_Result; Integrated Security=true"))
        //        //{
        //        //    c.Open();
        //        //    using (SqlDataAdapter a = new SqlDataAdapter("SELECT * FROM dbo.tblHistoricalCorrelationsTT WHERE dbo.tblHistoricalCorrelationsTT.SymbolID='XXXER'", c))
        //        //    {
        //        //        dt = new DataTable();
        //        //        a.Fill(dt);
        //        //    }
        //        //}

        //        //DataTable resultTable = new DataTable();
        //        //resultTable = dt;

        //        //for (int r = 0; r < 100; r++)
        //        //{
        //        //    DataRow row = resultTable.NewRow();
        //        //    row["CorrelatingEntityID"] = r;
        //        //    row["SymbolID"] = "TR" + r;
        //        //    row["StartDate"] = DateTime.Now;
        //        //    row["EndDate"] = DateTime.Now;
        //        //    row["Distance"] = r * 3;
        //        //    row["SourceStockExchange"] = "FakeExchange";
        //        //    row["DestinationStockExchange"] = "TrendoExchange";

        //        //    row["Event"] = "Test";
        //        //    row["HC_ID"] = "junks100";

        //        //    resultTable.Rows.Add(row);
        //        //}


        //        //InsertMethod("junks100", resultTable);

        //        ////-----------------------//

        //        //DataTable dtt = null;
        //        //using (SqlConnection c = new SqlConnection("Data Source=DENNIS-HP\\SQLEXPRESS;Initial Catalog=FMDA_Result; Integrated Security=true"))
        //        //{
        //        //    c.Open();
        //        //    using (SqlDataAdapter a = new SqlDataAdapter("SELECT * FROM dbo.tblHistoricalCorrelationsTT WHERE dbo.tblHistoricalCorrelationsTT.SymbolID='XXXER'", c))
        //        //    {
        //        //        dtt = new DataTable();
        //        //        a.Fill(dtt);
        //        //    }
        //        //}

        //        //DataTable resultTablett = new DataTable();
        //        //resultTablett = dtt;

        //        //for (int r = 0; r < 80; r++)
        //        //{
        //        //    DataRow row = resultTablett.NewRow();
        //        //    row["CorrelatingEntityID"] = r;
        //        //    row["SymbolID"] = "TR" + r;
        //        //    row["StartDate"] = DateTime.Now;
        //        //    row["EndDate"] = DateTime.Now;
        //        //    row["Distance"] = r * 3;
        //        //    row["SourceStockExchange"] = "FakeExchange";
        //        //    row["DestinationStockExchange"] = "TrendoExchange";

        //        //    row["Event"] = "Test";
        //        //    row["HC_ID"] = "junks870";

        //        //    resultTablett.Rows.Add(row);
        //        //}
        //        //taskQueue.Enqueue(resultTable);



        //        ////**

        //        //InsertMethod("junks870", resultTablett);

        //    }

        //    static public void PopulateLookUpX()
        //    {
        //        //pure stock exchange symbols

        //        //Get unique symbol key list

        //        //-----------StockExchange Symbol key gathering----------------//

        //        List<SymbolKeyStockExchange> seSymbolKeysList = new List<SymbolKeyStockExchange>();
        //        try
        //        {
        //            //change all of this to stored procedure later
        //            using (SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["SC_Configuration"].ConnectionString))
        //            {
        //                conn.Open();

        //                //Perhaps extend this feature so that the sysaltfuiles could be run from databases not sitting on the same machine as FMDA_Result
        //                SqlCommand sqlCommand = new SqlCommand("SELECT SymbolID, StockExchange FROM dbo.StockExchangeSymbols", conn);
        //                SqlDataReader reader = sqlCommand.ExecuteReader();

        //                int symbolKeysColumn = reader.GetOrdinal("SymbolID");
        //                int stockExchangeColumn = reader.GetOrdinal("StockExchange");

        //                List<DBConnectionProperties> listDBConnectionProperties = new List<DBConnectionProperties>();

        //                while (reader.Read())
        //                {
        //                    SymbolKeyStockExchange objTemp = new SymbolKeyStockExchange();
        //                    objTemp.SymbolKey = reader.GetString(symbolKeysColumn).Trim();
        //                    objTemp.StockExchange = reader.GetString(stockExchangeColumn).Trim();

        //                    seSymbolKeysList.Add(objTemp);
        //                }
        //              }
        //        }
        //        catch (Exception e)
        //        {
        //            System.Diagnostics.Debug.Write("Msg " + e.ToString());
        //        }


        //        List<SymbolDBProp> symbolDBPropList = new List<SymbolDBProp>();
        //        try
        //        {
        //            //change all of this to stored procedure later
        //            using (SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["SC_Configuration"].ConnectionString))
        //            {
        //                conn.Open();

        //                //Perhaps extend this feature so that the sysaltfuiles could be run from databases not sitting on the same machine as FMDA_Result
        //                SqlCommand sqlCommand = new SqlCommand("SELECT * FROM dbo.[SC_Databases] SCD JOIN dbo.SES_Associated_Databases SESD ON SCD.DID = SESD.DID JOIN dbo.StockExchangeSymbols SE ON SESD.SID = SE.SID", conn);
        //                SqlDataReader reader = sqlCommand.ExecuteReader();

        //                int databaseColumn = reader.GetOrdinal("Database");

        //                int databaseTypeColumn = reader.GetOrdinal("DatabaseType");
        //                int dataSourceColumn = reader.GetOrdinal("DataSource");
        //                int integratedSecurityColumn = reader.GetOrdinal("IntegratedSecurity");

        //                int creationStoredProcedureColumn = reader.GetOrdinal("CreationStoredProcedure");
        //                int scconfigUpdateSPColumn = reader.GetOrdinal("SCConfigUpdateSP");
        //                int connectionStringColumn = reader.GetOrdinal("ConnectionString");

        //                int sidColumn = reader.GetOrdinal("SID");
        //                int didColumn = reader.GetOrdinal("DID");

        //                int symbolIDColumn = reader.GetOrdinal("SymbolID");
        //                int stockExchangeColumn = reader.GetOrdinal("StockExchange");



        //                while (reader.Read())
        //                {
        //                    DBConnectionProperties dbPropItem = new DBConnectionProperties();

        //                    dbPropItem.ConnectionStr = reader.GetString(connectionStringColumn);
        //                    dbPropItem.Database = reader.GetString(databaseColumn);
        //                    dbPropItem.DataSource = reader.GetString(dataSourceColumn);
        //                    dbPropItem.IntegratedSecurity = reader.GetBoolean(integratedSecurityColumn);
        //                    dbPropItem.CreationStoredProcedure = reader.GetString(creationStoredProcedureColumn);
        //                    dbPropItem.SCConfigUpdateSP = reader.GetString(scconfigUpdateSPColumn);
        //                    dbPropItem.DatabaseType = reader.GetInt32(databaseTypeColumn);

        //                    String symbolID = reader.GetString(symbolIDColumn).Trim();
        //                    String stockExchange = reader.GetString(stockExchangeColumn).Trim();

        //                    SymbolDBProp objSymbolDBProp = new SymbolDBProp();
        //                    objSymbolDBProp.SymbolKey = symbolID;
        //                    objSymbolDBProp.DBProp = dbPropItem;
        //                    objSymbolDBProp.StockExchange = stockExchange;

        //                    symbolDBPropList.Add(objSymbolDBProp);

        //                    //SymbolDBProp objSymbolDBPropV = new SymbolDBProp();
        //                    //DBConnectionProperties dbPropItemV = new DBConnectionProperties();

        //                    //dbPropItemV = dbPropItem;
        //                    //dbPropItemV.ConnectionStr = "TEst 2";


        //                    //objSymbolDBPropV.SymbolKey = symbolID;
        //                    //objSymbolDBPropV.DBProp = dbPropItem;//test

        //                    //symbolDBPropList.Add(objSymbolDBPropV);



        //                    //SymbolDBProp objSymbolDBPropT = new SymbolDBProp();
        //                    //DBConnectionProperties dbPropItemT = new DBConnectionProperties();

        //                    //dbPropItemT = dbPropItem;
        //                    //dbPropItemT.ConnectionStr = "TEst 3";

        //                    //objSymbolDBPropT.SymbolKey = symbolID;
        //                    //objSymbolDBPropT.DBProp = dbPropItem;//test

        //                    //symbolDBPropList.Add(objSymbolDBPropT);

        //                    //listDBConnectionProperties.Add(dbPropItem);
        //                }
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            System.Diagnostics.Debug.Write("Msg " + e.ToString());
        //        }

        //        //filtering stage
        //        foreach (var symKey in seSymbolKeysList)
        //        {

        //            var queryAllInstanceOfSpecifiedSymbol = from items in symbolDBPropList
        //                                                    where items.SymbolKey == symKey.SymbolKey && items.StockExchange == symKey.StockExchange
        //                                                    select items;

        //            int count = queryAllInstanceOfSpecifiedSymbol.ToList().Count;

        //            List<DBConnectionProperties> listDBConnectionProperties = new List<DBConnectionProperties>();

        //            foreach (var obj in queryAllInstanceOfSpecifiedSymbol.ToList())
        //            {
        //                listDBConnectionProperties.Add(obj.DBProp);
        //            }

        //            if (count > 0)
        //            {
        //                SymbolKeyStockExchange objSES = new SymbolKeyStockExchange();
        //                objSES.SymbolKey = symKey.SymbolKey;
        //                objSES.StockExchange = symKey.StockExchange;

        //                symbolDBLookUp.Add(new KeyValuePair<SymbolKeyStockExchange, List<DBConnectionProperties>>(objSES, listDBConnectionProperties));
        //            }
        //        }
        //        //-------------------END Stock Exchange Symbol Gathering END---------------------------------//





        //        //test queue population//

        //        DataTable dt = null;
        //        using (SqlConnection c = new SqlConnection("Data Source=DENNIS-HP\\SQLEXPRESS;Initial Catalog=FMDA_Result; Integrated Security=true"))
        //        {
        //            c.Open();
        //            using (SqlDataAdapter a = new SqlDataAdapter("SELECT * FROM dbo.tblHistoricalCorrelationsTT WHERE dbo.tblHistoricalCorrelationsTT.SymbolID='XXXER'", c))
        //            {
        //                dt = new DataTable();
        //                a.Fill(dt);
        //            }
        //        }

        //        DataTable resultTable = new DataTable();
        //        resultTable = dt;

        //        for (int r = 0; r < 100; r++)
        //        {
        //            DataRow row = resultTable.NewRow();
        //            row["CorrelatingEntityID"] = r;
        //            row["SymbolID"] = "TR" + r;
        //            row["StartDate"] = DateTime.Now;
        //            row["EndDate"] = DateTime.Now;
        //            row["Distance"] = r * 3;
        //            row["SourceStockExchange"] = "FakeExchange";
        //            row["DestinationStockExchange"] = "TrendoExchange";

        //            row["Event"] = "Test";
        //            row["HC_ID"] = "junks100";

        //            resultTable.Rows.Add(row);
        //        }


        //        InsertMethod("junks100", resultTable);

        //        //-----------------------//

        //        DataTable dtt = null;
        //        using (SqlConnection c = new SqlConnection("Data Source=DENNIS-HP\\SQLEXPRESS;Initial Catalog=FMDA_Result; Integrated Security=true"))
        //        {
        //            c.Open();
        //            using (SqlDataAdapter a = new SqlDataAdapter("SELECT * FROM dbo.tblHistoricalCorrelationsTT WHERE dbo.tblHistoricalCorrelationsTT.SymbolID='XXXER'", c))
        //            {
        //                dtt = new DataTable();
        //                a.Fill(dtt);
        //            }
        //        }

        //        DataTable resultTablett = new DataTable();
        //        resultTablett = dtt;

        //        for (int r = 0; r < 80; r++)
        //        {
        //            DataRow row = resultTablett.NewRow();
        //            row["CorrelatingEntityID"] = r;
        //            row["SymbolID"] = "TR" + r;
        //            row["StartDate"] = DateTime.Now;
        //            row["EndDate"] = DateTime.Now;
        //            row["Distance"] = r * 3;
        //            row["SourceStockExchange"] = "FakeExchange";
        //            row["DestinationStockExchange"] = "TrendoExchange";

        //            row["Event"] = "Test";
        //            row["HC_ID"] = "junks870";

        //            resultTablett.Rows.Add(row);
        //        }
        //        //taskQueue.Enqueue(resultTable);



        //        //**

        //        InsertMethod("junks870", resultTablett);

        //        int hu = 99;


        //    }

        //    static public void InitialiseDataTableSchema()
        //    {
        //        DataTable hcDT = null;
        //        using (SqlConnection c = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["SC_Configuration"].ConnectionString))
        //        {
        //            c.Open();
        //            using (SqlDataAdapter a = new SqlDataAdapter("SELECT * FROM dbo.tblHistoricalCorrelations WHERE dbo.tblHistoricalCorrelations.SymbolID='XXXER'", c))
        //            {
        //                hcDT = new DataTable();
        //                a.Fill(hcDT);
        //            }
        //            if (hcDT != null) dataTables.Add(DatabaseType.HistoricalCorrelations, hcDT);
        //        }

        //        DataTable rcDT = null;
        //        using (SqlConnection c = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["SC_Configuration"].ConnectionString))
        //        {
        //            c.Open();
        //            using (SqlDataAdapter a = new SqlDataAdapter("SELECT * FROM dbo.tblRangeCorrelations WHERE dbo.tblRangeCorrelations.SymbolID='XXXER'", c))
        //            {
        //                rcDT = new DataTable();
        //                a.Fill(rcDT);
        //            }
        //            if (rcDT != null) dataTables.Add(DatabaseType.RangeCorrelations, rcDT);
        //        }


        //        //DataTable patternsDT = null;
        //        //using (SqlConnection c = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["SC_Configuration"].ConnectionString))
        //        //{
        //        //    c.Open();
        //        //    using (SqlDataAdapter a = new SqlDataAdapter("SELECT * FROM dbo.tblHistoricalCorrelations WHERE dbo.tblHistoricalCorrelations.SymbolID='XXXER'", c))
        //        //    {
        //        //        patternsDT = new DataTable();
        //        //        a.Fill(patternsDT);
        //        //    }
        //        //}

        //        DataTable stockExchangeDT = null;
        //        using (SqlConnection c = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["SC_Configuration"].ConnectionString))
        //        {
        //            c.Open();
        //            using (SqlDataAdapter a = new SqlDataAdapter("SELECT * FROM dbo.DayTradeSummaries WHERE dbo.DayTradeSummaries.SymbolID='XXXER'", c))
        //            {
        //                stockExchangeDT = new DataTable();
        //                a.Fill(stockExchangeDT);
        //            }
        //            if (stockExchangeDT != null) dataTables.Add(DatabaseType.StockExchange, stockExchangeDT);
        //        }

        //    }

        //    static public bool SelectMethod(String statement, String symbol)
        //    {
        //        //*Perform lookup

        //        String stockExchange = "AMEX";

        //        //*Query associated tables
        //        var dbpropsFirst = symbolDBLookUp.FirstOrDefault().Value.FirstOrDefault();
        //        int relatedDBCount = symbolDBLookUp.FirstOrDefault().Value.Count;

        //        //CheckDatabaseSize(dbpropsFirst, relatedDBCount);//temp

        //        //var databaseList = symbolDBLookUp.Find(x => x.Key == symbol);
        //        //var databaseList = symbolDBLookUp.Find(x => x.Key.SymbolKey == symbol);

        //         var databaseList = from itemObj in symbolDBLookUp where itemObj.Key.SymbolKey == symbol && itemObj.Key.StockExchange == stockExchange
        //                            select symbolDBLookUp;


        //        String compliedCommandStr = "";
        //        int count = 0;


        //        //change this ti use LINQ query
        //        foreach (var symb in symbolDBLookUp)
        //        {
        //            if (symb.Key.SymbolKey == symbol)
        //            {
        //                for (int i = 0; i < symb.Value.Count; i++)
        //                {
        //                    var dbProp = symb.Value[i];

        //                    String stageStr = "SELECT * FROM " + dbProp.Database + ".dbo.DayTradeSummaries WHERE " + dbProp.Database + ".dbo.DayTradeSummaries.SymbolID = '" + symbol + "' ORDER BY " + dbProp.Database + ".dbo.DayTradeSummaries.Date DESC; ";
        //                    compliedCommandStr += stageStr;
        //                }
        //                break;
        //            }
        //        }


        //        using (var connection = new SqlConnection("Data Source=DENNIS-HP\\SQLEXPRESS;Initial Catalog=DBM_AMEX_STOCK_1; Integrated Security=true"))
        //        using (var command = connection.CreateCommand())
        //        {
        //            String symbolTemp = "ACU";

        //            connection.Open();
        //            //SELECT * FROM [dbo].[DayTradeSummaries] WHERE dbo.DayTradeSummaries.SymbolID = 'APP' ORDER BY dbo.DayTradeSummaries.Date DESC

        //            command.CommandText = compliedCommandStr;

        //           /* command.CommandText = "SELECT * FROM DBM_AMEX_STOCK_1.dbo.DayTradeSummaries WHERE DBM_AMEX_STOCK_1.dbo.DayTradeSummaries.SymbolID = '" + symbolTemp + "' ORDER BY DBM_AMEX_STOCK_1.dbo.DayTradeSummaries.Date DESC; "
        //            + "SELECT * FROM DBM_AMEX_STOCK_2.dbo.DayTradeSummaries WHERE DBM_AMEX_STOCK_2.dbo.DayTradeSummaries.SymbolID = '" + symbolTemp + "' ORDER BY DBM_AMEX_STOCK_2.dbo.DayTradeSummaries.Date DESC;"
        //            + "SELECT * FROM DBM_AMEX_STOCK_3.dbo.DayTradeSummaries WHERE DBM_AMEX_STOCK_3.dbo.DayTradeSummaries.SymbolID = '" + symbolTemp + "' ORDER BY DBM_AMEX_STOCK_3.dbo.DayTradeSummaries.Date DESC;";


        //            */


        //            //command.CommandText = "SELECT * FROM DBM_AMEX_STOCK_1.dbo.DayTradeSummaries WHERE dbo.DayTradeSummaries.SymbolID = 'APP' ORDER BY dbo.DayTradeSummaries.Date DESC; SELECT * FROM DBM_AMEX_STOCK_2.dbo.DayTradeSummaries WHERE dbo.DayTradeSummaries.SymbolID = 'APP' ORDER BY dbo.DayTradeSummaries.Date DESC; SELECT * FROM DBM_AMEX_STOCK_3.dbo.DayTradeSummaries WHERE dbo.DayTradeSummaries.SymbolID = 'APP' ORDER BY dbo.DayTradeSummaries.Date DESC;";



        //            using (var reader = command.ExecuteReader())
        //            {
        //                do
        //                {
        //                    while (reader.Read())
        //                    {
        //                        Console.WriteLine(reader.GetInt32(0));

        //                        count++;
        //                    }
        //                    Console.WriteLine("--next command--");
        //                } while (reader.NextResult());

        //            }
        //        }

        //        return false;
        //    }

        //    static private bool CheckDatabaseSize(DBConnectionProperties dbproperties, int relatedDBCount, ref DBConnectionProperties newDBproperties, bool bSymbolKeyExist, QueueTask queueItem)
        //    {
        //        bool newDatabaseCreated = false;
        //        //Create Database

        //        int databaseSize = 0;

        //        DBConnectionProperties lastUsedDB = new DBConnectionProperties();
        //        lastUsedDB = dbproperties;

        //        if (bSymbolKeyExist)
        //        {
        //            try
        //            {
        //                using (SqlConnection conn = new SqlConnection(lastUsedDB.ConnectionStr))
        //                {
        //                    conn.Open();

        //                    SqlCommand sqlCommand = new SqlCommand("SELECT size FROM master.dbo.sysaltfiles WHERE name = '" + lastUsedDB.Database + "'", conn);

        //                    SqlDataReader reader = sqlCommand.ExecuteReader();
        //                    int sizeItem = reader.GetOrdinal("size");


        //                    while (reader.Read())
        //                    {
        //                        databaseSize = reader.GetInt32(sizeItem);
        //                    }
        //                }
        //            }
        //            catch (Exception e)
        //            {
        //                System.Diagnostics.Debug.Write("Msg " + e.ToString());
        //            }
        //        }
        //        else
        //        {

        //            // get all databases of the given type

        //            //get smallest sized one

        //            //is it within limit or out of it



        //            List<String> databaseList = new List<String>();

        //            try
        //            {
        //                //change all of this to stored procedure later
        //                using (SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["SC_Configuration"].ConnectionString))
        //                {
        //                    conn.Open();

        //                    //Perhaps extend this feature so that the sysaltfuiles could be run from databases not sitting on the same machine as FMDA_Result
        //                    SqlCommand sqlCommand = new SqlCommand("SELECT TOP 1 S.name, S.size, SCD.[Database], SCD.DatabaseType, SCD.DataSource, SCD.IntegratedSecurity, SCD.CreationStoredProcedure, SCD.SCConfigUpdateSP, SCD.ConnectionString FROM master.dbo.sysaltfiles S "
        //                    + "JOIN SC_Configuration.dbo.SC_Databases SCD ON S.name = SCD.[Database] WHERE SCD.DatabaseType = " + (int)queueItem.DatabaseType + " ORDER BY S.size ASC", conn);

        //                    SqlDataReader reader = sqlCommand.ExecuteReader();

        //                    int sizeItem = reader.GetOrdinal("size");
        //                    int databaseColumn = reader.GetOrdinal("Database");

        //                    int databaseTypeColumn = reader.GetOrdinal("DatabaseType");
        //                    int dataSourceColumn = reader.GetOrdinal("DataSource");
        //                    int integratedSecurityColumn = reader.GetOrdinal("IntegratedSecurity");

        //                    int creationStoredProcedureColumn = reader.GetOrdinal("CreationStoredProcedure");
        //                    int scconfigUpdateSPColumn = reader.GetOrdinal("SCConfigUpdateSP");
        //                    int connectionStringColumn = reader.GetOrdinal("ConnectionString");


        //                    while (reader.Read())
        //                    {
        //                        databaseSize = reader.GetInt32(sizeItem);

        //                        lastUsedDB.ConnectionStr = reader.GetString(connectionStringColumn);
        //                        lastUsedDB.Database = reader.GetString(databaseColumn);
        //                        lastUsedDB.DataSource = reader.GetString(dataSourceColumn);
        //                        lastUsedDB.IntegratedSecurity = reader.GetBoolean(integratedSecurityColumn);
        //                        lastUsedDB.CreationStoredProcedure = reader.GetString(creationStoredProcedureColumn);
        //                        lastUsedDB.SCConfigUpdateSP = reader.GetString(scconfigUpdateSPColumn);

        //                        lastUsedDB.DatabaseType = reader.GetInt32(databaseTypeColumn);

        //                    }

        //                    //int databaseColumn = reader.GetOrdinal("Database");

        //                    //while (reader.Read())
        //                    //{
        //                    //    String databaseName = reader.GetString(databaseColumn);
        //                    //    databaseList.Add(databaseName);
        //                    //}
        //                }
        //            }
        //            catch (Exception e)
        //            {
        //                System.Diagnostics.Debug.Write("Msg " + e.ToString());
        //            }
        //        }



        //        //if (databaseSize >= 258747392) //temp
        //        if (databaseSize >= 27392) //388224 currently based on FMDA_Result
        //        {
        //            //SqlConnection myConn = new SqlConnection("Server=" + dbproperties.DataSource + ";Integrated security=True;database=master");

        //            SqlConnection myConn = new SqlConnection("Server=" + lastUsedDB.DataSource + ";Integrated security=True;database=SC_Configuration");

        //            int total = relatedDBCount + 1;

        //            String newDBName = lastUsedDB.Database + "_" + total;

        //            //str = "CREATE DATABASE " + newDBName + " ON PRIMARY " +
        //            //    "(NAME = " + newDBName + "_Data, " +
        //            //    "FILENAME = 'C:\\Program Files\\Microsoft SQL Server\\MSSQL10.SQLEXPRESS\\MSSQL\\DATA\\" + newDBName + ".mdf', " +
        //            //    "SIZE = 5MB, MAXSIZE = 10MB, FILEGROWTH = 10%) " +
        //            //    "LOG ON (NAME = " + newDBName + "_Log, " +
        //            //    "FILENAME = 'C:\\Program Files\\Microsoft SQL Server\\MSSQL10.SQLEXPRESS\\MSSQL\\DATA\\" + newDBName + ".ldf', " +
        //            //    "SIZE = 1MB, " +
        //            //    "MAXSIZE = 5MB, " +
        //            //    "FILEGROWTH = 10%)";



        //            SqlCommand myCommand = new SqlCommand(lastUsedDB.CreationStoredProcedure, myConn);
        //            myCommand.CommandType = CommandType.StoredProcedure;

        //            SqlParameter sqlparam = myCommand.Parameters.Add("@bd_name", SqlDbType.NVarChar, 50);
        //            sqlparam.Value = newDBName;

        //            try
        //            {
        //                myConn.Open();
        //                myCommand.ExecuteNonQuery();
        //                System.Console.WriteLine("DataBase is Created Successfully");

        //                //Data Source=DENNIS-HP\SQLEXPRESS;Initial Catalog=DBM_AMEX_STOCK_1; Integrated Security=true

        //                newDBproperties.ConnectionStr = "Data Source=" + lastUsedDB.DataSource + ";Initial Catalog=" + newDBName + ";Integrated Security="+ lastUsedDB.IntegratedSecurity.ToString();
        //                newDBproperties.Database = newDBName;
        //                newDBproperties.DataSource = lastUsedDB.DataSource;
        //                newDBproperties.IntegratedSecurity = lastUsedDB.IntegratedSecurity;
        //                newDBproperties.CreationStoredProcedure = lastUsedDB.CreationStoredProcedure;
        //                newDBproperties.SCConfigUpdateSP = lastUsedDB.SCConfigUpdateSP;
        //                newDBproperties.DatabaseType = lastUsedDB.DatabaseType;

        //                //fix integrited security


        //                newDatabaseCreated = true;
        //            }
        //            catch (System.Exception ex)
        //            {
        //                System.Console.WriteLine(ex.ToString(), "MyProgram");
        //            }
        //            finally
        //            {
        //                if (myConn.State == ConnectionState.Open)
        //                {
        //                    myConn.Close();
        //                }
        //            }
        //        }
        //        else
        //        {
        //            if (bSymbolKeyExist == false)
        //            {
        //                //use case
        //                //original dbs exist and symbols table exist
        //                //however no association in the linker tables
        //                //yet database type is known and database is good size
        //                //will treat it as if symbolkey does not exist in lookup due to join in fetching from db ()
        //                //update linker table and symbolKeyTable
        //                //

        //                newDBproperties = lastUsedDB;
        //                bool bUpdateLinkerTableAndMore = true;

        //                return bUpdateLinkerTableAndMore;
        //            }
        //            System.Console.WriteLine("Database size within limit");
        //        }
        //        return newDatabaseCreated;
        //    }

        //    static public bool InsertMethod(String symbolKey, DataTable dt)
        //    {
        //        //taskQueue.Enqueue(new KeyValuePair<String, DataTable>(symbolKey, dt));

        //        //insertions must be in the form of a datatable a group of records belonging to the specified symbol
        //        QueueTask taskItem;
        //        taskItem.DatabaseType = DatabaseType.HistoricalCorrelations;
        //        taskItem.StockExchange = "AMEX";
        //        taskItem.Dt = dt;
        //        taskItem.SymbolKey = symbolKey;

        //        taskQueue.Enqueue(taskItem);

        //        return false;
        //    }

        //    static public bool ProcessQueueTest()
        //    {
        //        while (true)
        //        {
        //            if (taskQueue.Count > 0)
        //            {
        //                var taskItem = (QueueTask)taskQueue.Dequeue();


        //                var databaseList = from itemObj in symbolDBLookUp
        //                                   where itemObj.Key.SymbolKey == taskItem.SymbolKey && itemObj.Key.StockExchange == taskItem.StockExchange
        //                                   select symbolDBLookUp;

        //                //out of this group databaseList gettaskItem.SymbolKey
        //                DBConnectionProperties lastUsed = new DBConnectionProperties();

        //                bool bSymbolKeyExist = true;
        //                int relatedDBCount = databaseList.Count();

        //                if (relatedDBCount > 0)
        //                {
        //                    foreach (var kvp in databaseList.FirstOrDefault())
        //                    {
        //                        String symkey = kvp.Key.SymbolKey;
        //                        lastUsed = kvp.Value.LastOrDefault();
        //                    }
        //                }
        //                else
        //                {
        //                    bSymbolKeyExist = false;
        //                }



        //                    //KeyValuePair<SymbolKeyStockExchange, List<DBConnectionProperties>> lastUsedDBKvp = databaseList.FirstOrDefault();

        //                    //*Perform lookup
        //                    //var dbpropsFirst = symbolDBLookUp.FirstOrDefault().Value.FirstOrDefault();

        //                    //*Query associated tables

        //                    //int relatedDBCount = databaseList.Count();
        //                    //bool bSymbolKeyExist = relatedDBCount > 0 ? true : false;



        //                    //int relatedDBCount = symbolDBLookUp.FirstOrDefault().Value.Count;

        //                    DBConnectionProperties dbcon = new DBConnectionProperties();

        //                    if (CheckDatabaseSize(lastUsed, relatedDBCount, ref dbcon, bSymbolKeyExist, taskItem))
        //                    {
        //                        //Process

        //                        ViewDataTable(taskItem.Dt);
        //                        //insert into the newly created/extended database






        //                        String symbolTest = taskItem.SymbolKey; // remove was for testing
        //                        String stockExchangeTest = taskItem.StockExchange;

        //                        //Update SC_Configuration database with the latest extended database
        //                        if (true)//if Dataconsumer related then get stockExchange and include it in the insert
        //                        {
        //                            try
        //                            {
        //                                //this is part of code can be used for all db extenstions now

        //                                String connectionStr = System.Configuration.ConfigurationManager.ConnectionStrings["SC_Configuration"].ConnectionString;

        //                                using (SqlConnection myConn = new SqlConnection(connectionStr))
        //                                {
        //                                    SqlCommand myCommand = new SqlCommand(dbcon.SCConfigUpdateSP, myConn);//config sp being called
        //                                    myCommand.CommandType = CommandType.StoredProcedure;

        //                                    SqlParameter sqlparamConnectionStr = myCommand.Parameters.Add("@ConnectionStr", SqlDbType.NVarChar, 180);
        //                                    sqlparamConnectionStr.Value = dbcon.ConnectionStr;

        //                                    SqlParameter sqlparamDataSource = myCommand.Parameters.Add("@DataSource", SqlDbType.NVarChar, 60);
        //                                    sqlparamDataSource.Value = dbcon.DataSource;

        //                                    SqlParameter sqlparamDatabase = myCommand.Parameters.Add("@Database", SqlDbType.NVarChar, 60);
        //                                    sqlparamDatabase.Value = dbcon.Database;

        //                                    SqlParameter sqlparamIntegratedSecurity = myCommand.Parameters.Add("@IntegratedSecurity", SqlDbType.Bit);
        //                                    sqlparamIntegratedSecurity.Value = dbcon.IntegratedSecurity;

        //                                    SqlParameter sqlparamCreationStoredProcedure = myCommand.Parameters.Add("@CreationStoredProcedure", SqlDbType.NVarChar, 50);
        //                                    sqlparamCreationStoredProcedure.Value = dbcon.CreationStoredProcedure;

        //                                    SqlParameter sqlparamSCConfigUpdateSP = myCommand.Parameters.Add("@SCConfigUpdateSP", SqlDbType.NVarChar, 60);
        //                                    sqlparamSCConfigUpdateSP.Value = dbcon.SCConfigUpdateSP;


        //                                    SqlParameter sqlparamSymbolID = myCommand.Parameters.Add("@SymbolID", SqlDbType.NVarChar, 20);
        //                                    sqlparamSymbolID.Value = symbolTest;

        //                                    SqlParameter sqlparamStockExchange = myCommand.Parameters.Add("@StockExchange", SqlDbType.NVarChar, 20);
        //                                    sqlparamStockExchange.Value = stockExchangeTest;

        //                                    SqlParameter sqlparamDatabaseType = myCommand.Parameters.Add("@DatabaseType", SqlDbType.Int);
        //                                    sqlparamDatabaseType.Value = dbcon.DatabaseType;

        //                                    myConn.Open();

        //                                    myCommand.ExecuteNonQuery();
        //                                    System.Console.WriteLine("SC_Configuration Database Updated Successfully");
        //                                }

        //                            }
        //                            catch (System.Exception ex)
        //                            {
        //                                System.Console.WriteLine(ex.ToString(), "MyProgram");
        //                            }
        //                        }
        //                        else
        //                        {
        //                            //may be redundant
        //                            ViewDataTable(taskItem.Dt);
        //                        }
        //                    }
        //                    else
        //                    {
        //                        ViewDataTable(taskItem.Dt);

        //                        // System.Console.WriteLine(taskItem.ToString(), "key: " + taskItem.Key, "value: " + taskItem.Value);
        //                    }
        //                }

        //        }

        //            //return false;  
        //    }

        //    static public bool ProcessQueue()
        //    {
        //        do
        //        {
        //            //var taskItem = (KeyValuePair<String, DataTable>)taskQueue.Dequeue();

        //            //current dbconnection list has sc_config as part of list this is wrong

        //            var taskItem = (QueueTask)taskQueue.Dequeue();


        //            var databaseList = from itemObj in symbolDBLookUp
        //                               where itemObj.Key.SymbolKey == "ACU" && itemObj.Key.StockExchange == taskItem.StockExchange
        //                               select symbolDBLookUp;

        //            //out of this group databaseList gettaskItem.SymbolKey

        //            KeyValuePair<SymbolKeyStockExchange, List<DBConnectionProperties>> iu = databaseList.FirstOrDefault().LastOrDefault();



        //            //*Perform lookup
        //            var dbpropsFirst = symbolDBLookUp.FirstOrDefault().Value.FirstOrDefault();
        //            bool bSymbolKeyExist = true;

        //            //*Query associated tables

        //            int relatedDBCount = symbolDBLookUp.FirstOrDefault().Value.Count;

        //            DBConnectionProperties dbcon = new DBConnectionProperties();

        //            if (CheckDatabaseSize(dbpropsFirst, relatedDBCount, ref dbcon, bSymbolKeyExist, taskItem))
        //            {
        //                //Process

        //                ViewDataTable(taskItem.Dt);
        //                //insert into the newly created/extended database

        //                String symbolTest = "GLU"; // remove was for testing
        //                String stockExchangeTest = "AMEX";

        //                //Update SC_Configuration database with the latest extended database
        //                if (true)//if Dataconsumer related then get stockExchange and include it in the insert
        //                {
        //                    try
        //                    {
        //                        //this is part of code can be used for all db extenstions now

        //                        String connectionStr = System.Configuration.ConfigurationManager.ConnectionStrings["SC_Configuration"].ConnectionString;

        //                        using (SqlConnection myConn = new SqlConnection(connectionStr))
        //                        {
        //                            SqlCommand myCommand = new SqlCommand(dbcon.SCConfigUpdateSP, myConn);//config sp being called
        //                            myCommand.CommandType = CommandType.StoredProcedure;

        //                            SqlParameter sqlparamConnectionStr = myCommand.Parameters.Add("@ConnectionStr", SqlDbType.NVarChar, 180);
        //                            sqlparamConnectionStr.Value = dbcon.ConnectionStr;

        //                            SqlParameter sqlparamDataSource = myCommand.Parameters.Add("@DataSource", SqlDbType.NVarChar, 60);
        //                            sqlparamDataSource.Value = dbcon.DataSource;

        //                            SqlParameter sqlparamDatabase = myCommand.Parameters.Add("@Database", SqlDbType.NVarChar, 60);
        //                            sqlparamDatabase.Value = dbcon.Database;

        //                            SqlParameter sqlparamIntegratedSecurity = myCommand.Parameters.Add("@IntegratedSecurity", SqlDbType.Bit);
        //                            sqlparamIntegratedSecurity.Value = dbcon.IntegratedSecurity;

        //                            SqlParameter sqlparamCreationStoredProcedure = myCommand.Parameters.Add("@CreationStoredProcedure", SqlDbType.NVarChar, 50);
        //                            sqlparamCreationStoredProcedure.Value = dbcon.CreationStoredProcedure;

        //                            SqlParameter sqlparamSCConfigUpdateSP = myCommand.Parameters.Add("@SCConfigUpdateSP", SqlDbType.NVarChar, 60);
        //                            sqlparamSCConfigUpdateSP.Value = dbcon.SCConfigUpdateSP;


        //                            SqlParameter sqlparamSymbolID = myCommand.Parameters.Add("@SymbolID", SqlDbType.NVarChar, 20);
        //                            sqlparamSymbolID.Value = symbolTest;

        //                            SqlParameter sqlparamStockExchange = myCommand.Parameters.Add("@StockExchange", SqlDbType.NVarChar, 20);
        //                            sqlparamStockExchange.Value = stockExchangeTest;

        //                            SqlParameter sqlparamDatabaseType = myCommand.Parameters.Add("@DatabaseType", SqlDbType.Int);
        //                            sqlparamDatabaseType.Value = dbcon.DatabaseType;

        //                            myConn.Open();

        //                            myCommand.ExecuteNonQuery();
        //                            System.Console.WriteLine("SC_Configuration Database Updated Successfully");
        //                        }

        //                    }
        //                    catch (System.Exception ex)
        //                    {
        //                        System.Console.WriteLine(ex.ToString(), "MyProgram");
        //                    }
        //                }
        //                else
        //                {
        //                    //may be redundant
        //                    ViewDataTable(taskItem.Dt);
        //                }
        //            }
        //            else
        //            {
        //                ViewDataTable(taskItem.Dt);

        //               // System.Console.WriteLine(taskItem.ToString(), "key: " + taskItem.Key, "value: " + taskItem.Value);
        //            }
        //        } while (taskQueue.Count != 0);


        //        int hyu = 2;
        //        //var databaseList = symbolDBLookUp.Find(x => x.Key == symbol)
        //        return false;
        //    }

        //    static public void InsertIntoDatabase(DataTable dtView, DatabaseType databaseType, DBConnectionProperties dbcon)
        //    {
        //        //DataTable dt = null;
        //        //dt = dataTables[databaseType].Clone();

        //        //find a way to insert datatable without the need to select .

        //        switch (databaseType)
        //        {
        //            case DatabaseType.HistoricalCorrelations:
        //                {
        //                    //using (SqlDataAdapter dataAdapter = new SqlDataAdapter("SELECT * FROM dbo.tblHistoricalCorrelations", System.Configuration.ConfigurationManager.ConnectionStrings["FMDA_Result"].ConnectionString))
        //                    //{
        //                    //    //dataAdapter.SelectCommand 

        //                    //    SqlCommand jn;
        //                    //    jn.

        //                    //    new SqlCommandBuilder(dataAdapter);
        //                    //    dataAdapter.Update(resultTable);
        //                    //}
        //                }

        //            case DatabaseType.RangeCorrelations:
        //                {

        //                }

        //            case DatabaseType.StockExchange:
        //                {

        //                }

        //            case DatabaseType.Patterns:
        //                {

        //                }
        //        }
        //    }


        //    static public void ViewDataTable(DataTable dtView)
        //    {
        //        System.IO.StringWriter sw;
        //        string output;

        //        foreach (DataRow row in dtView.Rows)
        //        {
        //            sw = new System.IO.StringWriter();
        //            // Loop through each column.
        //            foreach (DataColumn col in dtView.Columns)
        //            {
        //                // Output the value of each column's data.
        //                sw.Write(row[col].ToString() + ",");
        //            }
        //            output = sw.ToString();
        //            // Trim off the trailing ", ", so the output looks correct.
        //            if (output.Length > 2)
        //            {
        //                output = output.Substring(0, output.Length - 2);
        //            }
        //            // Display the row in the console window.
        //            Console.WriteLine(output);
        //        } //

        //    }
        //}


        //internal struct DBConnectionProperties
        //{
        //    internal String ConnectionStr;
        //    internal String DataSource;
        //    internal String Database;
        //    internal bool IntegratedSecurity;
        //    internal String CreationStoredProcedure;
        //    internal String SCConfigUpdateSP;
        //    internal int DatabaseType;
        //}

        ////this will eventually be the datacontract
        //internal struct QueueTask
        //{
        //    internal String SymbolKey;
        //    internal String StockExchange;
        //    internal DataTable Dt;
        //    internal DatabaseType DatabaseType;
        //}

        //public enum DatabaseType
        //{
        //    StockExchange = 0,
        //    HistoricalCorrelations,
        //    RangeCorrelations,
        //    Patterns
        //}

        //internal struct SymbolDBProp
        //{
        //    internal String SymbolKey;
        //    internal DBConnectionProperties DBProp;
        //    internal String StockExchange;
        //}

        //internal struct SymbolKeyStockExchange
        //{
        //    internal String SymbolKey;
        //    internal String StockExchange;
        //}

    }
}
