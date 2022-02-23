using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace FileService
{
    public enum MODE
    {
        Live = 0,
        Test = 1,
        LiveTest = 2
    }

    public class DatabaseImporter
    {
        static public MODE Mode;

        public void ImportHistoricalCorrelations(String dir, String fileName, ref int rowsCommitted, String selector)
        {
            String fullpath = dir + "\\" + fileName;
            int rowsAffected = 0;

            SqlParameter outputIdParam = new SqlParameter("@ROWSAFFECTED", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };

            try
            {
                using (SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings[selector].ConnectionString))
                {
                    conn.Open();
                    SqlCommand sqlCommand = new SqlCommand("proc_BulkInsertCorrelations", conn);
                    sqlCommand.CommandType = CommandType.StoredProcedure;

                    sqlCommand.Parameters.Add(outputIdParam);
                    sqlCommand.CommandTimeout = 0;
                    rowsAffected = sqlCommand.ExecuteNonQuery();

                    rowsCommitted = Convert.ToInt32(outputIdParam.Value);
                }
            }
            catch (Exception ex)
            {
                Library.WriteErrorLog(DateTime.Now + " :: " + ex.ToString());
                LogHelper.Error("ImportHistoricalCorrelations", ex);
                rowsCommitted = Convert.ToInt32(outputIdParam.Value);

                throw; 
            }
        }

        public void ImportRawData(String fileName, String stockExchange, ref int rowsCommitted, String selector, String dataType)
        {
            int rowsAffected = 0;
            
            string storedProc = "";
            switch (dataType)
            {
                case "EventData": { storedProc = "proc_BulkInsertEconomicValues"; } break;
                case "EventDesc": { storedProc = "proc_BulkInsertEconomicEvents"; } break;
                case "KeyPers": { storedProc = "proc_BulkInsertEventKeyPersonnel"; } break;
                default: { storedProc = "proc_BulkInsertTrades"; } break;
            }



            SqlParameter outputIdParam = new SqlParameter("@ROWSAFFECTED", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };

            try
            {
                String directoryOutput = "";

                using (SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings[selector].ConnectionString))
                {
                    conn.Open();
                    SqlCommand sqlCommand = new SqlCommand(storedProc, conn);
                    sqlCommand.CommandType = CommandType.StoredProcedure;

                    sqlCommand.Parameters.AddWithValue("@PATH", fileName);

                    sqlCommand.Parameters.Add(outputIdParam);
                    sqlCommand.CommandTimeout = 0;
                    sqlCommand.ExecuteNonQuery();

                    rowsCommitted = Convert.ToInt32(outputIdParam.Value);
                }
            }
            catch (Exception ex)
            {
                Library.WriteErrorLog(DateTime.Now + " :: " + ex.ToString());
                rowsCommitted = Convert.ToInt32(outputIdParam.Value);
                throw;
            }
        }

        private String GetReportHtml(bool bRaw)
        {
            String selector = "JobsManager";
            String masterContent = "";

            switch (DatabaseImporter.Mode)
            {
                case MODE.Test:
                    {
                        selector += "_TEST";
                    }
                    break;

                case MODE.Live:
                    {
                        selector += "_LIVE";
                    }
                    break;
            }

            var style = "<style>table, td, th { border: 1px solid green;} th {background-color: green;color: white;}table {border-collapse: collapse;}table {width: 100%;}th {height: 50px;}td {padding: 25px;}</style>";

            masterContent += "<html><head>" + style + "</head><body><table style='width:100%; border: 1px solid black'>";

            try
            {
                //Import Log
                using (SqlConnection con = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings[selector].ToString()))
                {
                    String proc = bRaw ? "proc_ViewFileImports" : "proc_ViewComputedFileImports";

                    SqlCommand sqlCommand = new SqlCommand(proc, con);
                    con.Open();

                    String startDateTimeStr = String.Format("{0}-{1}-{2} {3}:{4}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute);
                    String endDateTimeStr = String.Format("{0}-{1}-{2} {3}:{4}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day + 1, DateTime.Now.Hour, DateTime.Now.Minute);


                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    sqlCommand.Parameters.AddWithValue("@ImportStartTime", startDateTimeStr);
                    sqlCommand.Parameters.AddWithValue("@ImportEndTime", endDateTimeStr);

                    var sqlReader = sqlCommand.ExecuteReader();

                    masterContent += "<tr>";
                    masterContent += "<th>FileImportID</th>";
                    // masterContent += "<th>ErrorMsg</th>";
                    masterContent += "<th>CSVRecordCount</th>";
                    masterContent += "<th>ImportedRecordCount</th>";
                    masterContent += "<th>PercentageSuccess</th>";
                    masterContent += "<th>PercentageFailure</th>";
                    masterContent += "<th>ImportStartTime</th>";
                    masterContent += "<th>ImportEndTime</th>";

                    masterContent += "<th>FileName</th>";
                    masterContent += "<th>ImportCompleted</th>";
                    masterContent += "<th>DataType</th>";

                    if (bRaw)
                    {
                        masterContent += "<th>DownloadCompleted</th>";
                    }

                    masterContent += "</tr>";

                    Dictionary<String, String> errorList = new Dictionary<String, String>();

                    while (sqlReader.Read())
                    {
                        if (String.IsNullOrEmpty(sqlReader["ErrorMsg"].ToString()) == false)
                        {
                            String errorMsgTemp = "";
                            if (errorList.TryGetValue(sqlReader["FileName"].ToString(), out errorMsgTemp) == false)
                            {
                                errorList.Add(sqlReader["FileName"].ToString(), sqlReader["ErrorMsg"].ToString());
                            }
                        }

                        masterContent += "<tr>";
                        masterContent += "<td>" + sqlReader["FileImportID"].ToString() + "</td>";
                        //masterContent += "<td>" + sqlReader["ErrorMsg"].ToString() + "</td>";
                        masterContent += "<td>" + sqlReader["CSVRecordCount"].ToString() + "</td>";
                        masterContent += "<td>" + sqlReader["ImportedRecordCount"].ToString() + "</td>";
                        masterContent += "<td>" + sqlReader["PercentageSuccess"].ToString() + "</td>";
                        masterContent += "<td>" + sqlReader["PercentageFailure"].ToString() + "</td>";
                        masterContent += "<td>" + sqlReader["ImportStartTime"].ToString() + "</td>";
                        masterContent += "<td>" + sqlReader["ImportEndTime"].ToString() + "</td>";

                        masterContent += "<td>" + sqlReader["FileName"].ToString() + "</td>";
                        masterContent += "<td>" + sqlReader["ImportCompleted"].ToString() + "</td>";
                        masterContent += "<td>" + sqlReader["DataType"].ToString() + "</td>";

                        if (bRaw)
                        {
                            masterContent += "<td>" + sqlReader["DownloadCompleted"].ToString() + "</td>";
                        }

                        masterContent += "</tr>";
                    }

                    if (errorList.Any())
                    {
                        String miniTable = "<br/><br/><br/><table><tr><th>FileName</th><th>Error Msg</th></tr><tr>";

                        foreach (var i in errorList)
                        {
                            miniTable += "<td>" + i.Key + "</td>";
                            miniTable += "<td>" + i.Value + "</td>";

                        }
                        miniTable += "</tr></table>";

                        masterContent += "</table>" + miniTable + "</body></html>";
                    }
                    else
                    {
                        masterContent += "</table></body></html>";
                    }
                }
            }
            catch (Exception ex)
            {
                Library.WriteErrorLog(DateTime.Now + " :: " + ex.ToString());
            }
            return masterContent;
        }

        public void EmailNotificationCheck(List<String> exchange, bool bRaw)
        {
            try
            {

                var message = new StringBuilder();
                string title = "";
                string importLog = "";

                String listing = "";
                foreach (var item in exchange)
                {
                    listing += item + "; ";
                }


                if (bRaw)
                {
                    message.AppendLine("Trade Data imported to central database. For the following Exchange/Asset Class " + listing + "\n\n");
                    title = "STAGE (2b) Raw Trade Data Importation Completed";
                    //importLog = "<html><head></head><body>" +
                    //"<table style='width:100%'><tr><td>Jill</td><td>Smith</td><td>50</td></tr><tr><td>Eve</td><td>Jackson</td><td>94</td></tr></table>" +"</body></html>";
                }
                else
                {
                    message.AppendLine("Computed Data imported to central database. For the following Exchange/Asset Class " + listing + "\n\n");
                    title = "STAGE (4b) Computed Data Importation Completed";
                }

                message.AppendLine("Date :" + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
                message.AppendLine("\n\nData Manufacturing Process\n\n <br/><br/>");

                importLog = GetReportHtml(bRaw);

                message.AppendLine(importLog);

                var email = new EmailManager();

                email.Send(null, title, message.ToString(), true);
            }
            catch (Exception ex)
            {
                Library.WriteErrorLog(DateTime.Now + " :: " + ex.ToString());
            }
        }

        public void NotifyJM(String selector, List<String> exchange)
        {
            EmailNotificationCheck(exchange, true);

            String queryWhereClause = "";
            for (int i = 0; i < exchange.Count; i++)
            {
                if (i == 0)
                {
                    queryWhereClause += " WHERE [dbo].[Notify].Exchange = '" + exchange[i] + "'";
                }
                else
                {
                    queryWhereClause += " OR [dbo].[Notify].Exchange = '" + exchange[i] + "'";
                }
            }


            try
            {
                using (SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings[selector].ConnectionString))
                {
                    conn.Open();
                    SqlCommand sqlCommand = new SqlCommand("UPDATE [dbo].[Notify] SET [dbo].[Notify].Notify = '1'" + queryWhereClause, conn);


                    //SqlCommand sqlCommand = new SqlCommand("UPDATE [JobsManager].[dbo].[Notify] SET [JobsManager].[dbo].[Notify].Notify = '1' WHERE [JobsManager].[dbo].[Notify].Exchange = '" + exchange + "'", conn);
                    sqlCommand.CommandType = CommandType.Text;

                    // sqlCommand.Parameters.Add(stockExchange);
                    sqlCommand.CommandTimeout = 0;
                    sqlCommand.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Library.WriteErrorLog(DateTime.Now + " :: " + ex.ToString());
                throw;
            }
        }

        //public List<String> GetIncompletedRawDataFileList(String selector)
        //{
        //    int rowsAffected = 0;
        //    List<String> fileList = new List<String>();

        //    SqlParameter outputIdParam = new SqlParameter("@ROWSAFFECTED", SqlDbType.Int)
        //    {
        //        Direction = ParameterDirection.Output
        //    };

        //    try
        //    {
        //        String directoryOutput = "";

        //        using (SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings[selector].ConnectionString))
        //        {
        //            conn.Open();
        //            SqlCommand sqlCommand = new SqlCommand("proc_BulkInsertTrades", conn);
        //            sqlCommand.CommandType = CommandType.Text;

        //            sqlCommand.Parameters.Add(outputIdParam);
        //            sqlCommand.CommandTimeout = 0;
        //            sqlCommand.ExecuteNonQuery();

        //            rowsCommitted = Convert.ToInt32(outputIdParam.Value);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        rowsCommitted = Convert.ToInt32(outputIdParam.Value);
        //        throw;
        //    }
        //}
    }
}
