using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;
using System.Data.SqlClient;

namespace BackTestingApplication
{

    public class BatchRunLogItem
    {
        public int RunId;
        public string SymbolID;
        public string TimeFrame;
        public string Indicator;
        public string Categorization;
        public string Scenario;
        public double Returns;
        public double AverageReturn;
        public double MedianReturn;
        public double PercentPositive;
        public double PercentNegative;
        public string Description;
    } 

    public class DatabaseModel
    {
        private MODE _mode;

        public DatabaseModel(MODE mode)
        {
            _mode = mode;
        }

        public List<DateTime> GetDataExtents(string symbolID, string timeframe)
        {
            List<DateTime> dataRes = new List<DateTime>();

            String selector = "TRADES";
            switch (_mode)
            {
                case MODE.Test:
                    {
                        selector += "_TEST_Forex";
                    }
                    break;

                case MODE.Live:
                    {
                        selector += "_LIVE_Forex";
                    }
                    break;
            }


            var settings = System.Configuration.ConfigurationManager.ConnectionStrings[selector];


            try
            {
                using (SqlConnection c = new SqlConnection(settings.ToString()))
                {
                    c.Open();

                    SqlCommand sqlCommand = new SqlCommand("proc_GetDateExtents", c);
                    sqlCommand.CommandTimeout = 0;
                    sqlCommand.CommandType = CommandType.StoredProcedure;

                    sqlCommand.Parameters.AddWithValue("@SymbolID", symbolID);
                    sqlCommand.Parameters.AddWithValue("@TimeFrame", timeframe);

                    SqlDataReader reader = sqlCommand.ExecuteReader();
      
                    while (reader.Read())
                    {
                        DateTime start = new DateTime();
                        DateTime end = new DateTime();

                        if (DateTime.TryParse(reader["Minimum"].ToString(), out start))
                        {
                            dataRes.Add(start);

                            if (DateTime.TryParse(reader["Maximum"].ToString(), out end))
                            {
                                dataRes.Add(end);
                            }
                        }                       
                        //dataRes.Add(Convert.ToDateTime(reader["Minimum"].ToString()));
                        //dataRes.Add(Convert.ToDateTime(reader["Maximum"].ToString()));
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            return dataRes;
        }

        public int StartLogIndicatorPerformanceStats()
        {
            String selector = "JobsManager";
            switch (_mode)
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

            var settings = System.Configuration.ConfigurationManager.ConnectionStrings[selector];
            int id = 0;
            
            try
            {
                using (SqlConnection c = new SqlConnection(settings.ToString()))
                {
                    c.Open();

                    SqlCommand sqlCommand = new SqlCommand("proc_StartLogIndicatorPerformanceStats", c);
                    sqlCommand.CommandTimeout = 0;
                    sqlCommand.CommandType = CommandType.StoredProcedure;

                    //sqlCommand.ExecuteNonQuery();


                    SqlDataReader reader = sqlCommand.ExecuteReader();

                    while (reader.Read())
                    {
                        id = Convert.ToInt32(reader["BatchRunID"].ToString());                       
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            return id;
        }


        public void EndLogIndicatorPerformanceStats(int batchRunId)
        {
            String selector = "JobsManager";
            switch (_mode)
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


            var settings = System.Configuration.ConfigurationManager.ConnectionStrings[selector];

            try
            {
                using (SqlConnection c = new SqlConnection(settings.ToString()))
                {
                    c.Open();

                    SqlCommand sqlCommand = new SqlCommand("proc_EndLogIndicatorPerformanceStats", c);
                    sqlCommand.CommandTimeout = 0;
                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    sqlCommand.Parameters.AddWithValue("@ID", batchRunId);

                    sqlCommand.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }


        public void SavePerformanceStatistics(BatchRunLogItem batchrunLogitem)
        {
            List<DateTime> dataRes = new List<DateTime>();

            String selector = "JobsManager";
            switch (_mode)
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


            var settings = System.Configuration.ConfigurationManager.ConnectionStrings[selector];

            try
            {
                using (SqlConnection c = new SqlConnection(settings.ToString()))
                {
                    c.Open();

                    SqlCommand sqlCommand = new SqlCommand("proc_InsertIndicatorPerformanceStats", c);
                    sqlCommand.CommandTimeout = 0;
                    sqlCommand.CommandType = CommandType.StoredProcedure;

                    sqlCommand.Parameters.AddWithValue("@BatchRunId", batchrunLogitem.RunId);
                    sqlCommand.Parameters.AddWithValue("@SymbolID", batchrunLogitem.SymbolID);

                    sqlCommand.Parameters.AddWithValue("@TimeFrame", batchrunLogitem.TimeFrame);
                    sqlCommand.Parameters.AddWithValue("@Indicator", batchrunLogitem.Indicator);

                    sqlCommand.Parameters.AddWithValue("@Categorization", batchrunLogitem.Categorization);
                    sqlCommand.Parameters.AddWithValue("@Scenario", batchrunLogitem.Scenario);
                    sqlCommand.Parameters.AddWithValue("@Returns", batchrunLogitem.Returns);

                    sqlCommand.Parameters.AddWithValue("@AverageReturns", batchrunLogitem.AverageReturn);
                    sqlCommand.Parameters.AddWithValue("@MedianReturn", batchrunLogitem.MedianReturn);
                    sqlCommand.Parameters.AddWithValue("@PercentPositive", batchrunLogitem.PercentPositive);
                    sqlCommand.Parameters.AddWithValue("@PercentNegative", batchrunLogitem.PercentNegative);

                    sqlCommand.Parameters.AddWithValue("@Description", batchrunLogitem.Description);

                    sqlCommand.ExecuteNonQuery();                    
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}

