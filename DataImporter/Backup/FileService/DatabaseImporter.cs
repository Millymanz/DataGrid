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
                    SqlCommand sqlCommand = new SqlCommand("proc_BulkInsertHistoricalCorrelations", conn);
                    sqlCommand.CommandType = CommandType.StoredProcedure;

                    sqlCommand.Parameters.Add(outputIdParam);
                    sqlCommand.CommandTimeout = 0;
                    rowsAffected = sqlCommand.ExecuteNonQuery();

                    rowsCommitted = Convert.ToInt32(outputIdParam.Value);
                }
            }
            catch (Exception ex)
            {
                rowsCommitted = Convert.ToInt32(outputIdParam.Value);

                throw; 
            }
        }

        public void ImportRawData(String fileName, String stockExchange, ref int rowsCommitted, String selector)
        {
            int rowsAffected = 0;

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
                    SqlCommand sqlCommand = new SqlCommand("proc_BulkInsertTrades", conn);
                    sqlCommand.CommandType = CommandType.StoredProcedure;

                    sqlCommand.Parameters.Add(outputIdParam);
                    sqlCommand.CommandTimeout = 0;
                    sqlCommand.ExecuteNonQuery();

                    rowsCommitted = Convert.ToInt32(outputIdParam.Value);
                }
            }
            catch (Exception ex)
            {
                rowsCommitted = Convert.ToInt32(outputIdParam.Value);
                throw;
            }
        }

        public void NotifyJM(String selector, String stockExchange)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings[selector].ConnectionString))
                {
                    conn.Open();
                    SqlCommand sqlCommand = new SqlCommand("UPDATE [JobsManager].[dbo].[Notify] SET [JobsManager].[dbo].[Notify].Notify = '1' WHERE [JobsManager].[dbo].[Notify].Exchange = '" + stockExchange + "'", conn);
                    sqlCommand.CommandType = CommandType.Text;

                   // sqlCommand.Parameters.Add(stockExchange);
                    sqlCommand.CommandTimeout = 0;
                    sqlCommand.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
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
