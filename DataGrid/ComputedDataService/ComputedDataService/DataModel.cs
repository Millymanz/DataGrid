using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace ComputedDataService
{

    public class PerformanceStatsMaster
    {
        public List<PerformanceStats> ChartPatternPerformanceStats = new List<PerformanceStats>();
        public PerformanceStats IndicatorPerformanceStats = new PerformanceStats();
    }

    public enum TIMEFRAME
    {
        OneMinuteTimeFrame = 0,
        TwoMinuteTimeFrame,
        ThreeMinuteTimeFrame,
        FourMinuteTimeFrame,
        FiveMinuteTimeFrame,
        FifteenMinuteTimeFrame,
        ThrityMinuteTimeFrame,
        OneHourTimeFrame,
        TwoHourTimeFrame,
        ThreeHourTimeFrame,
        FourHourTimeFrame,
        EndOfDayTimeFrame,
        COUNT
    }

    public struct IndicatorPerformanceStats
    {
        public string SymbolID;
        public string TimeFrame;
        public string Indicator;
        public string Categorization;
        public string Scenario;

        public double Returns;
        public double AverageReturn;
        public double MedianReturn;
        public double PercentPositive;
    }

    public class DataModel
    {
        private MODE Mode;

        private PerformanceStatsMaster oneMinuteTimeFrameStats = new PerformanceStatsMaster();
        private PerformanceStatsMaster twoMinuteTimeFrameStats = new PerformanceStatsMaster();
        private PerformanceStatsMaster threeMinuteTimeFrameStats = new PerformanceStatsMaster();
        private PerformanceStatsMaster fourMinuteTimeFrameStats = new PerformanceStatsMaster();
        private PerformanceStatsMaster fiveMinuteTimeFrameStats = new PerformanceStatsMaster();
        private PerformanceStatsMaster fiffteenMinuteTimeFrameStats = new PerformanceStatsMaster();

        private PerformanceStatsMaster thirtyMinuteTimeFrameStats = new PerformanceStatsMaster();
        private PerformanceStatsMaster oneHourTimeFrameStats = new PerformanceStatsMaster();
        private PerformanceStatsMaster twoHourTimeFrameStats = new PerformanceStatsMaster();
        private PerformanceStatsMaster threeHourTimeFrameStats = new PerformanceStatsMaster();
        private PerformanceStatsMaster fourHourTimeFrameStats = new PerformanceStatsMaster();

        private PerformanceStatsMaster dayTimeFrameStats = new PerformanceStatsMaster();

        private Dictionary<string, string> _friendlyLookUp = new Dictionary<string, string>();


        public DataModel()
        {
            PopulateLookUp();

            var modeStr = System.Configuration.ConfigurationManager.AppSettings["MODE"];

            if (modeStr == "LIVE")
            {
                Mode = (MODE)0;
            }
            else 
            {
                Mode = (MODE)1;
            }

            InitializePerformanceStatistics();
        }

        public void InitializePerformanceStatistics()
        {
            for (int timef = 0; timef < (int)TIMEFRAME.COUNT; timef++)
            {
                //Pattern Table Listing
                PerformanceStats chartPatternStatsOverview = new PerformanceStats();
                chartPatternStatsOverview.Headers.Add("Pattern");
                chartPatternStatsOverview.Headers.Add("Total");
                chartPatternStatsOverview.Headers.Add("Correct Recognition");
                chartPatternStatsOverview.Headers.Add("Percentage");

                //Symbol Table Listing
                PerformanceStats chartPatternStats = new PerformanceStats();
                chartPatternStats.Headers.Add("Symbol");
                chartPatternStats.Headers.Add("Pattern");
                chartPatternStats.Headers.Add("TimeFrame");
                chartPatternStats.Headers.Add("Total");
                chartPatternStats.Headers.Add("Correct Recognition");
                chartPatternStats.Headers.Add("Percentage");

                PerformanceStats indicatorStats = new PerformanceStats();
                indicatorStats.Headers.Add("SymbolID");
                indicatorStats.Headers.Add("TimeFrame");
                indicatorStats.Headers.Add("Indicator");
                indicatorStats.Headers.Add("Categorization");
                indicatorStats.Headers.Add("Scenario");
                indicatorStats.Headers.Add("Returns");
                indicatorStats.Headers.Add("Average Return");
                indicatorStats.Headers.Add("Median Return");
                indicatorStats.Headers.Add("Percent Positive");

                PerformanceStatsMaster currentChartPatternStats = null;
                string timeFrame = "";

                switch ((TIMEFRAME)timef)
                {
                    case TIMEFRAME.EndOfDayTimeFrame: 
                    {
                        timeFrame = "EndOfDay";
                        currentChartPatternStats = dayTimeFrameStats;
                    } 
                    break;
                }

                if (currentChartPatternStats != null)
                {
                    InitChartPatternPerformanceStatistics(timeFrame, chartPatternStatsOverview, chartPatternStats, currentChartPatternStats);

                    InitIndicatorPerformanceStatistics(timeFrame, indicatorStats, currentChartPatternStats);
                }
            }


            //Symbol Table Listing
            //PerformanceStats indicatorStats = new PerformanceStats();
            //indicatorStats.Headers.Add("Indicator");
            //indicatorStats.Headers.Add("Returns");
            //indicatorStats.Headers.Add("Average Return");
            //indicatorStats.Headers.Add("Median Return");
            //indicatorStats.Headers.Add("Positive Return");
            //dayTimeFrameStats.ChartPatternPerformanceStats.Add(indicatorStats);

        }

        private void InitChartPatternPerformanceStatistics(string timeframe, PerformanceStats overview, PerformanceStats performanceStats, PerformanceStatsMaster currentChartPatternStats)
        {
            foreach (var item in Exchanges.List)
            {
                String exchange = item;

                String selector = "JobsManager";
                switch (Mode)
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

               // if (item == "Forex") selector += "_" + item;

                var settings = System.Configuration.ConfigurationManager.ConnectionStrings[selector];

                using (SqlConnection c = new SqlConnection(settings.ToString()))
                {
                    c.Open();

                    String query = "proc_GetChartPatternPerformanceStats_Overview";

                    SqlCommand sqlCommand = new SqlCommand(query, c);
                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    sqlCommand.Parameters.AddWithValue("@timeFrame", timeframe);

                    sqlCommand.CommandTimeout = 0;
                    SqlDataReader reader = sqlCommand.ExecuteReader();
                   


                    while (reader.Read())
                    {
                        switch (exchange)
                        {
                            case "NASDAQ":
                            case "LSE":
                            case "NYSE":
                            case "AMEX":
                                {
                                } break;

                            case "Forex":
                                {
                                    var dataList = new List<string>();
                                    dataList.Add(reader["Pattern"].ToString());
                                    dataList.Add(reader["Total"].ToString());
                                    dataList.Add(reader["CorrectRecognition"].ToString());
                                    dataList.Add(reader["Percentage"].ToString());

                                    overview.StatsLog.Add(dataList);

                                    //Symbol Table Listing
                                    //PerformanceStats indicatorStats = new PerformanceStats();
                                    //indicatorStats.Headers.Add("Indicator");
                                    //indicatorStats.Headers.Add("Returns");
                                    //indicatorStats.Headers.Add("Average Return");
                                    //indicatorStats.Headers.Add("Median Return");
                                    //indicatorStats.Headers.Add("Positive Return");
                                    //dayTimeFrameStats.ChartPatternPerformanceStats.Add(indicatorStats);

                                } break;
                        }
                    }
                }


                settings = System.Configuration.ConfigurationManager.ConnectionStrings[selector];

                using (SqlConnection c = new SqlConnection(settings.ToString()))
                {
                    c.Open();

                    String query = "proc_GetChartPatternPerformanceStats";

                    SqlCommand sqlCommand = new SqlCommand(query, c);
                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    sqlCommand.Parameters.AddWithValue("@timeFrame", timeframe);


                    sqlCommand.CommandTimeout = 0;
                    SqlDataReader reader = sqlCommand.ExecuteReader();

                    while (reader.Read())
                    {
                        switch (exchange)
                        {
                            case "NASDAQ":
                            case "LSE":
                            case "NYSE":
                            case "AMEX":
                                {
                                } break;

                            case "Forex":
                                {
                                    var dataList = new List<string>();
                                    dataList.Add(reader["SymbolID"].ToString());
                                    dataList.Add(reader["Pattern"].ToString());
                                    dataList.Add(reader["TimeFrame"].ToString());
                                    dataList.Add(reader["Total"].ToString());
                                    dataList.Add(reader["CorrectRecognition"].ToString());
                                    dataList.Add(reader["Percentage"].ToString());

                                    performanceStats.StatsLog.Add(dataList);

                                } break;
                        }
                    }
                }


                if (exchange == "Forex")
                {//temp solution
                    currentChartPatternStats.ChartPatternPerformanceStats.Add(overview);
                    currentChartPatternStats.ChartPatternPerformanceStats.Add(performanceStats);
                }

            }
        }

        private void InitIndicatorPerformanceStatistics(string timeframe, PerformanceStats performanceStats, PerformanceStatsMaster currentIndicatorStats)
        {
            foreach (var item in Exchanges.List)
            {
                String exchange = item;

                String selector = "JobsManager";
                switch (Mode)
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

                using (SqlConnection c = new SqlConnection(settings.ToString()))
                {
                    c.Open();

                    String query = "proc_GetIndicatorPerformanceStats";

                    SqlCommand sqlCommand = new SqlCommand(query, c);
                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    sqlCommand.Parameters.AddWithValue("@timeFrame", timeframe);

                    sqlCommand.CommandTimeout = 0;
                    SqlDataReader reader = sqlCommand.ExecuteReader();

                    while (reader.Read())
                    {
                        switch (exchange)
                        {
                            case "NASDAQ":
                            case "LSE":
                            case "NYSE":
                            case "AMEX":
                                {
                                } break;

                            case "Forex":
                                {
                                    var dataList = new List<string>();
                                    dataList.Add(reader["SymbolID"].ToString());
                                    dataList.Add(reader["TimeFrame"].ToString());
                                    dataList.Add(reader["Indicator"].ToString());
                                    dataList.Add(reader["Categorization"].ToString());

                                    dataList.Add(reader["Scenario"].ToString());
                                    dataList.Add(reader["Returns"].ToString());

                                    dataList.Add(reader["Average Return"].ToString());
                                    dataList.Add(reader["Median Return"].ToString());

                                    dataList.Add(reader["Percent Positive"].ToString());

                                    performanceStats.StatsLog.Add(dataList);                                 

                                } break;
                        }
                    }
                }

                if (exchange == "Forex")
                {
                    currentIndicatorStats.IndicatorPerformanceStats = performanceStats;
                }
            }
        }


        public List<PerformanceStats> GetChartPatternPerformanceStatistics(string operation, string type, string timeframe, string exchange)
        {
            var performanceStats = SelectPerformanceStatistics(timeframe);

            //operation convert to database friendly version

            if (type  == "ChartPattern")
            {
                var selectedPortion = performanceStats.ChartPatternPerformanceStats.Where(m => m.StatsLog.Any(y => y.Contains("Head and Shoulders")));

                return selectedPortion.ToList();
            }
            else if (type == "Indicator")
            {
                var selectedPortion = performanceStats.IndicatorPerformanceStats.StatsLog.Any(y => y.Contains("Relative Strength Index - RSI"));

                return null;
            }

            return null;
        }

        public List<PerformanceStats> GetIndicatorPerformanceStatistics(List<String> symbolList, string category, string timeframe, string exchange)
        {
            var performanceStats = SelectPerformanceStatistics(timeframe);
            string found = "";

            if (_friendlyLookUp.TryGetValue(category, out found))
            {
                var selectedPortion = performanceStats.IndicatorPerformanceStats.StatsLog.Where(y => y.Contains(found) && y.Any(t1 => symbolList.Contains(t1)));

                if (selectedPortion.Any())
                {
                    var list = new List<PerformanceStats>();

                    var perform = new PerformanceStats();
                    perform.Description = GetDescription("Indicator");
                    perform.PerformanceStatsType = "Indicator";

                    perform.Headers = performanceStats.IndicatorPerformanceStats.Headers;
                    perform.StatsLog = SortIndicatorPerformanceStatistics(selectedPortion.ToList());

                    list.Add(perform);
                    return list;
                }
            }
            return null;
        }

        private string GetDescription(string type)
        {
            string msg = "";
            switch (type)
            {
                case "ChartPattern" :
                    {
                        msg = "*Below are listed table(s) of performance statistics which predominantly shows the pattern recoginition rate and this tells you how reliable the recognition is for the symbol."
                    + "<br/>The percentage value gains more significance and value over time as the number of patterns found increases.";
                    } break;

                case "Indicator":
                    {
                        msg = "Below are listed table(s) of performance statistics showing the performance of the listed technical analysis indicators for Over the Past 12 Months."
                            + "<br/>TradeRiser selects the best indicator to use based on the percentage average return for the given currency and timeframe.";
                    } break;
            }
            return msg;

        }

        public string GetIndicatorWithBestPerformanceStatistics(List<String> symbolList, string operation, string timeframe, string exchange)
        {
            var performanceStats = SelectPerformanceStatistics(timeframe);

            string found = "";
            if (_friendlyLookUp.TryGetValue(operation, out found))
            {
                //workout best performance                   
                var relevantSelection = performanceStats.IndicatorPerformanceStats.StatsLog.Where(y => y.Contains(found) && y.Contains(timeframe) && y.Any(t1 => symbolList.Contains(t1)));

                if (relevantSelection.Any())
                {
                    var finalSelection = SortIndicatorPerformanceStatistics(relevantSelection.ToList());
                    if (finalSelection.Any())
                    {
                        var indicator = finalSelection.FirstOrDefault()[2]; //AverageReturn
                        
                        _friendlyLookUp.TryGetValue(indicator, out found);

                        return found;
                    }
                }
            }
            return String.Empty;
        }

        private List<List<string>> SortIndicatorPerformanceStatistics(List<List<string>> performanceStats)
        {
            var performList = new List<IndicatorPerformanceStats>();

            foreach (var item in performanceStats)
            {
                var indPerform = new IndicatorPerformanceStats()
                {
                    SymbolID = item[0],
                    TimeFrame = item[1],
                    Indicator = item[2],
                    Categorization = item[3],
                    Scenario = item[4],
                    Returns = Convert.ToDouble(item[5]),
                    AverageReturn = Convert.ToDouble(item[6]),
                    MedianReturn = Convert.ToDouble(item[7]),
                    PercentPositive = Convert.ToDouble(item[8])                    
                };
                performList.Add(indPerform);
            }

            var orderedVersion = performList.OrderByDescending(m => m.AverageReturn).ToList();

            if (orderedVersion != null)
            {
                if (orderedVersion.Any())
                {
                    List<List<string>> finalList = new List<List<string>>();

                    foreach (var itemA in orderedVersion)
                    {
                        var list = new List<string>();
                        list.Add(itemA.SymbolID);
                        list.Add(itemA.TimeFrame);
                        list.Add(itemA.Indicator);
                        list.Add(itemA.Categorization);
                        list.Add(itemA.Scenario);

                        list.Add(itemA.Returns.ToString());
                        list.Add(itemA.AverageReturn.ToString());
                        list.Add(itemA.MedianReturn.ToString());
                        list.Add(itemA.PercentPositive.ToString());

                        finalList.Add(list);
                    }
                    return finalList;
                }               
            }
            return null;
        }

        public PerformanceStatsMaster SelectPerformanceStatistics(string timeFrame)
        {
            PerformanceStatsMaster currentChartPatternStats = null;

            switch (timeFrame)
            {
                case "1min":
                    {
                        currentChartPatternStats = oneMinuteTimeFrameStats;                       
                    } break;

                case "5min":
                    {
                        currentChartPatternStats = fiveMinuteTimeFrameStats;                      
                    } break;

                case "15min":
                    {
                        currentChartPatternStats = fiffteenMinuteTimeFrameStats;                      
                    } break;

                case "30min":
                    {
                        currentChartPatternStats = thirtyMinuteTimeFrameStats;                     
                    } break;

                case "1hour":
                    {
                        currentChartPatternStats = oneHourTimeFrameStats;                     
                    } break;

                case "2hour":
                    {
                        currentChartPatternStats = twoHourTimeFrameStats;                     
                    } break;

                case "3hour":
                    {
                        currentChartPatternStats = threeHourTimeFrameStats;                     
                    } break;

                case "4hour":
                    {
                        currentChartPatternStats = fourHourTimeFrameStats;
                    } break;

                case "EndOfDay":
                    {
                        currentChartPatternStats = dayTimeFrameStats;                    
                    } break;
            }
            return currentChartPatternStats;
        }
        
        private void PopulateLookUp()
        {
            //_friendlyLookUp.Add("aroonoscillator", "Aroon Oscillator");
            //_friendlyLookUp.Add("adx", "Average Directional Movement Index - ADX");
            //_friendlyLookUp.Add("adxr", "Average Directional Movement Index Rating - ADXR");

            //_friendlyLookUp.Add("atr", "Average True Range - ATR");
            //_friendlyLookUp.Add("std", "Standard Deviation - STD");

            //_friendlyLookUp.Add("keltnerchannels", "Keltner Channels");
            //_friendlyLookUp.Add("bollingerbands", "Bollinger Bands");

            //_friendlyLookUp.Add("rsi", "Relative Strength Index - RSI");
            //_friendlyLookUp.Add("trix", "Triple Exponential Average - TRIX");

            //_friendlyLookUp.Add("apo", "Absolute Price Oscillator - APO");
            //_friendlyLookUp.Add("ad", "Chaikin A/D Line - AD");

            //_friendlyLookUp.Add("adosc", "Chaikin A/D Oscillator - ADOSC");


            _friendlyLookUp.Add("Aroon Oscillator", "aroonoscillator");
            _friendlyLookUp.Add("Average Directional Movement Index - ADX", "adx");
            _friendlyLookUp.Add("Average Directional Movement Index Rating - ADXR", "adxr");

            _friendlyLookUp.Add("Average True Range - ATR", "atr");
            _friendlyLookUp.Add("Standard Deviation - STD", "std");

            _friendlyLookUp.Add("Keltner Channels", "keltnerchannels");
            _friendlyLookUp.Add("Bollinger Bands", "bollingerbands");

            _friendlyLookUp.Add("Relative Strength Index - RSI", "rsi");
            _friendlyLookUp.Add("Triple Exponential Average - TRIX", "trix");

            _friendlyLookUp.Add("Absolute Price Oscillator - APO", "apo");
            _friendlyLookUp.Add("Chaikin A/D Line - AD", "ad");

            _friendlyLookUp.Add("Chaikin A/D Oscillator - ADOSC", "adosc");


            _friendlyLookUp.Add("momentum", "Momentum");
            _friendlyLookUp.Add("volatility", "Volatility");

            _friendlyLookUp.Add("trend", "Trend");


        }
    }


}
