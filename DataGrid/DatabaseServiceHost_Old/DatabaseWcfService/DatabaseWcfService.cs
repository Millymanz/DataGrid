using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

using System.Data;
using System.Data.SqlClient;
//using System.Data.DataSetExtensions;

using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace DatabaseWcfService
{

    public enum MODE
    {
        Live = 0,
        Test = 1,
        LiveTest = 2
    }

    static public class Exchanges
    {
        static public List<String> List = new List<string>();

        static public void InitialiseExchangeList()
        {
            string[] exchangesArray = System.Configuration.ConfigurationManager.AppSettings["EXCHANGES"].Split(';');
            Exchanges.List = exchangesArray.ToList();
        }
    }

    public struct DataFetchCriteria
    {
        public string SymbolID;
        public string TimeFrame;
        public string StartDateTimeStr;
    }


    // NOTE: If you change the class name "Service1" here, you must also update the reference to "Service1" in App.config.

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class DatabaseWcfService : IDatabaseWcfService
    {
        //exchange / data
        public static Dictionary<String, List<TradeSummary>> dayTradeSummaries_LSE = new Dictionary<String, List<TradeSummary>>();
        public static Dictionary<String, List<TradeSummary>> dayTradeSummaries_NYSE = new Dictionary<String, List<TradeSummary>>();
        public static Dictionary<String, List<TradeSummary>> dayTradeSummaries_AMEX = new Dictionary<String, List<TradeSummary>>();
        public static Dictionary<String, List<TradeSummary>> dayTradeSummaries_NASDAQ = new Dictionary<String, List<TradeSummary>>();


        public static Dictionary<String, TradeSummary> dayTradeSummaries_AMEX_LOOKUP = new Dictionary<String, TradeSummary>();
        public static Dictionary<String, TradeSummary> dayTradeSummaries_NYSE_LOOKUP = new Dictionary<String, TradeSummary>();
        public static Dictionary<String, TradeSummary> dayTradeSummaries_NASDAQ_LOOKUP = new Dictionary<String, TradeSummary>();
        public static Dictionary<String, TradeSummary> dayTradeSummaries_LSE_LOOKUP = new Dictionary<String, TradeSummary>();

        public static Dictionary<String, TradeSummary> dayTradeSummaries_FOREX_LOOKUP = new Dictionary<String, TradeSummary>();



        public static Dictionary<String, TradeSummary> oneMinuteTradeSummaries_FOREX_LOOKUP_X = new Dictionary<String, TradeSummary>();
        public static Dictionary<String, TradeSummary> fiveMinuteTradeSummaries_FOREX_LOOKUP_X = new Dictionary<String, TradeSummary>();

        public static Dictionary<String, TradeSummary> fifteenMinuteTradeSummaries_FOREX_LOOKUPX = new Dictionary<String, TradeSummary>();
        public static Dictionary<String, TradeSummary> thrityMinuteTradeSummaries_FOREX_LOOKUPX = new Dictionary<String, TradeSummary>();
        public static Dictionary<String, TradeSummary> oneHourTradeSummaries_FOREX_LOOKUPX = new Dictionary<String, TradeSummary>();
        public static Dictionary<String, TradeSummary> twoHourTradeSummaries_FOREX_LOOKUPX = new Dictionary<String, TradeSummary>();
        public static Dictionary<String, TradeSummary> threeHourTradeSummaries_FOREX_LOOKUPX = new Dictionary<String, TradeSummary>();


        //Economic Fundamental
        public static Dictionary<String, EconomicFundamentals> employmentFundamentals = new Dictionary<String, EconomicFundamentals>();
        public static Dictionary<String, EconomicFundamentals> centralBankFundamentals = new Dictionary<String, EconomicFundamentals>();

        public static List<EconomicFundamentalsEssentials> economicFundamentalsEssentials = new List<EconomicFundamentalsEssentials>();

        static public MODE Mode;
        private const double _lackValueCheck = -999999.9999;


        public DatabaseWcfService()
        {
            ThreadStart threadStart = new ThreadStart(InitialseDataTables);
            Thread initialseDataTablesThread = new Thread(threadStart);
            initialseDataTablesThread.Start();

            //InitialseDataTables();
        }

        public void InitialseDataTables()
        {
            Exchanges.InitialiseExchangeList();

            //Listen for updates
            ThreadStart threadStart = new ThreadStart(DataUpdateListenner);
            Thread jobCreatorThread = new Thread(threadStart);
            jobCreatorThread.Start();

            PopulateDataTables();
        }

        private void ClearAllData()
        {
            //clear data
            dayTradeSummaries_LSE.Clear();
            dayTradeSummaries_NYSE.Clear();
            dayTradeSummaries_NASDAQ.Clear();
            dayTradeSummaries_AMEX.Clear();

            oneMinuteTradeSummaries_FOREX_LOOKUP_X.Clear();
            fiveMinuteTradeSummaries_FOREX_LOOKUP_X.Clear();
            fifteenMinuteTradeSummaries_FOREX_LOOKUPX.Clear();
            thrityMinuteTradeSummaries_FOREX_LOOKUPX.Clear();
            oneHourTradeSummaries_FOREX_LOOKUPX.Clear();
            twoHourTradeSummaries_FOREX_LOOKUPX.Clear();
            threeHourTradeSummaries_FOREX_LOOKUPX.Clear();

            dayTradeSummaries_FOREX_LOOKUP.Clear();
        }

        private void PopulateDataTables()
        {
            Console.WriteLine("\n\n");

            ClearAllData();

            Console.WriteLine("Data fetching in progress");

            GetEconomicFundamentalMarketData();

            GetMarketDataSmart();

        }

        private void DataUpdateListenner()
        {
            IPHostEntry host;
            string localIP = "?";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily.ToString() == "InterNetwork")
                {
                    localIP = ip.ToString();
                }
            }
            int port = 0;

            if (Mode == MODE.Test)
            {
                //Incase running on same machine as jbs manager
               // port = 11022;
                port = 11000;
            }
            else
            {
                //Live will always been on different machines
               // port = 11000; change temporary
                port = 11008;
            }

            while (true)
            {
                UdpClient udpClient = new UdpClient(port);
                try
                {
                    //IPEndPoint object will allow us to read datagrams sent from any source.
                    IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Parse(localIP), port);

                    // Blocks until a message returns on this socket from a remote host.
                    Byte[] receiveBytes = udpClient.Receive(ref RemoteIpEndPoint);
                    string returnData = Encoding.ASCII.GetString(receiveBytes);

                    // Uses the IPEndPoint object to determine which of these two hosts responded.
                   

                    String[] conditionArray = returnData.Split('_');

                    if (conditionArray.FirstOrDefault() == "bUpdateDBServicesANDStartProcessingCycle")
                    {
                        //update database
                        Console.WriteLine("Updating In Memory DB :: " +
                                                conditionArray[1].ToString());
                        PopulateDataTables();
                    }

                    udpClient.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    Logger.log.Error(e.Message);
                }
            }
        }

        private void GetMarketData()
        {
            String currentTitle = Console.Title;

            var startDate = System.Configuration.ConfigurationManager.AppSettings["STARTDATE"].ToString();

            DateTime currentStartDate = DateTime.Now;
            currentStartDate = currentStartDate.AddYears(-2);
            var dateStr = currentStartDate.ToString("yyyy-MM-dd");


            foreach (var item in Exchanges.List)
            {
                GC.Collect();

                String stockExchange = item;

                String selector = "TRADES";
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

                if (item == "Forex") selector += "_" + item;

                var settings = System.Configuration.ConfigurationManager.ConnectionStrings[selector];

                List<String> timeframeList = System.Configuration.ConfigurationManager.AppSettings["TimeFrameList"].Split(',').ToList();

                foreach (var timeframeItem in timeframeList)
                {
                    try
                    {
                        using (SqlConnection c = new SqlConnection(settings.ToString()))
                        {
                            c.Open();
                            String query = "SELECT * FROM dbo.tblTradesWorkingVersion WHERE dbo.tblTradesWorkingVersion.StockExchange = '" + item
                                + "' AND dbo.tblTradesWorkingVersion.DateTime > '" + startDate + "' ORDER BY dbo.tblTradesWorkingVersion.SymbolID, dbo.tblTradesWorkingVersion.DateTime ASC";

                            if (item == "Forex") query = "SELECT * FROM dbo.tblTradesWorkingVersion WHERE dbo.tblTradesWorkingVersion.TimeFrame = '" + timeframeItem
                                + "' AND dbo.tblTradesWorkingVersion.DateTime > '" + dateStr + "' ORDER BY dbo.tblTradesWorkingVersion.SymbolID, dbo.tblTradesWorkingVersion.DateTime ASC";

                            SqlCommand sqlCommand = new SqlCommand(query, c);
                            sqlCommand.CommandTimeout = 0;

                            SqlDataReader reader = sqlCommand.ExecuteReader();
                            if (reader.HasRows)
                                Console.WriteLine("Fetching :: " + item + " Data.." + DateTime.Now.ToString());

                            String previousSymbolID = "";

                            List<TradeSummary> dayTradeList = new List<TradeSummary>(); ;

                            while (reader.Read())
                            {
                                TradeSummary dayTrade = new TradeSummary();

                                String symbolID = reader["SymbolID"].ToString();

                                dayTrade.SymbolID = symbolID;
                                dayTrade.DateTime = Convert.ToDateTime(reader["DateTime"].ToString());
                                dayTrade.Open = Convert.ToDouble(reader["Open"].ToString());
                                dayTrade.High = Convert.ToDouble(reader["High"].ToString());
                                dayTrade.Low = Convert.ToDouble(reader["Low"].ToString());
                                dayTrade.Close = Convert.ToDouble(reader["Close"].ToString());
                                dayTrade.TimeFrame = reader["TimeFrame"].ToString();

                                dayTrade.Exchange = item;

                                if (item != "Forex")
                                {
                                    dayTrade.Volume = Convert.ToInt32(reader["Volume"].ToString());
                                }

                                dayTrade.AdjustmentClose = Convert.ToDouble(reader["AdjustmentClose"].ToString());

                                previousSymbolID = symbolID;

                                String lookUpID = symbolID + "#" + dayTrade.DateTime.Ticks.ToString();

                                switch (stockExchange)
                                {
                                    case "LSE":
                                        {
                                            dayTradeSummaries_LSE_LOOKUP.Add(lookUpID, dayTrade);
                                        } break;

                                    case "NASDAQ":
                                        {
                                            dayTradeSummaries_NASDAQ_LOOKUP.Add(lookUpID, dayTrade);
                                        } break;

                                    case "NYSE":
                                        {
                                            dayTradeSummaries_NYSE_LOOKUP.Add(lookUpID, dayTrade);
                                        } break;

                                    case "AMEX":
                                        {
                                            dayTradeSummaries_AMEX_LOOKUP.Add(lookUpID, dayTrade);
                                        } break;

                                    case "Forex":
                                        {
                                            switch (timeframeItem)
                                            {
                                                case "1min":
                                                    {
                                                        oneMinuteTradeSummaries_FOREX_LOOKUP_X.Add(lookUpID, dayTrade);

                                                    } break;

                                                case "5min":
                                                    {
                                                        fiveMinuteTradeSummaries_FOREX_LOOKUP_X.Add(lookUpID, dayTrade);

                                                    } break;

                                                case "15min":
                                                    {
                                                        fifteenMinuteTradeSummaries_FOREX_LOOKUPX.Add(lookUpID, dayTrade);

                                                    } break;

                                                case "30min":
                                                    {
                                                        thrityMinuteTradeSummaries_FOREX_LOOKUPX.Add(lookUpID, dayTrade);

                                                    } break;

                                                case "1hour":
                                                    {
                                                        oneHourTradeSummaries_FOREX_LOOKUPX.Add(lookUpID, dayTrade);

                                                    } break;

                                                case "2hour":
                                                    {
                                                        twoHourTradeSummaries_FOREX_LOOKUPX.Add(lookUpID, dayTrade);

                                                    } break;

                                                case "3hour":
                                                    {
                                                        threeHourTradeSummaries_FOREX_LOOKUPX.Add(lookUpID, dayTrade);

                                                    } break;

                                                case "EndOfDay":
                                                    {
                                                        dayTradeSummaries_FOREX_LOOKUP.Add(lookUpID, dayTrade);

                                                    } break;
                                            }

                                        } break;
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.log.Error("Database Error - " + e.Message);
                    }
                }


                switch (stockExchange)
                {
                    case "LSE":
                        {
                            Console.WriteLine("Exchange LSE:: " + dayTradeSummaries_LSE_LOOKUP.Count());
                        } break;

                    case "NASDAQ":
                        {
                            Console.WriteLine("Exchange NASDAQ:: " + dayTradeSummaries_NASDAQ_LOOKUP.Count());
                        } break;

                    case "NYSE":
                        {
                            Console.WriteLine("Exchange NYSE:: " + dayTradeSummaries_NYSE_LOOKUP.Count());
                        } break;

                    case "AMEX":
                        {
                            Console.WriteLine("Exchange AMEX:: " + dayTradeSummaries_AMEX_LOOKUP.Count());
                        } break;

                    case "Forex":
                        {
                            Console.WriteLine("Exchange 1min Forex:: " + oneMinuteTradeSummaries_FOREX_LOOKUP_X.Count());
                            Console.WriteLine("Exchange 5min Forex:: " + fiveMinuteTradeSummaries_FOREX_LOOKUP_X.Count());
                            Console.WriteLine("Exchange 15min Forex:: " + fifteenMinuteTradeSummaries_FOREX_LOOKUPX.Count());
                            Console.WriteLine("Exchange 30min Forex:: " + thrityMinuteTradeSummaries_FOREX_LOOKUPX.Count());
                            Console.WriteLine("Exchange 1hour Forex:: " + oneHourTradeSummaries_FOREX_LOOKUPX.Count());
                            Console.WriteLine("Exchange 2hour Forex:: " + twoHourTradeSummaries_FOREX_LOOKUPX.Count());
                            Console.WriteLine("Exchange 3hour Forex:: " + threeHourTradeSummaries_FOREX_LOOKUPX.Count());

                            Console.WriteLine("Exchange EndOfDay Forex:: " + dayTradeSummaries_FOREX_LOOKUP.Count());
                        } break;
                }
            }
            Console.WriteLine("[Data Retrieval Complete]");

        }

        private void GetEconomicFundamentalMarketData()
        {
            String currentTitle = Console.Title;

            var startDate = System.Configuration.ConfigurationManager.AppSettings["STARTDATE"].ToString();

            DateTime currentStartDate = DateTime.Now;
            currentStartDate = currentStartDate.AddYears(-2);
            var dateStr = currentStartDate.ToString("yyyy-MM-dd");

            GC.Collect();


            String selector = "ECONOMIC_FUNDAMENTALS";
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

            List<String> categoryList = System.Configuration.ConfigurationManager.AppSettings["CategoryList"].Split(',').ToList();
            Dictionary<string, int> lookUpCount = new Dictionary<string, int>();

            foreach (var categoryItem in categoryList)
            {
                DataEconomicFundamentalsCount(settings.ToString(), categoryItem, dateStr, lookUpCount);
            }

            foreach (var categoryItem in categoryList)
            {
                Dictionary<string, EconomicFundamentals> currentLookUp = null;
                switch (categoryItem)
                {
                    case "none":
                    case "central bank":
                        {
                            currentLookUp = centralBankFundamentals;

                        } break;

                    case "employment":
                        {
                            currentLookUp = employmentFundamentals;

                        } break;
                }

                PopulateEconomicFundamentalsLookUp(settings.ToString(), categoryItem, dateStr, currentLookUp, lookUpCount);
            }

            PopulateEconomicFundamentalsEssentialsLookUp(settings.ToString());


            //
            Console.WriteLine("Central Bank :: " + centralBankFundamentals.Count());
            Console.WriteLine("Employment :: " + employmentFundamentals.Count());
            Console.WriteLine("[Data Retrieval Complete]");

        }


        private void GetMarketDataSmart()
        {
            String currentTitle = Console.Title;

            var startDate = System.Configuration.ConfigurationManager.AppSettings["STARTDATE"].ToString();

            DateTime currentStartDate = DateTime.Now;
            currentStartDate = currentStartDate.AddYears(-2);
            var dateStr = currentStartDate.ToString("yyyy-MM-dd");
            //dateStr = "2015-04-01";


            foreach (var item in Exchanges.List)
            {
                GC.Collect();

                String stockExchange = item;

                String selector = "TRADES";
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

                if (item == "Forex") selector += "_" + item;

                var settings = System.Configuration.ConfigurationManager.ConnectionStrings[selector];

                List<String> timeframeList = System.Configuration.ConfigurationManager.AppSettings["TimeFrameList"].Split(',').ToList();
                Dictionary<string, int> lookUpCount = new Dictionary<string,int>();

                foreach (var timeframeItem in timeframeList)
                {
                    DataCount(settings.ToString(), timeframeItem, dateStr, stockExchange, lookUpCount);
                }

                foreach (var timeframeItem in timeframeList)
                {
                    Dictionary<string, TradeSummary> currentLookUp = null;
                    switch (stockExchange)
                    {
                        case "LSE":
                            {
                                Console.WriteLine("Exchange LSE:: " + dayTradeSummaries_LSE_LOOKUP.Count());
                            } break;

                        case "NASDAQ":
                            {
                                Console.WriteLine("Exchange NASDAQ:: " + dayTradeSummaries_NASDAQ_LOOKUP.Count());
                            } break;

                        case "NYSE":
                            {
                                Console.WriteLine("Exchange NYSE:: " + dayTradeSummaries_NYSE_LOOKUP.Count());
                            } break;

                        case "AMEX":
                            {
                                Console.WriteLine("Exchange AMEX:: " + dayTradeSummaries_AMEX_LOOKUP.Count());
                            } break;

                        case "Forex":
                            {
                                switch (timeframeItem)
                                    {
                                        case "1min":
                                            {
                                                currentLookUp = oneMinuteTradeSummaries_FOREX_LOOKUP_X;

                                            } break;

                                        case "5min":
                                            {
                                               currentLookUp = fiveMinuteTradeSummaries_FOREX_LOOKUP_X;

                                            } break;

                                        case "15min":
                                            {
                                                currentLookUp = fifteenMinuteTradeSummaries_FOREX_LOOKUPX;

                                            } break;

                                        case "30min":
                                            {
                                                currentLookUp = thrityMinuteTradeSummaries_FOREX_LOOKUPX;

                                            } break;

                                        case "1hour":
                                            {
                                                currentLookUp = oneHourTradeSummaries_FOREX_LOOKUPX;

                                            } break;

                                        case "2hour":
                                            {
                                                currentLookUp = twoHourTradeSummaries_FOREX_LOOKUPX;

                                            } break;

                                        case "3hour":
                                            {
                                                currentLookUp = threeHourTradeSummaries_FOREX_LOOKUPX;

                                            } break;

                                        case "EndOfDay":
                                            {
                                                currentLookUp = dayTradeSummaries_FOREX_LOOKUP;

                                            } break;
                                }
                                //Console.WriteLine("Exchange 1min Forex:: " + oneMinuteTradeSummaries_FOREX_LOOKUP_X.Count());
                                //Console.WriteLine("Exchange 5min Forex:: " + fiveMinuteTradeSummaries_FOREX_LOOKUP_X.Count());
                                //Console.WriteLine("Exchange 15min Forex:: " + fifteenMinuteTradeSummaries_FOREX_LOOKUPX.Count());
                                //Console.WriteLine("Exchange 30min Forex:: " + thrityMinuteTradeSummaries_FOREX_LOOKUPX.Count());
                                //Console.WriteLine("Exchange 1hour Forex:: " + oneHourTradeSummaries_FOREX_LOOKUPX.Count());
                                //Console.WriteLine("Exchange 2hour Forex:: " + twoHourTradeSummaries_FOREX_LOOKUPX.Count());
                                //Console.WriteLine("Exchange 3hour Forex:: " + threeHourTradeSummaries_FOREX_LOOKUPX.Count());

                                //Console.WriteLine("Exchange EndOfDay Forex:: " + dayTradeSummaries_FOREX_LOOKUP.Count());
                            } break;
                    }


                    PopulateLookUp(settings.ToString(), timeframeItem, dateStr, item, currentLookUp, lookUpCount);
                }

                //


                switch (stockExchange)
                {
                    case "LSE":
                        {
                            Console.WriteLine("Exchange LSE:: " + dayTradeSummaries_LSE_LOOKUP.Count());
                        } break;

                    case "NASDAQ":
                        {
                            Console.WriteLine("Exchange NASDAQ:: " + dayTradeSummaries_NASDAQ_LOOKUP.Count());
                        } break;

                    case "NYSE":
                        {
                            Console.WriteLine("Exchange NYSE:: " + dayTradeSummaries_NYSE_LOOKUP.Count());
                        } break;

                    case "AMEX":
                        {
                            Console.WriteLine("Exchange AMEX:: " + dayTradeSummaries_AMEX_LOOKUP.Count());
                        } break;

                    case "Forex":
                        {
                            Console.WriteLine("Exchange 1min Forex:: " + oneMinuteTradeSummaries_FOREX_LOOKUP_X.Count());
                            Console.WriteLine("Exchange 5min Forex:: " + fiveMinuteTradeSummaries_FOREX_LOOKUP_X.Count());
                            Console.WriteLine("Exchange 15min Forex:: " + fifteenMinuteTradeSummaries_FOREX_LOOKUPX.Count());
                            Console.WriteLine("Exchange 30min Forex:: " + thrityMinuteTradeSummaries_FOREX_LOOKUPX.Count());
                            Console.WriteLine("Exchange 1hour Forex:: " + oneHourTradeSummaries_FOREX_LOOKUPX.Count());
                            Console.WriteLine("Exchange 2hour Forex:: " + twoHourTradeSummaries_FOREX_LOOKUPX.Count());
                            Console.WriteLine("Exchange 3hour Forex:: " + threeHourTradeSummaries_FOREX_LOOKUPX.Count());

                            Console.WriteLine("Exchange EndOfDay Forex:: " + dayTradeSummaries_FOREX_LOOKUP.Count());
                        } break;
                }
            }
            Console.WriteLine("[Data Retrieval Complete]");

        }

        public void DataCount(string conStr, string timeFrame, string startDate, string exchange, Dictionary<string, int> lookUpCount)
        {
            //Dictionary<string, int> currentLookUpCount = new Dictionary<string, int>();
            
            try
            {
                using (SqlConnection c = new SqlConnection(conStr.ToString()))
                {
                    c.Open();
                    String query = "SELECT COUNT(*) AS DataCount FROM dbo.tblTradesWorkingVersion WHERE dbo.tblTradesWorkingVersion.StockExchange = '" + exchange
                        + "' AND dbo.tblTradesWorkingVersion.DateTime > '" + startDate + "'";

                    if (exchange == "Forex") query = "SELECT COUNT(*) AS DataCount FROM dbo.tblTradesWorkingVersion WHERE dbo.tblTradesWorkingVersion.TimeFrame = '" + timeFrame
                        + "' AND dbo.tblTradesWorkingVersion.DateTime > '" + startDate + "'";

                    SqlCommand sqlCommand = new SqlCommand(query, c);
                    sqlCommand.CommandTimeout = 0;

                    SqlDataReader reader = sqlCommand.ExecuteReader();
                    //if (reader.HasRows)
                       // Console.WriteLine("Fetching :: " + exchange + " Data.." + DateTime.Now.ToString());

                    List<TradeSummary> dayTradeList = new List<TradeSummary>(); ;

                    while (reader.Read())
                    {
                        int dataCount = Convert.ToInt32(reader["DataCount"].ToString());
                        lookUpCount.Add(timeFrame, dataCount);

                        Console.WriteLine("Expected Count :: " + exchange + " " + timeFrame + " Data.." + dataCount);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.log.Error("Database Error - " + e.Message);
            }
        }

        public void DataEconomicFundamentalsCount(string conStr, string category, string startDate, Dictionary<string, int> lookUpCount)
        {
            try
            {
                using (SqlConnection c = new SqlConnection(conStr.ToString()))
                {
                    c.Open();
                    String query = "proc_GetEconomicFundamentalsCount";

                    SqlCommand sqlCommand = new SqlCommand(query, c);
                    sqlCommand.Parameters.AddWithValue("@StartDateTime", startDate);
                    sqlCommand.Parameters.AddWithValue("@Category", category);

                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    sqlCommand.CommandTimeout = 0;

                    SqlDataReader reader = sqlCommand.ExecuteReader();

                    while (reader.Read())
                    {
                        int dataCount = Convert.ToInt32(reader["DataCount"].ToString());

                        Console.WriteLine("Expected Count :: " + category + " Data.." + dataCount);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.log.Error("Database Error - " + e.Message);
            }
        }

        private double PurifyValues(string value)
        {
            value = value.ToLower();
            double numericDoubleParameter = -1;

            var percentageArray = value.Split('%');

            if (percentageArray.Count() > 1)
            {
                if (String.IsNullOrEmpty(percentageArray[1]))
                {
                    if (Double.TryParse(percentageArray.FirstOrDefault(), out numericDoubleParameter))
                    {
                        return numericDoubleParameter;
                    }
                }
            }

            var thousandsChecker = value.Split('k');

            if (thousandsChecker.Count() > 1)
            {
                if (String.IsNullOrEmpty(thousandsChecker[1]))
                {
                    if (Double.TryParse(thousandsChecker.FirstOrDefault(), out numericDoubleParameter))
                    {
                        return numericDoubleParameter;
                    }
                }
            }

            var millionChecker = value.Split('m');
            if (millionChecker.Count() > 1)
            {
                if (String.IsNullOrEmpty(millionChecker[1]))
                {
                    if (Double.TryParse(millionChecker.FirstOrDefault(), out numericDoubleParameter))
                    {
                        return numericDoubleParameter;
                    }
                }
            }
            return _lackValueCheck;      
        }

        public void PopulateEconomicFundamentalsEssentialsLookUp(string conStr)
        {
            try
            {
                var startDateTime = DateTime.Now;

                using (SqlConnection c = new SqlConnection(conStr.ToString()))
                {
                    c.Open();
                    String query = "proc_GetEconomicFundamentalsEssentials";

                    SqlCommand sqlCommand = new SqlCommand(query, c);
                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    sqlCommand.CommandTimeout = 0;

                    SqlDataReader reader = sqlCommand.ExecuteReader();
                  
                    List<TradeSummary> dayTradeList = new List<TradeSummary>();
                    while (reader.Read())
                    {
                        try
                        {
                            EconomicFundamentalsEssentials economicFundamentals = new EconomicFundamentalsEssentials()
                            {
                                Category = reader["Category"].ToString(),
                                Country = reader["Country"].ToString(),
                                Event = reader["EventType"].ToString(),
                                Currency = reader["Currency"].ToString()                              
                            };
                            economicFundamentalsEssentials.Add(economicFundamentals);
                        }
                        catch (Exception e)
                        {
                           
                        }
                    }
                }


            }
            catch (Exception e)
            {
                Logger.log.Error("Database Error - " + e.Message);
            }         
        }

        public void PopulateEconomicFundamentalsLookUp(string conStr, string category, string startDate, Dictionary<string, EconomicFundamentals> currentLookUp, Dictionary<string, int> currentLookUpCount)
        {
            bool continueFetchingData = true;
            Console.WriteLine("Fetching :: Economic Fundamental Data.." + DateTime.Now.ToString());

           // while (continueFetchingData)
            {
                try
                {
                    var startDateTime = DateTime.Now;
                    Console.WriteLine("Loading Start Time :: " + startDateTime.ToString());

                    using (SqlConnection c = new SqlConnection(conStr.ToString()))
                    {
                        c.Open();
                        String query = "proc_GetEconomicFundamentals";

                        SqlCommand sqlCommand = new SqlCommand(query, c);
                        sqlCommand.Parameters.AddWithValue("@StartDateTime", startDate);
                        sqlCommand.Parameters.AddWithValue("@Category", category);

                        sqlCommand.CommandType = CommandType.StoredProcedure;
                        sqlCommand.CommandTimeout = 0;

                        SqlDataReader reader = sqlCommand.ExecuteReader();
                        String previousCountry = "";

                        List<TradeSummary> dayTradeList = new List<TradeSummary>(); 
                        while (reader.Read())
                        {

                            try
                            {

                                EconomicFundamentals economicFundamentals = new EconomicFundamentals()
                                {
                                    ReleaseDateTime = Convert.ToDateTime(reader["ReleaseDateTime"].ToString()),
                                    Actual = reader["Actual"] == DBNull.Value ? 0.0 : Convert.ToDouble(PurifyValues(reader["Actual"].ToString())),
                                    Previous = reader["Previous"] == DBNull.Value ? 0.0 : Convert.ToDouble(PurifyValues(reader["Previous"].ToString())),
                                    Forecast = reader["Forecast"] == DBNull.Value ? 0.0 : Convert.ToDouble(PurifyValues(reader["Forecast"].ToString())),

                                    Category = reader["Category"].ToString(),
                                    Country = reader["Country"].ToString(),
                                    Currency = reader["Currency"].ToString(),
                                    EventDescription = reader["EventDescription"].ToString(),
                                    EventHeadline = reader["EventHeadline"].ToString(),
                                    EventType = reader["EventType"].ToString(),
                                    Importance = reader["Importance"].ToString(),
                                    InstitutionBody = reader["InstitutionBody"].ToString(),

                                    NumericType = reader["NumericType"] == DBNull.Value ? String.Empty : reader["NumericType"].ToString()
                                };

                                //previousCountry = economicFundamentals.Country;
                                var tempFundamentals = new EconomicFundamentals();

                                //String lookUpID = DateTime.Now.Millisecond + "#" + economicFundamentals.ReleaseDateTime.Ticks.ToString();

                                String lookUpID = economicFundamentals.EventType + "#" + economicFundamentals.Country 
                                    + "#"+ economicFundamentals.ReleaseDateTime.Ticks.ToString();

                                if (currentLookUp.TryGetValue(lookUpID, out tempFundamentals) == false)
                                {
                                    currentLookUp.Add(lookUpID, economicFundamentals);
                                }
                            }
                            catch (Exception e)
                            {
                                int hh = 9;
                            }
                        }
                    }

                    TimeSpan timeSpan = DateTime.Now - startDateTime;

                    Console.WriteLine(category + " Loading End Time :: " + DateTime.Now.ToString() + " Total Time : " + timeSpan.ToString());

                }
                catch (Exception e)
                {
                    Logger.log.Error("Database Error - " + e.Message);
                }

                if (currentLookUp != null)
                {


                    Console.WriteLine("Actual Count :: "+ category +" : " + currentLookUp.Count);
                    Logger.log.Info("Actual Count :: "+ category +" : " + currentLookUp.Count);





                    //int dataCounts = 0;
                    //if (currentLookUpCount.TryGetValue(category, out dataCounts))
                    //{
                    //    if (currentLookUp.Count == dataCounts)
                    //    {
                    //        continueFetchingData = false;
                    //        Console.WriteLine("Data Completed :: " + category);
                    //        Console.WriteLine(" ");
                    //        Logger.log.Info("Data Completed :: " + category + "Row Count :: " + dataCounts);
                    //    }
                    //    else
                    //    {
                    //        Console.WriteLine("Actual Count :: " + currentLookUp.Count + " Expected Count :: " + dataCounts);
                    //        Logger.log.Info("Actual Count :: " + currentLookUp.Count + " Expected Count :: " + dataCounts);

                    //        currentLookUp.Clear();
                    //    }
                    //}



                }
                else
                {
                    continueFetchingData = false;
                    Console.WriteLine(" ");
                }
            }
        }

        public void PopulateLookUp(string conStr, string timeFrame, string startDate, string exchange, Dictionary<string, TradeSummary> currentLookUp, Dictionary<string, int> currentLookUpCount)
        {
            bool continueFetchingData = true;
            Console.WriteLine("Fetching :: " + exchange + " Data.." + DateTime.Now.ToString());

            while (continueFetchingData)
            {
                try
                {
                    var startDateTime = DateTime.Now;
                    Console.WriteLine("Loading Start Time :: " + startDateTime.ToString());

                    using (SqlConnection c = new SqlConnection(conStr.ToString()))
                    {
                        c.Open();
                        String query = "SELECT * FROM dbo.tblTradesWorkingVersion WHERE dbo.tblTradesWorkingVersion.StockExchange = '" + exchange
                            + "' AND dbo.tblTradesWorkingVersion.DateTime > '" + startDate + "' ORDER BY dbo.tblTradesWorkingVersion.SymbolID, dbo.tblTradesWorkingVersion.DateTime ASC";

                        if (exchange == "Forex") query = "SELECT * FROM dbo.tblTradesWorkingVersion WHERE dbo.tblTradesWorkingVersion.TimeFrame = '" + timeFrame
                            + "' AND dbo.tblTradesWorkingVersion.DateTime > '" + startDate + "' ORDER BY dbo.tblTradesWorkingVersion.SymbolID, dbo.tblTradesWorkingVersion.DateTime ASC";

                        SqlCommand sqlCommand = new SqlCommand(query, c);
                        sqlCommand.CommandTimeout = 0;

                        SqlDataReader reader = sqlCommand.ExecuteReader();
                        //if (reader.HasRows)
                        //    Console.WriteLine("Fetching :: " + exchange + " Data.." + DateTime.Now.ToString());

                        String previousSymbolID = "";

                        List<TradeSummary> dayTradeList = new List<TradeSummary>(); ;

                        while (reader.Read())
                        {
                            TradeSummary dayTrade = new TradeSummary();

                            String symbolID = reader["SymbolID"].ToString();

                            dayTrade.SymbolID = symbolID;
                            dayTrade.DateTime = Convert.ToDateTime(reader["DateTime"].ToString());
                            dayTrade.Open = Convert.ToDouble(reader["Open"].ToString());
                            dayTrade.High = Convert.ToDouble(reader["High"].ToString());
                            dayTrade.Low = Convert.ToDouble(reader["Low"].ToString());
                            dayTrade.Close = Convert.ToDouble(reader["Close"].ToString());
                            dayTrade.TimeFrame = reader["TimeFrame"].ToString();

                            dayTrade.Exchange = exchange;

                            //if (exchange != "Forex")
                            {
                                dayTrade.Volume = Convert.ToInt32(reader["Volume"].ToString());
                            }

                            dayTrade.AdjustmentClose = Convert.ToDouble(reader["AdjustmentClose"].ToString());

                            previousSymbolID = symbolID;

                            String lookUpID = symbolID + "#" + dayTrade.DateTime.Ticks.ToString();

                            currentLookUp.Add(lookUpID, dayTrade);

                        }
                    }

                    TimeSpan timeSpan = DateTime.Now - startDateTime;

                    Console.WriteLine(timeFrame + " Loading End Time :: " + DateTime.Now.ToString() + " Total Time : " + timeSpan.ToString());

                }
                catch (Exception e)
                {
                    Logger.log.Error("Database Error - " + e.Message);
                }

                if (currentLookUp != null)
                {
                    int dataCounts = 0;
                    if (currentLookUpCount.TryGetValue(timeFrame, out dataCounts))
                    {
                        if (currentLookUp.Count == dataCounts)
                        {
                            continueFetchingData = false;
                            Console.WriteLine("Data Completed :: " + timeFrame);
                            Console.WriteLine(" ");
                            Logger.log.Info("Data Completed :: " + timeFrame + "Row Count :: " + dataCounts);
                        }
                        else
                        {
                            Console.WriteLine("Actual Count :: " + currentLookUp.Count + " Expected Count :: " + dataCounts);
                            Logger.log.Info("Actual Count :: " + currentLookUp.Count + " Expected Count :: " + dataCounts);

                            currentLookUp.Clear();
                        }
                    }
                }
                else
                {
                    continueFetchingData = false;
                    Console.WriteLine(" ");
                }
            }
        }

        public List<TradeSummary> GetTradeSummaries(List<String> symbolList, DateTime startDate, DateTime endDate, String exchange, String timeFrame)
        {
            List<TradeSummary> dtList = new List<TradeSummary>();
            long dayTicks = 864000000000;

            switch (exchange)
            {
                case "LSE":
                    {
                        List<String> lookUpIDList = new List<String>();

                        for (int f = 0; f < symbolList.Count; f++)
                        {
                            var temp = symbolList[f] + "#";

                            long tempCurrent = startDate.Ticks;
                            while (tempCurrent <= endDate.Ticks)
                            {
                                String lookUpIDTemp = temp + tempCurrent.ToString();
                                lookUpIDList.Add(lookUpIDTemp);

                                tempCurrent = tempCurrent + dayTicks;
                            }
                        }

                        for (int i = 0; i < lookUpIDList.Count; i++)
                        {
                            TradeSummary resultDayTradeSum = new TradeSummary();

                            bool bFound = dayTradeSummaries_LSE_LOOKUP.TryGetValue(lookUpIDList[i], out resultDayTradeSum);
                            if (bFound)
                            {
                                dtList.Add(resultDayTradeSum);
                            }
                        }


                    } break;

                case "NASDAQ":
                    {
                        List<String> lookUpIDList = new List<String>();

                        for (int f = 0; f < symbolList.Count; f++)
                        {
                            var temp = symbolList[f] + "#";

                            long tempCurrent = startDate.Ticks;
                            while (tempCurrent <= endDate.Ticks)
                            {
                                String lookUpIDTemp = temp + tempCurrent.ToString();
                                lookUpIDList.Add(lookUpIDTemp);

                                tempCurrent = tempCurrent + dayTicks;
                            }
                        }

                        for (int i = 0; i < lookUpIDList.Count; i++)
                        {
                            TradeSummary resultDayTradeSum = new TradeSummary();

                            bool bFound = dayTradeSummaries_NASDAQ_LOOKUP.TryGetValue(lookUpIDList[i], out resultDayTradeSum);
                            if (bFound)
                            {
                                dtList.Add(resultDayTradeSum);
                            }
                        }

                    } break;

                case "NYSE":
                    {
                        List<String> lookUpIDList = new List<String>();

                        for (int f = 0; f < symbolList.Count; f++)
                        {
                            var temp = symbolList[f] + "#";

                            long tempCurrent = startDate.Ticks;
                            while (tempCurrent <= endDate.Ticks)
                            {
                                String lookUpIDTemp = temp + tempCurrent.ToString();
                                lookUpIDList.Add(lookUpIDTemp);

                                tempCurrent = tempCurrent + dayTicks;
                            }
                        }

                        for (int i = 0; i < lookUpIDList.Count; i++)
                        {
                            TradeSummary resultDayTradeSum = new TradeSummary();

                            bool bFound = dayTradeSummaries_NYSE_LOOKUP.TryGetValue(lookUpIDList[i], out resultDayTradeSum);
                            if (bFound)
                            {
                                dtList.Add(resultDayTradeSum);
                            }
                        }

                    } break;

                case "AMEX":
                    {
                        List<String> lookUpIDList = new List<String>();

                        for (int f = 0; f < symbolList.Count; f++)
                        {
                            var temp = symbolList[f] + "#";

                            long tempCurrent = startDate.Ticks;
                            while (tempCurrent <= endDate.Ticks)
                            {
                                String lookUpIDTemp = temp + tempCurrent.ToString();
                                lookUpIDList.Add(lookUpIDTemp);

                                tempCurrent = tempCurrent + dayTicks;
                            }
                        }


                        for (int i = 0; i < lookUpIDList.Count; i++)
                        {
                            TradeSummary resultDayTradeSum = new TradeSummary();

                            bool bFound = dayTradeSummaries_AMEX_LOOKUP.TryGetValue(lookUpIDList[i], out resultDayTradeSum);
                            if (bFound)
                            {
                                dtList.Add(resultDayTradeSum);
                            }
                        }

                    } break;
                
                case "FOREX":
                case "Forex":
                    {
                        RetrievalCheck(symbolList, startDate, endDate, exchange, timeFrame, dtList, false);

                    } break;
            }
            return dtList;
        }

        public bool RetrievalCheckEconomicFundamentals(List<string> eventTypeList, DateTime startDate, DateTime endDate, string category, string country, List<EconomicFundamentals> economicFundamentalListing, bool check)
        {
            //long selectedTicks = 864000000000;
            long selectedTicks = TimeSpan.TicksPerMinute;
            
            Dictionary<string, EconomicFundamentals> lookUpEconomics = SelectEconomicFundamentals(category);

           // CheckMainDataStoreEconomicFundamentals(symbolList, startDate, exchange, timeFrame, lookUpTradeSummaries);

            if (lookUpEconomics != null)
            {
                String eventItem = eventTypeList.FirstOrDefault();

                // if (symbolData.ToUpper() == "ALLCURRENCYPAIRS")
                //{
                //    eventTypeList = (lookUpEconomics.Select(m => m.Value.EventType)).Distinct().ToList();
                //}

                List<String> lookUpIDList = new List<String>();

                //for (int f = 0; f < eventTypeList.Count; f++)
                {
                    //var temp = eventTypeList[f] + "#";
                    //var temp = eventTypeList[f] + "#" + country;
                    var temp = eventItem + "#" + country + "#";

                    long tempCurrent = startDate.Ticks;
                    while (tempCurrent <= endDate.Ticks)
                    {
                        String lookUpIDTemp = temp + tempCurrent.ToString();
                        lookUpIDList.Add(lookUpIDTemp);

                        tempCurrent = tempCurrent + selectedTicks;
                    }
                }

                for (int i = 0; i < lookUpIDList.Count; i++)
                {
                    EconomicFundamentals economicFundamentals = new EconomicFundamentals();

                    bool bFound = lookUpEconomics.TryGetValue(lookUpIDList[i], out economicFundamentals);
                    if (bFound)
                    {
                        if (check == false) { economicFundamentalListing.Add(economicFundamentals); }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
            return false;
        }

        private List<string> GetCurrency(string symbol)
        {
            List<string> currencies = new List<string>();
            var distinctList = economicFundamentalsEssentials.Select(k => k.Currency).Distinct();

            foreach (var item in distinctList)
            {
                var index = symbol.IndexOf(item);
                if (index >= 0)
                {
                    currencies.Add(item);
                }
            }
            return currencies;
        }

        //do this in the AnswerLibrary
        private EconomicFundamentalsEssentials EconomicFundamentalsParametersLookup(List<string> symbolList, string eventType, string category, string country)
        {
            var econEssen = new EconomicFundamentalsEssentials();

            //eventype, symbol
            if (String.IsNullOrEmpty(country) && String.IsNullOrEmpty(category))
            {
                var currency = GetCurrency(symbolList.FirstOrDefault());
                
                foreach (var item in currency)
                {
                    var found = economicFundamentalsEssentials.Where(m => m.Event == eventType 
                        && m.Currency == item);
                    
                    if (found.Any())
                    {
                        econEssen = found.FirstOrDefault();
                    }
                }
            }

           if (true)
            {

            }

            if (true)
            {

            }
            return econEssen;


            //eventype, country

            //category, country, symbolist
        }

        public List<EconomicFundamentals> GetEconomicFundamentals(List<string> symbolList, List<string> eventTypeList, DateTime startDate, DateTime endDate, string category, string country)
        {
            List<EconomicFundamentals> dataList = new List<EconomicFundamentals>();

            var econParameters = EconomicFundamentalsParametersLookup(symbolList, eventTypeList.FirstOrDefault(), category, country);

            RetrievalCheckEconomicFundamentals(new List<string>() {econParameters.Event}, startDate, endDate, econParameters.Category, econParameters.Country, dataList, false);
            
            //assumption made here
            dataList.ForEach(n => n.AssociatedSymbolID = symbolList.FirstOrDefault());

            return dataList;
        }

        private bool RetrievalCheck(List<String> symbolList, DateTime startDate, DateTime endDate, String exchange, String timeFrame, List<TradeSummary> dtList, bool bCheck)
        {
            long selectedTicks = 864000000000;

            Dictionary<String, TradeSummary> lookUpTradeSummaries = SelectTradeSummary(timeFrame, out selectedTicks);

            CheckMainDataStore(symbolList, startDate, exchange, timeFrame, lookUpTradeSummaries);

            if (lookUpTradeSummaries != null)
            {
                String symbolData = symbolList.FirstOrDefault();

                if (symbolData.ToUpper() == "ALLCURRENCYPAIRS")
                {
                    symbolList = (lookUpTradeSummaries.Select(m => m.Value.SymbolID)).Distinct().ToList();
                }


                List<String> lookUpIDList = new List<String>();

                for (int f = 0; f < symbolList.Count; f++)
                {
                    var temp = symbolList[f] + "#";

                    long tempCurrent = startDate.Ticks;
                    while (tempCurrent <= endDate.Ticks)
                    {
                        String lookUpIDTemp = temp + tempCurrent.ToString();
                        lookUpIDList.Add(lookUpIDTemp);

                        tempCurrent = tempCurrent + selectedTicks;
                    }
                }

                for (int i = 0; i < lookUpIDList.Count; i++)
                {
                    TradeSummary resultDayTradeSum = new TradeSummary();

                    bool bFound = lookUpTradeSummaries.TryGetValue(lookUpIDList[i], out resultDayTradeSum);
                    if (bFound)
                    {
                        if (bCheck == false) { dtList.Add(resultDayTradeSum); }
                        else
                        {
                            return false;
                        }
                    }
                }

            }
            return true;
        }
        //private bool RetrievalCheck(List<String> symbolList, DateTime startDate, DateTime endDate, String exchange, String timeFrame, List<TradeSummary> dtList, bool bCheck)
        //{
        //    long dayTicks = 864000000000;

        //    Dictionary<String, TradeSummary> lookUpTradeSummaries = SelectTradeSummary(timeFrame);

        //    CheckMainDataStore(symbolList, startDate, exchange, timeFrame, lookUpTradeSummaries);

        //    if (lookUpTradeSummaries != null)
        //    {
        //        String symbolData = symbolList.FirstOrDefault();

        //        if (symbolData == "ALLCURRENCYPAIRS" || symbolData == "allcurrencypairs")
        //        {
        //            foreach (var dataItem in lookUpTradeSummaries)
        //            {
        //                dtList.Add(dataItem.Value);
        //            }
        //        }
        //        else
        //        {
        //            List<String> lookUpIDList = new List<String>();

        //            for (int f = 0; f < symbolList.Count; f++)
        //            {
        //                var temp = symbolList[f] + "#";

        //                long tempCurrent = startDate.Ticks;
        //                while (tempCurrent <= endDate.Ticks)
        //                {
        //                    String lookUpIDTemp = temp + tempCurrent.ToString();
        //                    lookUpIDList.Add(lookUpIDTemp);

        //                    tempCurrent = tempCurrent + dayTicks;
        //                }
        //            }

        //            for (int i = 0; i < lookUpIDList.Count; i++)
        //            {
        //                TradeSummary resultDayTradeSum = new TradeSummary();

        //                bool bFound = lookUpTradeSummaries.TryGetValue(lookUpIDList[i], out resultDayTradeSum);
        //                if (bFound)
        //                {
        //                    if (bCheck == false) { dtList.Add(resultDayTradeSum); } 
        //                    else
        //                    {
        //                        return false;
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    return true;
        //}

        private void CheckMainDataStore(List<String> symbolList, DateTime startDate, String exchange, String timeFrame, Dictionary<String, TradeSummary> lookUpTradeSummaries)
        {
            String symbolData = symbolList.FirstOrDefault();
            var dataFetchList = new List<DataFetchCriteria>();

            if (symbolData.ToUpper() == "ALLCURRENCYPAIRS")
            {
                symbolList = (lookUpTradeSummaries.Select(m => m.Value.SymbolID)).Distinct().ToList();
            }

            if (symbolList != null)
            {
                foreach (var item in symbolList)
                {
                    var dataFetch = new DataFetchCriteria();

                    if (lookUpTradeSummaries.Where(m => m.Value.SymbolID == item && m.Value.TimeFrame == timeFrame
                        && m.Value.DateTime <= startDate).Any() == false)
                    {
                        dataFetch.SymbolID = item;
                        dataFetch.TimeFrame = timeFrame;
                        dataFetch.StartDateTimeStr = startDate.ToString("yyyy-MM-dd");

                        dataFetchList.Add(dataFetch);
                    }
                }

                //Get Data from db
                UpdateDataCollection(dataFetchList);
            }

        }

        private void CheckMainDataStoreEconomicFundamentals(List<String> symbolList, DateTime startDate, String exchange, String timeFrame, Dictionary<String, TradeSummary> lookUpTradeSummaries)
        {
            String symbolData = symbolList.FirstOrDefault();
            var dataFetchList = new List<DataFetchCriteria>();

            if (symbolData.ToUpper() == "ALLCURRENCYPAIRS")
            {
                symbolList = (lookUpTradeSummaries.Select(m => m.Value.SymbolID)).Distinct().ToList();
            }

            if (symbolList != null)
            {
                foreach (var item in symbolList)
                {
                    var dataFetch = new DataFetchCriteria();

                    if (lookUpTradeSummaries.Where(m => m.Value.SymbolID == item && m.Value.TimeFrame == timeFrame
                        && m.Value.DateTime <= startDate).Any() == false)
                    {
                        dataFetch.SymbolID = item;
                        dataFetch.TimeFrame = timeFrame;
                        dataFetch.StartDateTimeStr = startDate.ToString("yyyy-MM-dd");

                        dataFetchList.Add(dataFetch);
                    }
                }
                //Get Data from db
                UpdateDataCollection(dataFetchList);
            }
        }


        private void UpdateDataCollection(List<DataFetchCriteria> criteria)
        {
            String stockExchange = "Forex";

            String selector = "TRADES";
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

            if (stockExchange == "Forex") selector += "_" + stockExchange;

            var settings = System.Configuration.ConfigurationManager.ConnectionStrings[selector];

            foreach (var item in criteria)
            {
                try
                {
                    using (SqlConnection c = new SqlConnection(settings.ToString()))
                    {
                        c.Open();

                        String query = "SELECT * FROM dbo.tblTradesWorkingVersion WHERE dbo.tblTradesWorkingVersion.StockExchange = '" + stockExchange + "' AND dbo.tblTradesWorkingVersion.DateTime > '"
                            + item.StartDateTimeStr + "' ORDER BY dbo.tblTradesWorkingVersion.SymbolID, dbo.tblTradesWorkingVersion.DateTime ASC";

                        if (stockExchange == "Forex") query = "SELECT * FROM dbo.tblTradesWorkingVersion WHERE dbo.tblTradesWorkingVersion.TimeFrame = '" + item.TimeFrame
                            + "' AND dbo.tblTradesWorkingVersion.SymbolID = '" + item.SymbolID + "' AND dbo.tblTradesWorkingVersion.DateTime > '" + item.StartDateTimeStr + "' ORDER BY dbo.tblTradesWorkingVersion.SymbolID, dbo.tblTradesWorkingVersion.DateTime ASC";

                        SqlCommand sqlCommand = new SqlCommand(query, c);
                        sqlCommand.CommandTimeout = 0;

                        SqlDataReader reader = sqlCommand.ExecuteReader();
                        if (reader.HasRows)
                            Console.WriteLine("Fetching :: " + item + " Data.." + DateTime.Now.ToString());

                        String previousSymbolID = "";

                        List<TradeSummary> dayTradeList = new List<TradeSummary>(); ;

                        int count = 0;

                        while (reader.Read())
                        {
                            TradeSummary dayTrade = new TradeSummary();

                            String symbolID = reader["SymbolID"].ToString();

                            dayTrade.SymbolID = symbolID;
                            dayTrade.DateTime = Convert.ToDateTime(reader["DateTime"].ToString());
                            dayTrade.Open = Convert.ToDouble(reader["Open"].ToString());
                            dayTrade.High = Convert.ToDouble(reader["High"].ToString());
                            dayTrade.Low = Convert.ToDouble(reader["Low"].ToString());
                            dayTrade.Close = Convert.ToDouble(reader["Close"].ToString());
                            dayTrade.TimeFrame = reader["TimeFrame"].ToString();

                            dayTrade.Exchange = stockExchange;

                            if (stockExchange != "Forex")
                            {
                                dayTrade.Volume = Convert.ToInt32(reader["Volume"].ToString());
                            }

                            dayTrade.AdjustmentClose = Convert.ToDouble(reader["AdjustmentClose"].ToString());

                            previousSymbolID = symbolID;

                            String lookUpID = symbolID + "#" + dayTrade.DateTime.Ticks.ToString();

                            switch (stockExchange)
                            {
                                case "LSE":
                                    {
                                        dayTradeSummaries_LSE_LOOKUP.Add(lookUpID, dayTrade);
                                    } break;

                                case "NASDAQ":
                                    {
                                        dayTradeSummaries_NASDAQ_LOOKUP.Add(lookUpID, dayTrade);
                                    } break;

                                case "NYSE":
                                    {
                                        dayTradeSummaries_NYSE_LOOKUP.Add(lookUpID, dayTrade);
                                    } break;

                                case "AMEX":
                                    {
                                        dayTradeSummaries_AMEX_LOOKUP.Add(lookUpID, dayTrade);
                                    } break;

                                case "Forex":
                                    {
                                        TradeSummary tradeSum = null;
                                        long ticks = 0;

                                        var tradeCollection = SelectTradeSummary(item.TimeFrame, out ticks);

                                        if (tradeCollection.TryGetValue(lookUpID, out tradeSum) == false)
                                        {
                                            tradeCollection.Add(lookUpID, dayTrade);
                                        }                                  

                                    } break;
                            }
                            count++;
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.log.Error("Database Error - " + e.Message);
                }
            }
        }

        public bool DataExistForTradeSummaries(List<String> symbolList, DateTime startDate, DateTime endDate, String exchange, String timeFrame)
        {
            List<TradeSummary> dtList = new List<TradeSummary>();
            long dayTicks = 864000000000;

            switch (exchange)
            {
                case "LSE":
                    {
                        List<String> lookUpIDList = new List<String>();

                        for (int f = 0; f < symbolList.Count; f++)
                        {
                            var temp = symbolList[f] + "#";

                            long tempCurrent = startDate.Ticks;
                            while (tempCurrent <= endDate.Ticks)
                            {
                                String lookUpIDTemp = temp + tempCurrent.ToString();
                                lookUpIDList.Add(lookUpIDTemp);

                                tempCurrent = tempCurrent + dayTicks;
                            }
                        }

                        for (int i = 0; i < lookUpIDList.Count; i++)
                        {
                            TradeSummary resultDayTradeSum = new TradeSummary();

                            bool bFound = dayTradeSummaries_LSE_LOOKUP.TryGetValue(lookUpIDList[i], out resultDayTradeSum);
                            if (bFound)
                            {
                                dtList.Add(resultDayTradeSum);
                            }
                        }


                    } break;

                case "NASDAQ":
                    {
                        List<String> lookUpIDList = new List<String>();

                        for (int f = 0; f < symbolList.Count; f++)
                        {
                            var temp = symbolList[f] + "#";

                            long tempCurrent = startDate.Ticks;
                            while (tempCurrent <= endDate.Ticks)
                            {
                                String lookUpIDTemp = temp + tempCurrent.ToString();
                                lookUpIDList.Add(lookUpIDTemp);

                                tempCurrent = tempCurrent + dayTicks;
                            }
                        }

                        for (int i = 0; i < lookUpIDList.Count; i++)
                        {
                            TradeSummary resultDayTradeSum = new TradeSummary();

                            bool bFound = dayTradeSummaries_NASDAQ_LOOKUP.TryGetValue(lookUpIDList[i], out resultDayTradeSum);
                            if (bFound)
                            {
                                dtList.Add(resultDayTradeSum);
                            }
                        }

                    } break;

                case "NYSE":
                    {
                        List<String> lookUpIDList = new List<String>();

                        for (int f = 0; f < symbolList.Count; f++)
                        {
                            var temp = symbolList[f] + "#";

                            long tempCurrent = startDate.Ticks;
                            while (tempCurrent <= endDate.Ticks)
                            {
                                String lookUpIDTemp = temp + tempCurrent.ToString();
                                lookUpIDList.Add(lookUpIDTemp);

                                tempCurrent = tempCurrent + dayTicks;
                            }
                        }

                        for (int i = 0; i < lookUpIDList.Count; i++)
                        {
                            TradeSummary resultDayTradeSum = new TradeSummary();

                            bool bFound = dayTradeSummaries_NYSE_LOOKUP.TryGetValue(lookUpIDList[i], out resultDayTradeSum);
                            if (bFound)
                            {
                                dtList.Add(resultDayTradeSum);
                            }
                        }

                    } break;

                case "AMEX":
                    {
                        List<String> lookUpIDList = new List<String>();

                        for (int f = 0; f < symbolList.Count; f++)
                        {
                            var temp = symbolList[f] + "#";

                            long tempCurrent = startDate.Ticks;
                            while (tempCurrent <= endDate.Ticks)
                            {
                                String lookUpIDTemp = temp + tempCurrent.ToString();
                                lookUpIDList.Add(lookUpIDTemp);

                                tempCurrent = tempCurrent + dayTicks;
                            }
                        }


                        for (int i = 0; i < lookUpIDList.Count; i++)
                        {
                            TradeSummary resultDayTradeSum = new TradeSummary();

                            bool bFound = dayTradeSummaries_AMEX_LOOKUP.TryGetValue(lookUpIDList[i], out resultDayTradeSum);
                            if (bFound)
                            {
                                dtList.Add(resultDayTradeSum);
                            }
                        }

                    } break;

                case "FOREX":
                case "Forex":
                    {
                        if (RetrievalCheck(symbolList, startDate, endDate, exchange, timeFrame, dtList, false) == false)
                        {
                            return false;
                        }
                    } break;
            }
            return true;
        }

        private Dictionary<String, TradeSummary> SelectTradeSummary(string timeFrame, out long selectedTicks)
        {
            Dictionary<String, TradeSummary> lookUpTradeSummaries = new Dictionary<string, TradeSummary>();
            selectedTicks = new TimeSpan(1, 0, 0, 0).Ticks;

            //t6urn this into a method
            switch (timeFrame)
            {
                case "1min":
                    {
                        lookUpTradeSummaries = oneMinuteTradeSummaries_FOREX_LOOKUP_X;
                        selectedTicks = new TimeSpan(0, 1, 0).Ticks;
                    } break;

                case "5min":
                    {
                        lookUpTradeSummaries = fiveMinuteTradeSummaries_FOREX_LOOKUP_X;
                        selectedTicks = new TimeSpan(0, 5, 0).Ticks;
                    } break;

                case "15min":
                    {
                        lookUpTradeSummaries = fifteenMinuteTradeSummaries_FOREX_LOOKUPX;
                        selectedTicks = new TimeSpan(0, 15, 0).Ticks;

                    } break;

                case "30min":
                    {
                        lookUpTradeSummaries = thrityMinuteTradeSummaries_FOREX_LOOKUPX;
                        selectedTicks = new TimeSpan(0, 30, 0).Ticks;

                    } break;

                case "1hour":
                    {
                        lookUpTradeSummaries = oneHourTradeSummaries_FOREX_LOOKUPX;
                        selectedTicks = new TimeSpan(1, 0, 0).Ticks;

                    } break;

                case "2hour":
                    {
                        lookUpTradeSummaries = twoHourTradeSummaries_FOREX_LOOKUPX;
                        selectedTicks = new TimeSpan(2, 0, 0).Ticks;

                    } break;

                case "3hour":
                    {
                        lookUpTradeSummaries = threeHourTradeSummaries_FOREX_LOOKUPX;
                        selectedTicks = new TimeSpan(3, 0, 0).Ticks;

                    } break;

                case "EndOfDay":
                    {
                        lookUpTradeSummaries = dayTradeSummaries_FOREX_LOOKUP;
                        selectedTicks = new TimeSpan(1, 0, 0, 0).Ticks;

                    } break;
            }

            return lookUpTradeSummaries;
        }

        private Dictionary<string, EconomicFundamentals> SelectEconomicFundamentals(string category)
        {
            Dictionary<string, EconomicFundamentals> lookUpEconomicFundamentals = new Dictionary<string, EconomicFundamentals>();
            //selectedTicks = new TimeSpan(1, 0, 0, 0).Ticks;

            //t6urn this into a method
            switch (category)
            {
                case "none":
                case "central bank":
                    {
                        lookUpEconomicFundamentals = centralBankFundamentals;
                    } break;

                case "employment":
                    {
                        lookUpEconomicFundamentals = employmentFundamentals;
                    } break;                
            }
            return lookUpEconomicFundamentals;
        }


        public List<DataTable> GetDatabaseData(String query)
        {
            //List or DataSet
            List<DataTable> dataTableList = new List<DataTable>();
            return dataTableList;
        }
    }



    [DataContract]
    public class TradeSummary
    {
        private DateTime _dateTime;
        private double _open;
        private double _high;
        private double _low;
        private double _close;
        private double _adjustmentclose;
        private int _volume;
        private String _timeFrame;
        private String _exchange;

        private String _symbolID;

        [DataMember]
        public DateTime DateTime
        {
            get { return _dateTime; }
            set { _dateTime = value; }
        }

        [DataMember]
        public double Open
        {
            get { return _open; }
            set { _open = value; }
        }

        [DataMember]
        public double High
        {
            get { return _high; }
            set { _high = value; }
        }

        [DataMember]
        public double Low
        {
            get { return _low; }
            set { _low = value; }
        }

        [DataMember]
        public double Close
        {
            get { return _close; }
            set { _close = value; }
        }

        [DataMember]
        public int Volume
        {
            get { return _volume; }
            set { _volume = value; }
        }

        [DataMember]
        public double AdjustmentClose
        {
            get { return _adjustmentclose; }
            set { _adjustmentclose = value; }
        }

        [DataMember]
        public String TimeFrame
        {
            get { return _timeFrame; }
            set { _timeFrame = value; }
        }

        [DataMember]
        public String SymbolID
        {
            get { return _symbolID; }
            set { _symbolID = value; }
        }

        [DataMember]
        public String Exchange
        {
            get { return _exchange; }
            set { _exchange = value; }
        }
    }

}
