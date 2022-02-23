using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace DatabaseWcfService
{
    public class InMemoryDB
    {
        //exchange / data
        public static Dictionary<String, List<TradeSummary>> dayTradeSummaries_LSE = new Dictionary<String, List<TradeSummary>>();
        public static Dictionary<String, List<TradeSummary>> dayTradeSummaries_NYSE = new Dictionary<String, List<TradeSummary>>();
        public static Dictionary<String, List<TradeSummary>> dayTradeSummaries_AMEX = new Dictionary<String, List<TradeSummary>>();
        public static Dictionary<String, List<TradeSummary>> dayTradeSummaries_NASDAQ = new Dictionary<String, List<TradeSummary>>();


        public Dictionary<String, TradeSummary> dayTradeSummaries_AMEX_LOOKUP = new Dictionary<String, TradeSummary>();
        public Dictionary<String, TradeSummary> dayTradeSummaries_NYSE_LOOKUP = new Dictionary<String, TradeSummary>();
        public Dictionary<String, TradeSummary> dayTradeSummaries_NASDAQ_LOOKUP = new Dictionary<String, TradeSummary>();
        public Dictionary<String, TradeSummary> dayTradeSummaries_LSE_LOOKUP = new Dictionary<String, TradeSummary>();


        //INDICES Lookups
        public Dictionary<String, TradeSummary> oneMinuteTradeSummaries_INDICES_LOOKUP = new Dictionary<String, TradeSummary>();
        public Dictionary<String, TradeSummary> fiveMinuteTradeSummaries_INDICES_LOOKUP = new Dictionary<String, TradeSummary>();
        public Dictionary<String, TradeSummary> tenMinuteTradeSummaries_INDICES_LOOKUP = new Dictionary<String, TradeSummary>();
        public Dictionary<String, TradeSummary> fifteenMinuteTradeSummaries_INDICES_LOOKUP = new Dictionary<String, TradeSummary>();
        public Dictionary<String, TradeSummary> thrityMinuteTradeSummaries_INDICES_LOOKUP = new Dictionary<String, TradeSummary>();
        public Dictionary<String, TradeSummary> oneHourTradeSummaries_INDICES_LOOKUP = new Dictionary<String, TradeSummary>();
        public Dictionary<String, TradeSummary> twoHourTradeSummaries_INDICES_LOOKUP = new Dictionary<String, TradeSummary>();
        public Dictionary<String, TradeSummary> threeHourTradeSummaries_INDICES_LOOKUP = new Dictionary<String, TradeSummary>();
        public Dictionary<String, TradeSummary> fourHourTradeSummaries_INDICES_LOOKUP = new Dictionary<String, TradeSummary>();
        public Dictionary<String, TradeSummary> dayTradeSummaries_INDICES_LOOKUP = new Dictionary<String, TradeSummary>();


        //COMMODITIES Lookups
        public Dictionary<String, TradeSummary> oneMinuteTradeSummaries_COMMODITIES_LOOKUP = new Dictionary<String, TradeSummary>();
        public Dictionary<String, TradeSummary> fiveMinuteTradeSummaries_COMMODITIES_LOOKUP = new Dictionary<String, TradeSummary>();
        public Dictionary<String, TradeSummary> tenMinuteTradeSummaries_COMMODITIES_LOOKUP = new Dictionary<String, TradeSummary>();
        public Dictionary<String, TradeSummary> fifteenMinuteTradeSummaries_COMMODITIES_LOOKUP = new Dictionary<String, TradeSummary>();
        public Dictionary<String, TradeSummary> thrityMinuteTradeSummaries_COMMODITIES_LOOKUP = new Dictionary<String, TradeSummary>();
        public Dictionary<String, TradeSummary> oneHourTradeSummaries_COMMODITIES_LOOKUP = new Dictionary<String, TradeSummary>();
        public Dictionary<String, TradeSummary> twoHourTradeSummaries_COMMODITIES_LOOKUP = new Dictionary<String, TradeSummary>();
        public Dictionary<String, TradeSummary> threeHourTradeSummaries_COMMODITIES_LOOKUP = new Dictionary<String, TradeSummary>();
        public Dictionary<String, TradeSummary> fourHourTradeSummaries_COMMODITIES_LOOKUP = new Dictionary<String, TradeSummary>();
        public Dictionary<String, TradeSummary> dayTradeSummaries_COMMODITIES_LOOKUP = new Dictionary<String, TradeSummary>();


        //Forex Lookups
        public Dictionary<String, TradeSummary> oneMinuteTradeSummaries_FOREX_LOOKUP_X = new Dictionary<String, TradeSummary>();
        public Dictionary<String, TradeSummary> fiveMinuteTradeSummaries_FOREX_LOOKUP_X = new Dictionary<String, TradeSummary>();
        public Dictionary<String, TradeSummary> tenMinuteTradeSummaries_FOREX_LOOKUP_X = new Dictionary<String, TradeSummary>();
        public Dictionary<String, TradeSummary> fifteenMinuteTradeSummaries_FOREX_LOOKUPX = new Dictionary<String, TradeSummary>();
        public Dictionary<String, TradeSummary> thrityMinuteTradeSummaries_FOREX_LOOKUPX = new Dictionary<String, TradeSummary>();
        public Dictionary<String, TradeSummary> oneHourTradeSummaries_FOREX_LOOKUPX = new Dictionary<String, TradeSummary>();
        public Dictionary<String, TradeSummary> twoHourTradeSummaries_FOREX_LOOKUPX = new Dictionary<String, TradeSummary>();
        public Dictionary<String, TradeSummary> threeHourTradeSummaries_FOREX_LOOKUPX = new Dictionary<String, TradeSummary>();
        public Dictionary<String, TradeSummary> fourHourTradeSummaries_FOREX_LOOKUPX = new Dictionary<String, TradeSummary>();
        public Dictionary<String, TradeSummary> dayTradeSummaries_FOREX_LOOKUP = new Dictionary<String, TradeSummary>();


        //Economic Fundamental
        public Dictionary<String, EconomicFundamentals> employmentFundamentals = new Dictionary<String, EconomicFundamentals>();
        public Dictionary<String, EconomicFundamentals> centralBankFundamentals = new Dictionary<String, EconomicFundamentals>();
        public Dictionary<String, EconomicFundamentals> economicActivityFundamentals = new Dictionary<String, EconomicFundamentals>();
        public Dictionary<String, EconomicFundamentals> confidenceIndexFundamentals = new Dictionary<String, EconomicFundamentals>();
        public Dictionary<String, EconomicFundamentals> inflationFundamentals = new Dictionary<String, EconomicFundamentals>();
        public Dictionary<String, EconomicFundamentals> bondsFundamentals = new Dictionary<String, EconomicFundamentals>();
        public Dictionary<String, EconomicFundamentals> balanceFundamentals = new Dictionary<String, EconomicFundamentals>();
        public Dictionary<String, EconomicFundamentals> creditFundamentals = new Dictionary<String, EconomicFundamentals>();
        
        public Dictionary<String, EconomicFundamentals> sanctionEvents = new Dictionary<String, EconomicFundamentals>();
        public Dictionary<String, EconomicFundamentals> pandemicEvents = new Dictionary<String, EconomicFundamentals>();
        public Dictionary<String, EconomicFundamentals> warEvents = new Dictionary<String, EconomicFundamentals>();

        public List<EconomicFundamentalsEssentials> economicFundamentalsEssentials = new List<EconomicFundamentalsEssentials>();

        public Dictionary<string, string> nameLookUp = new Dictionary<string, string>();
        public Dictionary<string, string> exchangeLookUp = new Dictionary<string, string>();

        static public MODE Mode = MODE.Test;
        private const double _lackValueCheck = -999999.9999;


        public InMemoryDB()
        {
            //ThreadStart threadStart = new ThreadStart(InitialseDataTables);
            //Thread initialseDataTablesThread = new Thread(threadStart);
            //initialseDataTablesThread.Start();

            Exchanges.InitialiseExchangeList();
        }

        //private void InitialseDataTables()
        //{
        //    //Listen for updates
        //    //ThreadStart threadStart = new ThreadStart(DataUpdateListenner);
        //    //Thread jobCreatorThread = new Thread(threadStart);
        //    //jobCreatorThread.Start();

        //    PopulateDataTables();
        //}

        public void ClearAllData()
        {
            //clear data
            dayTradeSummaries_LSE.Clear();
            dayTradeSummaries_NYSE.Clear();
            dayTradeSummaries_NASDAQ.Clear();
            dayTradeSummaries_AMEX.Clear();

            //Commodities
            oneMinuteTradeSummaries_COMMODITIES_LOOKUP.Clear(); ;
            fiveMinuteTradeSummaries_COMMODITIES_LOOKUP.Clear();
            tenMinuteTradeSummaries_COMMODITIES_LOOKUP.Clear();
            fifteenMinuteTradeSummaries_COMMODITIES_LOOKUP.Clear();
            thrityMinuteTradeSummaries_COMMODITIES_LOOKUP.Clear();
            oneHourTradeSummaries_COMMODITIES_LOOKUP.Clear();
            twoHourTradeSummaries_COMMODITIES_LOOKUP.Clear();
            threeHourTradeSummaries_COMMODITIES_LOOKUP.Clear();
            fourHourTradeSummaries_COMMODITIES_LOOKUP.Clear();
            dayTradeSummaries_COMMODITIES_LOOKUP.Clear();


            //Forex
            oneMinuteTradeSummaries_FOREX_LOOKUP_X.Clear(); ;
            fiveMinuteTradeSummaries_FOREX_LOOKUP_X.Clear();
            tenMinuteTradeSummaries_FOREX_LOOKUP_X.Clear();
            fifteenMinuteTradeSummaries_FOREX_LOOKUPX.Clear();
            thrityMinuteTradeSummaries_FOREX_LOOKUPX.Clear();
            oneHourTradeSummaries_FOREX_LOOKUPX.Clear();
            twoHourTradeSummaries_FOREX_LOOKUPX.Clear();
            threeHourTradeSummaries_FOREX_LOOKUPX.Clear();
            fourHourTradeSummaries_FOREX_LOOKUPX.Clear();
            dayTradeSummaries_FOREX_LOOKUP.Clear();
        }

        public void PopulateDataTables()
        {
            Console.WriteLine("\n\n");
            LoadExchangeLookUp();
            LoadSymbolUsageName();

            ClearAllData();

            Console.WriteLine("Data fetching in progress");
            Library.WriteErrorLog(DateTime.Now + "::" + "Data fetching in progress");

            GetEconomicFundamentalMarketData();
            GetMarketDataSmart();

            Library.WriteErrorLog(DateTime.Now + "::" + "[Data fetching Completed]");
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

                        Library.WriteErrorLog(DateTime.Now + "::" + "Updating In Memory DB :: " +
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

            DateTime currentStartDate = DateTime.Now;

            var yearsBack = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["YEARSBACK"].ToString());
            currentStartDate = currentStartDate.AddYears(-1 * yearsBack);

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

                if (item == "NYMEX" || item == "Forex" || item == "FOREX") selector += "_" + item;


                var settings = System.Configuration.ConfigurationManager.ConnectionStrings[selector];

                List<String> timeframeList = System.Configuration.ConfigurationManager.AppSettings["TimeFrameList"].Split(',').ToList();

                foreach (var timeframeItem in timeframeList)
                {
                    try
                    {
                        using (SqlConnection c = new SqlConnection(settings.ToString()))
                        {
                            c.Open();
                            String query = "SELECT * FROM dbo.tblTrades WHERE dbo.tblTrades.StockExchange = '" + item
                                + "' AND dbo.tblTrades.DateTime > '" + dateStr + "' ORDER BY dbo.tblTrades.SymbolID, dbo.tblTrades.DateTime ASC";

                            if (item == "Forex") query = "SELECT * FROM dbo.tblTrades WHERE dbo.tblTrades.TimeFrame = '" + timeframeItem
                                + "' AND dbo.tblTrades.DateTime > '" + dateStr + "' ORDER BY dbo.tblTrades.SymbolID, dbo.tblTrades.DateTime ASC";

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

                                if (item != "Forex" && item != "FOREX")
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

                                    case "FOREX":
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

                                                case "10min":
                                                    {
                                                        tenMinuteTradeSummaries_FOREX_LOOKUP_X.Add(lookUpID, dayTrade);

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

                                                case "4hour":
                                                    {
                                                        fourHourTradeSummaries_FOREX_LOOKUPX.Add(lookUpID, dayTrade);

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

                    case "FOREX":
                    case "Forex":
                        {
                            Console.WriteLine("Exchange 1min Forex:: " + oneMinuteTradeSummaries_FOREX_LOOKUP_X.Count());
                            Console.WriteLine("Exchange 5min Forex:: " + fiveMinuteTradeSummaries_FOREX_LOOKUP_X.Count());
                            Console.WriteLine("Exchange 10min Forex:: " + tenMinuteTradeSummaries_FOREX_LOOKUP_X.Count());
                            Console.WriteLine("Exchange 15min Forex:: " + fifteenMinuteTradeSummaries_FOREX_LOOKUPX.Count());
                            Console.WriteLine("Exchange 30min Forex:: " + thrityMinuteTradeSummaries_FOREX_LOOKUPX.Count());
                            Console.WriteLine("Exchange 1hour Forex:: " + oneHourTradeSummaries_FOREX_LOOKUPX.Count());
                            Console.WriteLine("Exchange 2hour Forex:: " + twoHourTradeSummaries_FOREX_LOOKUPX.Count());
                            Console.WriteLine("Exchange 3hour Forex:: " + threeHourTradeSummaries_FOREX_LOOKUPX.Count());
                            Console.WriteLine("Exchange 4hour Forex:: " + fourHourTradeSummaries_FOREX_LOOKUPX.Count());

                            Console.WriteLine("Exchange EndOfDay Forex:: " + dayTradeSummaries_FOREX_LOOKUP.Count());
                        } break;
                }
            }
            Console.WriteLine("[Data Retrieval Complete]");

        }

        private void GetEconomicFundamentalMarketData()
        {
            String currentTitle = Console.Title;

            // var startDate = System.Configuration.ConfigurationManager.AppSettings["STARTDATE"].ToString();

            DateTime currentStartDate = DateTime.Now;
            currentStartDate = currentStartDate.AddYears(-40);
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

                    case "economic activity":
                        {
                            currentLookUp = economicActivityFundamentals;
                        } break;

                    case "confidence index":
                        {
                            currentLookUp = confidenceIndexFundamentals;
                        } break;

                    case "inflation":
                        {
                            currentLookUp = inflationFundamentals;
                        } break;

                    case "bonds":
                        {
                            currentLookUp = bondsFundamentals;
                        } break;

                    case "balance":
                        {
                            currentLookUp = balanceFundamentals;
                        } break;

                    case "credit":
                        {
                            currentLookUp = creditFundamentals;
                        } break;

                    case "sanction":
                        {
                            currentLookUp = sanctionEvents;
                        } break;
                    case "pandemic":
                        {
                            currentLookUp = pandemicEvents;
                        } break;
                    case "war":
                        {
                            currentLookUp = warEvents;
                        } break;
                }
                PopulateEconomicFundamentalsLookUp(settings.ToString(), categoryItem, dateStr, currentLookUp, lookUpCount);
            }

            PopulateEconomicFundamentalsEssentialsLookUp(settings.ToString());

            //
            Console.WriteLine("Central Bank :: " + centralBankFundamentals.Count());
            Console.WriteLine("Employment :: " + employmentFundamentals.Count());
            Console.WriteLine("Sanction Events :: " + sanctionEvents.Count());
            Console.WriteLine("Pandemic Events :: " + pandemicEvents.Count());
            Console.WriteLine("War Events :: " + warEvents.Count());
            
            Console.WriteLine("[Data Retrieval Complete]");

            Library.WriteErrorLog(DateTime.Now + "::" + "[Data Retrieval Complete]");


        }


        private void GetMarketDataSmart()
        {
            String currentTitle = Console.Title;

            //DateTime currentStartDate = DateTime.Now;
            //var yearsBack = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["YEARSBACK"].ToString());
            //currentStartDate = currentStartDate.AddYears(-1 * yearsBack);
            //var dateStr = currentStartDate.ToString("yyyy-MM-dd");


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

                if (item == "NYMEX" || item == "Forex" || item == "FOREX") selector += "_" + item;

                var settings = System.Configuration.ConfigurationManager.ConnectionStrings[selector];

                var useMulitipleSource = Convert.ToBoolean(System.Configuration.ConfigurationManager.AppSettings["GET_FOREX_FROM_MULTIPLEDB"].ToString());

                List<String> timeframeList = System.Configuration.ConfigurationManager.AppSettings["TimeFrameList"].Split(',').ToList();
                Dictionary<string, int> lookUpCount = new Dictionary<string, int>();

                foreach (var timeframeItem in timeframeList)
                {
                    DateTime currentStartDate = DateTime.Now;

                    var yearsBack = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["YEARSBACK"].ToString());
                    currentStartDate = currentStartDate.AddYears(-1 * yearsBack);
                    var dateStr = currentStartDate.ToString("yyyy-MM-dd");


                    if (timeframeItem == "15min" ||
                        timeframeItem == "10min" ||
                        timeframeItem == "5min")
                    {
                        currentStartDate = currentStartDate.AddYears(-3);
                        dateStr = currentStartDate.ToString("yyyy-MM-dd");
                    }


                    //forex criteria temporary
                    string newSelector = "";
                    if (useMulitipleSource && item == "Forex" && timeframeItem == "1min" || useMulitipleSource && item == "Forex" && timeframeItem == "5min")
                    {
                        newSelector = selector + "_1min5min";
                    }
                    else if (useMulitipleSource && item == "Forex" && timeframeItem == "10min" || useMulitipleSource && item == "Forex" && timeframeItem == "15min")
                    {
                        newSelector = selector + "_10min15min";
                    }
                    else
                    {
                        newSelector = selector;
                    }

                    settings = System.Configuration.ConfigurationManager.ConnectionStrings[newSelector];

                    DataCount(settings.ToString(), timeframeItem, dateStr, stockExchange, lookUpCount);
                }

                foreach (var timeframeItem in timeframeList)
                {
                    DateTime currentStartDate = DateTime.Now;

                    var yearsBack = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["YEARSBACK"].ToString());
                    currentStartDate = currentStartDate.AddYears(-1 * yearsBack);
                    var dateStr = currentStartDate.ToString("yyyy-MM-dd");

                    if (timeframeItem == "15min" ||
                        timeframeItem == "10min" ||
                        timeframeItem == "5min")
                    {
                        currentStartDate = currentStartDate.AddYears(-3);
                        dateStr = currentStartDate.ToString("yyyy-MM-dd");
                    }



                    string newSelector = "";
                    if (useMulitipleSource && item == "Forex" && timeframeItem == "1min" || useMulitipleSource && item == "Forex" && timeframeItem == "5min")
                    {
                        newSelector = selector + "_1min5min";
                    }
                    else if (useMulitipleSource && item == "Forex" && timeframeItem == "10min" || useMulitipleSource && item == "Forex" && timeframeItem == "15min")
                    {
                        newSelector = selector + "_10min15min";
                    }
                    else
                    {
                        newSelector = selector;
                    }
                    settings = System.Configuration.ConfigurationManager.ConnectionStrings[newSelector];


                    Dictionary<string, TradeSummary> currentLookUp = null;
                    switch (stockExchange)
                    {
                        case "EUREX":
                        case "MIXED":
                        case "NASDAQ":
                        case "INDEX":
                            {
                                //indicies
                                switch (timeframeItem)
                                {
                                    case "1min":
                                        {
                                            currentLookUp = oneMinuteTradeSummaries_INDICES_LOOKUP;

                                        } break;

                                    case "5min":
                                        {
                                            currentLookUp = fiveMinuteTradeSummaries_INDICES_LOOKUP;
                                        } break;

                                    case "10min":
                                        {
                                            currentLookUp = tenMinuteTradeSummaries_INDICES_LOOKUP;
                                        } break;

                                    case "15min":
                                        {
                                            currentLookUp = fifteenMinuteTradeSummaries_INDICES_LOOKUP;
                                        } break;

                                    case "30min":
                                        {
                                            currentLookUp = thrityMinuteTradeSummaries_INDICES_LOOKUP;
                                        } break;

                                    case "1hour":
                                        {
                                            currentLookUp = oneHourTradeSummaries_INDICES_LOOKUP;
                                        } break;

                                    case "2hour":
                                        {
                                            currentLookUp = twoHourTradeSummaries_INDICES_LOOKUP;
                                        } break;

                                    case "3hour":
                                        {
                                            currentLookUp = threeHourTradeSummaries_INDICES_LOOKUP;
                                        } break;

                                    case "4hour":
                                        {
                                            currentLookUp = fourHourTradeSummaries_INDICES_LOOKUP;
                                        } break;

                                    case "EndOfDay":
                                        {
                                            currentLookUp = dayTradeSummaries_INDICES_LOOKUP;
                                        } break;
                                }
                            } break;

                        case "NYMEX":
                            {
                                switch (timeframeItem)
                                {
                                    case "1min":
                                        {
                                            currentLookUp = oneMinuteTradeSummaries_COMMODITIES_LOOKUP;

                                        } break;

                                    case "5min":
                                        {
                                            currentLookUp = fiveMinuteTradeSummaries_COMMODITIES_LOOKUP;
                                        } break;

                                    case "10min":
                                        {
                                            currentLookUp = tenMinuteTradeSummaries_COMMODITIES_LOOKUP;
                                        } break;

                                    case "15min":
                                        {
                                            currentLookUp = fifteenMinuteTradeSummaries_COMMODITIES_LOOKUP;
                                        } break;

                                    case "30min":
                                        {
                                            currentLookUp = thrityMinuteTradeSummaries_COMMODITIES_LOOKUP;
                                        } break;

                                    case "1hour":
                                        {
                                            currentLookUp = oneHourTradeSummaries_COMMODITIES_LOOKUP;
                                        } break;

                                    case "2hour":
                                        {
                                            currentLookUp = twoHourTradeSummaries_COMMODITIES_LOOKUP;
                                        } break;

                                    case "3hour":
                                        {
                                            currentLookUp = threeHourTradeSummaries_COMMODITIES_LOOKUP;
                                        } break;

                                    case "4hour":
                                        {
                                            currentLookUp = fourHourTradeSummaries_COMMODITIES_LOOKUP;
                                        } break;

                                    case "EndOfDay":
                                        {
                                            currentLookUp = dayTradeSummaries_COMMODITIES_LOOKUP;
                                        } break;
                                }
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

                                    case "10min":
                                        {
                                            currentLookUp = tenMinuteTradeSummaries_FOREX_LOOKUP_X;

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

                                    case "4hour":
                                        {
                                            currentLookUp = fourHourTradeSummaries_FOREX_LOOKUPX;

                                        } break;

                                    case "EndOfDay":
                                        {
                                            currentLookUp = dayTradeSummaries_FOREX_LOOKUP;

                                        } break;
                                }
                            } break;
                    }
                    PopulateLookUp(settings.ToString(), timeframeItem, dateStr, item, currentLookUp, lookUpCount);
                }

                //


                switch (stockExchange)
                {
                    case "MIXED":
                    case "NASDAQ":
                    case "NYSE":
                    case "INDEX":
                        {
                            Console.WriteLine("Exchange 1min " + stockExchange + " :: " + oneMinuteTradeSummaries_INDICES_LOOKUP.Count());
                            Console.WriteLine("Exchange 5min " + stockExchange + " :: " + fiveMinuteTradeSummaries_INDICES_LOOKUP.Count());
                            Console.WriteLine("Exchange 10min " + stockExchange + " :: " + tenMinuteTradeSummaries_INDICES_LOOKUP.Count());
                            Console.WriteLine("Exchange 15min " + stockExchange + " :: " + fifteenMinuteTradeSummaries_INDICES_LOOKUP.Count());
                            Console.WriteLine("Exchange 30min " + stockExchange + " :: " + thrityMinuteTradeSummaries_INDICES_LOOKUP.Count());
                            Console.WriteLine("Exchange 1hour " + stockExchange + " :: " + oneHourTradeSummaries_INDICES_LOOKUP.Count());
                            Console.WriteLine("Exchange 2hour " + stockExchange + " :: " + twoHourTradeSummaries_INDICES_LOOKUP.Count());
                            Console.WriteLine("Exchange 3hour " + stockExchange + " :: " + threeHourTradeSummaries_INDICES_LOOKUP.Count());
                            Console.WriteLine("Exchange 4hour " + stockExchange + " :: " + fourHourTradeSummaries_INDICES_LOOKUP.Count());
                            Console.WriteLine("Exchange EndOfDay " + stockExchange + " :: " + dayTradeSummaries_INDICES_LOOKUP.Count());


                            Library.WriteErrorLog(DateTime.Now + "Exchange 1min " + stockExchange + " :: " + oneMinuteTradeSummaries_INDICES_LOOKUP.Count());
                            Library.WriteErrorLog(DateTime.Now + "Exchange 5min " + stockExchange + " :: " + fiveMinuteTradeSummaries_INDICES_LOOKUP.Count());
                            Library.WriteErrorLog(DateTime.Now + "Exchange 10min " + stockExchange + " :: " + tenMinuteTradeSummaries_INDICES_LOOKUP.Count());
                            Library.WriteErrorLog(DateTime.Now + "Exchange 15min " + stockExchange + " :: " + fifteenMinuteTradeSummaries_INDICES_LOOKUP.Count());
                            Library.WriteErrorLog(DateTime.Now + "Exchange 30min " + stockExchange + " :: " + thrityMinuteTradeSummaries_INDICES_LOOKUP.Count());
                            Library.WriteErrorLog(DateTime.Now + "Exchange 1hour " + stockExchange + " :: " + oneHourTradeSummaries_INDICES_LOOKUP.Count());
                            Library.WriteErrorLog(DateTime.Now + "Exchange 2hour " + stockExchange + " :: " + twoHourTradeSummaries_INDICES_LOOKUP.Count());
                            Library.WriteErrorLog(DateTime.Now + "Exchange 3hour " + stockExchange + " :: " + threeHourTradeSummaries_INDICES_LOOKUP.Count());
                            Library.WriteErrorLog(DateTime.Now + "Exchange 4hour " + stockExchange + " :: " + fourHourTradeSummaries_INDICES_LOOKUP.Count());
                            Library.WriteErrorLog(DateTime.Now + "Exchange EndOfDay " + stockExchange + " :: " + dayTradeSummaries_INDICES_LOOKUP.Count());

                        } break;

                    case "NYMEX":
                        {
                            Console.WriteLine("Exchange 1min " + stockExchange + " :: " + oneMinuteTradeSummaries_COMMODITIES_LOOKUP.Count());
                            Console.WriteLine("Exchange 5min " + stockExchange + " :: " + fiveMinuteTradeSummaries_COMMODITIES_LOOKUP.Count());
                            Console.WriteLine("Exchange 10min " + stockExchange + " :: " + tenMinuteTradeSummaries_COMMODITIES_LOOKUP.Count());
                            Console.WriteLine("Exchange 15min " + stockExchange + " :: " + fifteenMinuteTradeSummaries_COMMODITIES_LOOKUP.Count());
                            Console.WriteLine("Exchange 30min " + stockExchange + " :: " + thrityMinuteTradeSummaries_COMMODITIES_LOOKUP.Count());
                            Console.WriteLine("Exchange 1hour " + stockExchange + " :: " + oneHourTradeSummaries_COMMODITIES_LOOKUP.Count());
                            Console.WriteLine("Exchange 2hour " + stockExchange + " :: " + twoHourTradeSummaries_COMMODITIES_LOOKUP.Count());
                            Console.WriteLine("Exchange 3hour " + stockExchange + " :: " + threeHourTradeSummaries_COMMODITIES_LOOKUP.Count());
                            Console.WriteLine("Exchange 4hour " + stockExchange + " :: " + fourHourTradeSummaries_COMMODITIES_LOOKUP.Count());
                            Console.WriteLine("Exchange EndOfDay " + stockExchange + " :: " + dayTradeSummaries_COMMODITIES_LOOKUP.Count());



                            Library.WriteErrorLog(DateTime.Now + "Exchange 1min " + stockExchange + " :: " + oneMinuteTradeSummaries_COMMODITIES_LOOKUP.Count());
                            Library.WriteErrorLog(DateTime.Now + "Exchange 5min " + stockExchange + " :: " + fiveMinuteTradeSummaries_COMMODITIES_LOOKUP.Count());
                            Library.WriteErrorLog(DateTime.Now + "Exchange 10min " + stockExchange + " :: " + tenMinuteTradeSummaries_COMMODITIES_LOOKUP.Count());
                            Library.WriteErrorLog(DateTime.Now + "Exchange 15min " + stockExchange + " :: " + fifteenMinuteTradeSummaries_COMMODITIES_LOOKUP.Count());
                            Library.WriteErrorLog(DateTime.Now + "Exchange 30min " + stockExchange + " :: " + thrityMinuteTradeSummaries_COMMODITIES_LOOKUP.Count());
                            Library.WriteErrorLog(DateTime.Now + "Exchange 1hour " + stockExchange + " :: " + oneHourTradeSummaries_COMMODITIES_LOOKUP.Count());
                            Library.WriteErrorLog(DateTime.Now + "Exchange 2hour " + stockExchange + " :: " + twoHourTradeSummaries_COMMODITIES_LOOKUP.Count());
                            Library.WriteErrorLog(DateTime.Now + "Exchange 3hour " + stockExchange + " :: " + threeHourTradeSummaries_COMMODITIES_LOOKUP.Count());
                            Library.WriteErrorLog(DateTime.Now + "Exchange 4hour " + stockExchange + " :: " + fourHourTradeSummaries_COMMODITIES_LOOKUP.Count());
                            Library.WriteErrorLog(DateTime.Now + "Exchange EndOfDay " + stockExchange + " :: " + dayTradeSummaries_COMMODITIES_LOOKUP.Count());


                        } break;

                    case "FOREX":
                    case "Forex":
                        {
                            Console.WriteLine("Exchange 1min Forex:: " + oneMinuteTradeSummaries_FOREX_LOOKUP_X.Count());
                            Console.WriteLine("Exchange 5min Forex:: " + fiveMinuteTradeSummaries_FOREX_LOOKUP_X.Count());
                            Console.WriteLine("Exchange 10min Forex:: " + tenMinuteTradeSummaries_FOREX_LOOKUP_X.Count());
                            Console.WriteLine("Exchange 15min Forex:: " + fifteenMinuteTradeSummaries_FOREX_LOOKUPX.Count());
                            Console.WriteLine("Exchange 30min Forex:: " + thrityMinuteTradeSummaries_FOREX_LOOKUPX.Count());
                            Console.WriteLine("Exchange 1hour Forex:: " + oneHourTradeSummaries_FOREX_LOOKUPX.Count());
                            Console.WriteLine("Exchange 2hour Forex:: " + twoHourTradeSummaries_FOREX_LOOKUPX.Count());
                            Console.WriteLine("Exchange 3hour Forex:: " + threeHourTradeSummaries_FOREX_LOOKUPX.Count());
                            Console.WriteLine("Exchange 4hour Forex:: " + fourHourTradeSummaries_FOREX_LOOKUPX.Count());
                            Console.WriteLine("Exchange EndOfDay Forex:: " + dayTradeSummaries_FOREX_LOOKUP.Count());



                            Library.WriteErrorLog(DateTime.Now + "Exchange 1min Forex:: " + oneMinuteTradeSummaries_FOREX_LOOKUP_X.Count());
                            Library.WriteErrorLog(DateTime.Now + "Exchange 5min Forex:: " + fiveMinuteTradeSummaries_FOREX_LOOKUP_X.Count());
                            Library.WriteErrorLog(DateTime.Now + "Exchange 10min Forex:: " + tenMinuteTradeSummaries_FOREX_LOOKUP_X.Count());
                            Library.WriteErrorLog(DateTime.Now + "Exchange 15min Forex:: " + fifteenMinuteTradeSummaries_FOREX_LOOKUPX.Count());
                            Library.WriteErrorLog(DateTime.Now + "Exchange 30min Forex:: " + thrityMinuteTradeSummaries_FOREX_LOOKUPX.Count());
                            Library.WriteErrorLog(DateTime.Now + "Exchange 1hour Forex:: " + oneHourTradeSummaries_FOREX_LOOKUPX.Count());
                            Library.WriteErrorLog(DateTime.Now + "Exchange 2hour Forex:: " + twoHourTradeSummaries_FOREX_LOOKUPX.Count());
                            Library.WriteErrorLog(DateTime.Now + "Exchange 3hour Forex:: " + threeHourTradeSummaries_FOREX_LOOKUPX.Count());
                            Library.WriteErrorLog(DateTime.Now + "Exchange 4hour Forex:: " + fourHourTradeSummaries_FOREX_LOOKUPX.Count());
                            Library.WriteErrorLog(DateTime.Now + "Exchange EndOfDay Forex:: " + dayTradeSummaries_FOREX_LOOKUP.Count());

                        } break;
                }
            }
            Console.WriteLine("[Data Retrieval Complete]");
            Library.WriteErrorLog(DateTime.Now + "::" + "[Data Retrieval Complete]");

        }

        public void DataCount(string conStr, string timeFrame, string startDate, string exchange, Dictionary<string, int> lookUpCount)
        {
            bool continueFetchingData = true;
            Console.WriteLine("Fetching :: " + exchange + " Data.." + DateTime.Now.ToString());

            while (continueFetchingData)
            {
                try
                {
                    using (SqlConnection c = new SqlConnection(conStr.ToString()))
                    {
                        c.Open();
                        String query = "SELECT COUNT(*) AS DataCount FROM dbo.tblTrades WHERE dbo.tblTrades.StockExchange = '" + exchange
                            + "' AND dbo.tblTrades.TimeFrame = '" + timeFrame + "'AND dbo.tblTrades.DateTime > '" + startDate + "'";

                        if (exchange == "Forex") query = "SELECT COUNT(*) AS DataCount FROM dbo.tblTrades WHERE dbo.tblTrades.TimeFrame = '" + timeFrame
                            + "' AND dbo.tblTrades.DateTime > '" + startDate + "'";

                        SqlCommand sqlCommand = new SqlCommand(query, c);
                        sqlCommand.CommandTimeout = 0;

                        SqlDataReader reader = sqlCommand.ExecuteReader();
                        if (reader.HasRows) continueFetchingData = false;
                        // Console.WriteLine("Fetching :: " + exchange + " Data.." + DateTime.Now.ToString());

                        List<TradeSummary> dayTradeList = new List<TradeSummary>(); ;

                        while (reader.Read())
                        {
                            int dataCount = Convert.ToInt32(reader["DataCount"].ToString());
                            lookUpCount.Add(timeFrame, dataCount);

                            Console.WriteLine("Expected Count :: " + exchange + " " + timeFrame + " Data.." + dataCount);

                            Library.WriteErrorLog(DateTime.Now + "::" + "Expected Count :: " + exchange + " " + timeFrame + " Data.." + dataCount);
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.log.Error("Database Error - " + e.Message);
                    Library.WriteErrorLog(DateTime.Now + "::" + "Database Error - " + e.Message);

                }
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
                        lookUpCount.Add(category, dataCount);

                        Console.WriteLine("Expected Count :: " + category + " Data.." + dataCount);
                        Library.WriteErrorLog(DateTime.Now + "::" + "Expected Count :: " + category + " Data.." + dataCount);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.log.Error("Database Error - " + e.Message);
                Library.WriteErrorLog(DateTime.Now + "::" + "Database Error - " + e.Message);
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

            var billionChecker = value.Split('b');
            if (billionChecker.Count() > 1)
            {
                if (String.IsNullOrEmpty(billionChecker[1]))
                {
                    if (Double.TryParse(billionChecker.FirstOrDefault(), out numericDoubleParameter))
                    {
                        return numericDoubleParameter;
                    }
                }
            }


            if (Double.TryParse(value, out numericDoubleParameter))
            {
                return numericDoubleParameter;
            }
            return numericDoubleParameter;
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
                                HeadLine = reader["EventHeadline"].ToString(),
                                Category = reader["Category"].ToString(),
                                Country = reader["Country"].ToString(),
                                Event = reader["EventType"].ToString(),
                                Currency = reader["Currency"].ToString(),
                                InstitutionBody = reader["InstitutionBody"].ToString(),
                                EventDescription = reader["EventDescription"].ToString()                               
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
                Library.WriteErrorLog(DateTime.Now + "::" + "Database Error - " + e.Message);
            }
        }

        private List<string> GetHeadLinesOnly(string conStr, string category)
        {
            var collection = new List<string>();

            var startDateTime = DateTime.Now;
            Console.WriteLine("Loading Start Time :: " + startDateTime.ToString());

            using (SqlConnection c = new SqlConnection(conStr.ToString()))
            {
                c.Open();
                String query = "proc_GetHeadLinesOnly";

                SqlCommand sqlCommand = new SqlCommand(query, c);
                sqlCommand.Parameters.AddWithValue("@Category", category);

                sqlCommand.CommandType = CommandType.StoredProcedure;
                sqlCommand.CommandTimeout = 0;

                SqlDataReader reader = sqlCommand.ExecuteReader();
                while (reader.Read())
                {
                    collection.Add(reader["EventHeadline"].ToString());
                }
            }
            return collection;
        }

        public void PopulateEconomicFundamentalsLookUp(string conStr, string category, string startDate, Dictionary<string, EconomicFundamentals> currentLookUp, Dictionary<string, int> currentLookUpCount)
        {
            bool continueFetchingData = true;
            Console.WriteLine("Fetching :: Economic Fundamental Data.." + DateTime.Now.ToString());
            Library.WriteErrorLog(DateTime.Now + "::" + "Fetching :: Economic Fundamental Data..");

            while (continueFetchingData)
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

                                var tempFundamentals = new EconomicFundamentals();

                                //String lookUpID = economicFundamentals.EventType + "#" + economicFundamentals.Country
                                //    + "#" + economicFundamentals.ReleaseDateTime.Ticks.ToString();

                                String lookUpID = economicFundamentals.EventType + "#" + economicFundamentals.Country + "#"
                                    + economicFundamentals.EventHeadline
                                    + "#" + economicFundamentals.ReleaseDateTime.Ticks.ToString();

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
                    Console.WriteLine("Actual Count :: " + category + " : " + currentLookUp.Count);
                    Logger.log.Info("Actual Count :: " + category + " : " + currentLookUp.Count);

                    int dataCounts = 0;
                    if (currentLookUpCount.TryGetValue(category, out dataCounts))
                    {
                        if (currentLookUp.Count == dataCounts)
                        {
                            continueFetchingData = false;
                            Console.WriteLine("Data Completed :: " + category);
                            Console.WriteLine(" ");
                            Logger.log.Info("Data Completed :: " + category + "Row Count :: " + dataCounts);
                            Library.WriteErrorLog(DateTime.Now + "::" + "Data Completed :: " + category);
                        }
                        else
                        {
                            Console.WriteLine("Actual Count :: " + currentLookUp.Count + " Expected Count :: " + dataCounts);
                            Logger.log.Info("Actual Count :: " + currentLookUp.Count + " Expected Count :: " + dataCounts);
                            Library.WriteErrorLog(DateTime.Now + "::" + currentLookUp.Count + " Expected Count :: " + dataCounts);
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


        public void PopulateEconomicFundamentalsLookUpT(string conStr, string category, string startDate, Dictionary<string, EconomicFundamentals> currentLookUp, Dictionary<string, int> currentLookUpCount)
        {
            var headlines = GetHeadLinesOnly(conStr, category);

            bool continueFetchingData = true;
            Console.WriteLine("Fetching :: Economic Fundamental Data.." + DateTime.Now.ToString());
            Library.WriteErrorLog(DateTime.Now + "::" + "Fetching :: Economic Fundamental Data..");

            while (continueFetchingData)
            {
                foreach (var item in headlines)
                {
                    try
                    {
                        var startDateTime = DateTime.Now;
                        Console.WriteLine("Loading Start Time :: " + startDateTime.ToString());

                        using (SqlConnection c = new SqlConnection(conStr.ToString()))
                        {
                            c.Open();
                            String query = "proc_GetEconomicFundamentalsByEventHeadLine";

                            SqlCommand sqlCommand = new SqlCommand(query, c);
                            sqlCommand.Parameters.AddWithValue("@EventHeadline", item);

                            sqlCommand.CommandType = CommandType.StoredProcedure;
                            sqlCommand.CommandTimeout = 0;

                            SqlDataReader reader = sqlCommand.ExecuteReader();

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

                                    var tempFundamentals = new EconomicFundamentals();

                                    String lookUpID = economicFundamentals.EventType + "#" + economicFundamentals.Country + "#"
                                        + economicFundamentals.EventHeadline
                                        + "#" + economicFundamentals.ReleaseDateTime.Ticks.ToString();

                                    if (currentLookUp.TryGetValue(lookUpID, out tempFundamentals) == false)
                                    {
                                        currentLookUp.Add(lookUpID, economicFundamentals);
                                    }
                                    else
                                    {
                                        int gkgk = 2;
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
                }

                if (currentLookUp != null)
                {
                    Console.WriteLine("Actual Count :: " + category + " : " + currentLookUp.Count);
                    Logger.log.Info("Actual Count :: " + category + " : " + currentLookUp.Count);

                    int dataCounts = 0;
                    if (currentLookUpCount.TryGetValue(category, out dataCounts))
                    {
                        if (currentLookUp.Count == dataCounts)
                        {
                            continueFetchingData = false;
                            Console.WriteLine("Data Completed :: " + category);
                            Console.WriteLine(" ");
                            Logger.log.Info("Data Completed :: " + category + "Row Count :: " + dataCounts);
                            Library.WriteErrorLog(DateTime.Now + "::" + "Data Completed :: " + category);
                        }
                        else
                        {
                            Console.WriteLine("Actual Count :: " + currentLookUp.Count + " Expected Count :: " + dataCounts);
                            Logger.log.Info("Actual Count :: " + currentLookUp.Count + " Expected Count :: " + dataCounts);
                            Library.WriteErrorLog(DateTime.Now + "::" + currentLookUp.Count + " Expected Count :: " + dataCounts);
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

        public void PopulateLookUp(string conStr, string timeFrame, string startDate, string exchange, Dictionary<string, TradeSummary> currentLookUp, Dictionary<string, int> currentLookUpCount)
        {
            bool continueFetchingData = true;
            Console.WriteLine("Fetching :: " + exchange + " Data.." + DateTime.Now.ToString());

            while (continueFetchingData)
            {
                string logKeep = "";

                try
                {
                    var startDateTime = DateTime.Now;
                    Console.WriteLine("Loading Start Time :: " + startDateTime.ToString());
                    Library.WriteErrorLog(DateTime.Now + " :: " + "Loading Start Time :: " + startDateTime.ToString());

                    using (SqlConnection c = new SqlConnection(conStr.ToString()))
                    {
                        c.Open();
                        String query = "SELECT * FROM dbo.tblTrades WHERE dbo.tblTrades.StockExchange = '" + exchange
                            + "' AND dbo.tblTrades.TimeFrame = '" + timeFrame + "'AND dbo.tblTrades.DateTime > '" + startDate
                            + "' ORDER BY dbo.tblTrades.SymbolID, dbo.tblTrades.DateTime ASC";

                        if (exchange == "Forex") query = "SELECT * FROM dbo.tblTrades WHERE dbo.tblTrades.TimeFrame = '" + timeFrame
                            + "' AND dbo.tblTrades.DateTime > '" + startDate + "' ORDER BY dbo.tblTrades.SymbolID, dbo.tblTrades.DateTime ASC";

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
                            string sym = "";
                            if (nameLookUp.TryGetValue(symbolID, out sym))
                            {
                                dayTrade.SymbolID = sym;
                                symbolID = sym;
                            }
                            else
                            {
                                dayTrade.SymbolID = symbolID;
                            }

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
                            logKeep = symbolID + "#" + dayTrade.DateTime.ToString();

                            currentLookUp.Add(lookUpID, dayTrade);

                        }
                    }

                    TimeSpan timeSpan = DateTime.Now - startDateTime;

                    Console.WriteLine(timeFrame + " Loading End Time :: " + DateTime.Now.ToString() + " Total Time : " + timeSpan.ToString());

                    Library.WriteErrorLog(DateTime.Now + " :: " + timeFrame + " Loading End Time :: " + DateTime.Now.ToString() + " Total Time : " + timeSpan.ToString());


                }
                catch (Exception e)
                {
                    Logger.log.Error("Database Error - " + e.Message);
                    Library.WriteErrorLog(DateTime.Now + " :: Database Error - " + e.Message + " :: " + logKeep);

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
                            Library.WriteErrorLog(DateTime.Now + " :: " + "Data Completed :: " + timeFrame);

                            Console.WriteLine(" ");
                            Logger.log.Info("Data Completed :: " + timeFrame + "Row Count :: " + dataCounts);

                            Library.WriteErrorLog(DateTime.Now + " :: " + "Data Completed :: " + timeFrame + "Row Count :: " + dataCounts);

                        }
                        else
                        {
                            Console.WriteLine("Actual Count :: " + currentLookUp.Count + " Expected Count :: " + dataCounts);
                            Logger.log.Info("Actual Count :: " + currentLookUp.Count + " Expected Count :: " + dataCounts);

                            Library.WriteErrorLog(DateTime.Now + " :: " + "Actual Count :: " + currentLookUp.Count + " Expected Count :: " + dataCounts);


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


        public string GetSymbolUsageName(string symbol)
        {
            var conStr = System.Configuration.ConfigurationManager.ConnectionStrings["JobsManager_TEST"].ToString();

            string symbolCurrent = symbol;
            try
            {
                using (SqlConnection c = new SqlConnection(conStr.ToString()))
                {
                    c.Open();
                    String query = "proc_GetSymbolNameConversionLookUp";

                    SqlCommand sqlCommand = new SqlCommand(query, c);
                    sqlCommand.Parameters.AddWithValue("@keywordvar", symbol);

                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    sqlCommand.CommandTimeout = 0;

                    SqlDataReader reader = sqlCommand.ExecuteReader();

                    while (reader.Read())
                    {
                        symbolCurrent = reader["SymbolIDFriendly"].ToString();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.log.Error("Database Error - " + e.Message);
            }
            return symbolCurrent;
        }

        public void LoadSymbolUsageName()
        {
            var conStr = System.Configuration.ConfigurationManager.ConnectionStrings["JobsManager_TEST"].ToString();
            try
            {
                using (SqlConnection c = new SqlConnection(conStr.ToString()))
                {
                    c.Open();
                    String query = "proc_GetAllSymbolNameConversionLookUp";

                    SqlCommand sqlCommand = new SqlCommand(query, c);

                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    sqlCommand.CommandTimeout = 0;

                    SqlDataReader reader = sqlCommand.ExecuteReader();

                    while (reader.Read())
                    {
                        nameLookUp.Add(reader["SymbolID"].ToString(), reader["SymbolIDFriendly"].ToString());
                    }
                }
            }
            catch (Exception e)
            {
                Logger.log.Error("Database Error - " + e.Message);
            }
        }

        public List<TradeSummary> GetTradeSummaries(List<String> symbolList, DateTime startDate, DateTime endDate, String exchange, String timeFrame)
        {
            Library.WriteErrorLog(DateTime.Now + "::" + symbolList.FirstOrDefault() + " " + startDate.ToString() + " to " + endDate.ToString() + " " + timeFrame);

            List<TradeSummary> dtList = new List<TradeSummary>();
            long dayTicks = 864000000000;

            for (int i = 0; i < symbolList.Count; i++)
            {
                symbolList[i] = symbolList[i].ToUpper();
            }

            exchange = GetExchangeLookUp(symbolList.FirstOrDefault());

            switch (exchange)
            {
                case "INDEX":
                case "NASDAQ":
                case "NYSE":
                case "NYMEX":
                case "FOREX":
                case "Forex":
                    {
                        RetrievalCheck(symbolList, startDate, endDate, exchange, timeFrame, dtList, false);

                    } break;

            }
            return dtList;
        }

        public bool RetrievalCheckEconomicFundamentals(List<string> eventTypeList, DateTime startDate, DateTime endDate, string category, string country, List<EconomicFundamentals> economicFundamentalListing, bool check, string headline)
        {
            //long selectedTicks = 864000000000;
            long selectedTicks = TimeSpan.TicksPerMinute;

            Dictionary<string, EconomicFundamentals> lookUpEconomics = SelectEconomicFundamentals(category);

            if (lookUpEconomics != null && string.IsNullOrEmpty(category) == false)
            {
                String eventItem = eventTypeList.FirstOrDefault();

                List<String> lookUpIDList = new List<String>();

                var temp = eventItem + "#" + country + "#" + headline + "#";

                long tempCurrent = startDate.Ticks;
                while (tempCurrent <= endDate.Ticks)
                {
                    String lookUpIDTemp = temp + tempCurrent.ToString();
                    lookUpIDList.Add(lookUpIDTemp);

                    tempCurrent = tempCurrent + selectedTicks;
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

                //if (lookUpEconomics.Where(m => m.Value.EventHeadline == "U.S. Nonfarm Payrolls").Any())
                //{
                //    var collect = lookUpEconomics.Where(m => m.Value.EventHeadline == "U.S. Nonfarm Payrolls").ToList().Select(n => n.Value);
                //    int o = 0;
                //}
            }
            else
            {
                for (int i = 0; i < 11; i++)
                {
                    switch (i)
                    {
                        case 0:
                            {
                                GetEconomicDataItems(eventTypeList, startDate, endDate, category, country, economicFundamentalListing, centralBankFundamentals, check);
                            } break;
                        case 1:
                            {
                                GetEconomicDataItems(eventTypeList, startDate, endDate, category, country, economicFundamentalListing, employmentFundamentals, check);
                            } break;
                        case 2:
                            {
                                GetEconomicDataItems(eventTypeList, startDate, endDate, category, country, economicFundamentalListing, balanceFundamentals, check);
                            } break;
                        case 3:
                            {
                                GetEconomicDataItems(eventTypeList, startDate, endDate, category, country, economicFundamentalListing, economicActivityFundamentals, check);
                            } break;
                        case 4:
                            {
                                GetEconomicDataItems(eventTypeList, startDate, endDate, category, country, economicFundamentalListing, confidenceIndexFundamentals, check);
                            } break;
                        case 5:
                            {
                                GetEconomicDataItems(eventTypeList, startDate, endDate, category, country, economicFundamentalListing, inflationFundamentals, check);
                            } break;
                        case 6:
                            {
                                GetEconomicDataItems(eventTypeList, startDate, endDate, category, country, economicFundamentalListing, bondsFundamentals, check);
                            } break;
                        case 7:
                            {
                                GetEconomicDataItems(eventTypeList, startDate, endDate, category, country, economicFundamentalListing, creditFundamentals, check);
                            } break;
                        case 8:
                            {
                                GetEconomicDataItems(eventTypeList, startDate, endDate, category, country, economicFundamentalListing, sanctionEvents, check);
                            } break;
                        case 9:
                            {
                                GetEconomicDataItems(eventTypeList, startDate, endDate, category, country, economicFundamentalListing, pandemicEvents, check);
                            } break;

                        case 10:
                            {
                                GetEconomicDataItems(eventTypeList, startDate, endDate, category, country, economicFundamentalListing, warEvents, check);
                            } break;
                    }
                }

                //economicFundamentalListing.AddRange(centralBankFundamentals.Values.ToList().Where(h => h.Country == country));
                //economicFundamentalListing.AddRange(employmentFundamentals.Values.ToList().Where(h => h.Country == country));
                //economicFundamentalListing.AddRange(economicActivityFundamentals.Values.ToList().Where(h => h.Country == country));
                //economicFundamentalListing.AddRange(confidenceIndexFundamentals.Values.ToList().Where(h => h.Country == country));
                //economicFundamentalListing.AddRange(inflationFundamentals.Values.ToList().Where(h => h.Country == country));
                //economicFundamentalListing.AddRange(bondsFundamentals.Values.ToList().Where(h => h.Country == country));
                //economicFundamentalListing.AddRange(balanceFundamentals.Values.ToList().Where(h => h.Country == country));
                //economicFundamentalListing.AddRange(creditFundamentals.Values.ToList().Where(h => h.Country == country));
            }
            return false;
        }

        private bool GetEconomicDataItems(List<string> eventTypeList, DateTime startDate, DateTime endDate, string category, string country, List<EconomicFundamentals> economicFundamentalListing, Dictionary<string, EconomicFundamentals> lookUpEconomics, bool check)
        {
            economicFundamentalListing.AddRange(lookUpEconomics.Where(g => g.Value.Country == country && g.Value.ReleaseDateTime >= startDate
                && g.Value.ReleaseDateTime <= endDate).Select(v => v.Value));
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
        private EconomicFundamentalsEssentials EconomicFundamentalsParametersLookup(List<string> symbolList, string eventType, string category, string country, List<string> economicItemHelpers)
        {
            var econEssen = new EconomicFundamentalsEssentials();

            //eventype, symbol
            if (String.IsNullOrEmpty(country) && String.IsNullOrEmpty(category) && economicFundamentalsEssentials.Any(s => s.Event == eventType))
            {
                var currency = GetCurrency(symbolList.FirstOrDefault());

                foreach (var item in currency)
                {
                    var found = economicFundamentalsEssentials.Where(m => m.Event == eventType
                        && m.Currency == item);

                    if (found.Any())
                    {
                        return found.FirstOrDefault();
                    }
                }
            }
            else if (String.IsNullOrEmpty(eventType) == false && String.IsNullOrEmpty(country) == false && economicItemHelpers.Any())
            {
                //economic helper is a form of category checking
                //assumes country is provided
                var currency = GetCurrency(symbolList.FirstOrDefault());

                foreach (var item in economicItemHelpers)
                {
                    var found = economicFundamentalsEssentials.Where(m => m.Event.IndexOf(eventType) > -1);
                    if (found.Any())
                    {
                        var foundRes = found.Where(m => m.EventDescription.ToLower().IndexOf(item.ToLower()) > -1 && m.Country.ToLower() == country);
                        if (foundRes.Any())
                        {
                            return foundRes.FirstOrDefault();
                        }

                        var foundResTemp = found.Where(m => m.InstitutionBody.ToLower().IndexOf(item.ToLower()) > -1 && m.Country.ToLower() == country);
                        if (foundResTemp.Any())
                        {
                            return foundResTemp.FirstOrDefault();
                        }
                    }
                }
                var foundResA = economicFundamentalsEssentials.Where(m => m.Event == eventType && m.Country.ToLower() == country);
                if (foundResA.Any())
                {
                    return foundResA.FirstOrDefault();
                }
            }
            else if (string.IsNullOrEmpty(country) == false && string.IsNullOrEmpty(eventType) == false && string.IsNullOrEmpty(category) == false)
            {
                var found = economicFundamentalsEssentials.Where(m => m.Event.ToLower() == eventType
                    && m.Country.ToLower() == country && m.Category.ToLower() == category);

                if (found.Any())
                {
                    return found.FirstOrDefault();
                }
            }
            else if (String.IsNullOrEmpty(country) && economicItemHelpers.Any() && string.IsNullOrEmpty(eventType))
            {
                //we want a scenario were the category/economicitemhelper drives the results irrespective of country
                var currency = GetCurrency(symbolList.FirstOrDefault());

                foreach (var item in economicItemHelpers)
                {
                    foreach (var economicEssential in economicFundamentalsEssentials)
                    {
                        var index = economicEssential.EventDescription.ToLower().IndexOf(item.ToLower());
                        if (index > 0 && currency.Contains(economicEssential.Currency))
                        {
                            return economicEssential;
                        }

                        var indexTemp = economicEssential.InstitutionBody.ToLower().IndexOf(item.ToLower());
                        if (indexTemp > 0 && currency.Contains(economicEssential.Currency))
                        {
                            return economicEssential;
                        }
                    }
                }
            }
            else if (string.IsNullOrEmpty(country) == false && string.IsNullOrEmpty(eventType) == false)
            {
                var found = economicFundamentalsEssentials.Where(m => m.Event.ToLower() == eventType
                    && m.Country.ToLower() == country);

                if (found.Any())
                {
                    return found.FirstOrDefault();
                }
            }
            else if (string.IsNullOrEmpty(country) == false && string.IsNullOrEmpty(symbolList.FirstOrDefault()) == false)
            {
                var found = economicFundamentalsEssentials.Where(m => m.Country.ToLower() == country);

                if (found.Any())
                {
                    return found.FirstOrDefault();
                }
            }
            else if (economicFundamentalsEssentials.Any(s => s.Event == eventType))
            {
                var currency = GetCurrency(symbolList.FirstOrDefault());

                foreach (var item in currency)
                {
                    var found = economicFundamentalsEssentials.Where(m => m.Event == eventType
                        && m.Currency == item);

                    if (found.Any())
                    {
                        return found.FirstOrDefault();
                    }
                }
            }
            else if (economicItemHelpers.Any())
            {
                var currency = GetCurrency(symbolList.FirstOrDefault());

                foreach (var item in economicItemHelpers)
                {
                    foreach (var economicEssential in economicFundamentalsEssentials)
                    {
                        var index = economicEssential.EventDescription.ToLower().IndexOf(item.ToLower());

                        var eventIndex = economicEssential.Event.ToLower().IndexOf(eventType.ToLower());

                        if (index > 0 && eventIndex > 0)
                        {
                            return economicEssential;
                        }
                    }
                }
            }
            else
            {
                var currency = GetCurrency(symbolList.FirstOrDefault());

                foreach (var item in currency)
                {
                    var found = economicFundamentalsEssentials.Where(m => m.Currency == item);

                    if (found.Any())
                    {
                        return found.FirstOrDefault();
                    }
                }
            }
            return econEssen;

            //return econEssen;


            //eventype, country

            //category, country, symbolist
        }

        public List<EconomicFundamentals> GetEconomicFundamentals(List<string> symbolList, List<string> eventTypeList, DateTime startDate, DateTime endDate, string category, string country, List<string> economicItemHelpers)
        {
            Library.WriteErrorLog(DateTime.Now + "::" + symbolList.FirstOrDefault() + " " + startDate.ToString() + " to " + endDate.ToString() + " " + country);


            var dataList = new List<EconomicFundamentals>();
            var finalDataList = new List<EconomicFundamentals>();

            string eventItem = eventTypeList.FirstOrDefault();
            if (eventItem.ToUpper() == "COUNTRYECONOMICS")
            {
                eventTypeList = new List<string> { "vote on interest rate", "unemployment rate", "gross domestic product qoq", "continuing jobless claims" };
            }

            foreach (var eventCurrent in eventTypeList)
            {
                var econParameters = EconomicFundamentalsParametersLookup(symbolList, eventCurrent, category, country, economicItemHelpers);

                string useableEvents = string.IsNullOrEmpty(eventCurrent) == false ? econParameters.Event : "";
                var useableCategory = string.IsNullOrEmpty(eventCurrent) && string.IsNullOrEmpty(category) == false ? category : econParameters.Category;

                if (string.IsNullOrEmpty(eventCurrent) && string.IsNullOrEmpty(category)) { useableCategory = ""; }

                RetrievalCheckEconomicFundamentals(new List<string>() { useableEvents }, startDate, endDate, useableCategory, econParameters.Country, dataList, false, econParameters.HeadLine);

                AutoMapper.Mapper.CreateMap<EconomicFundamentals, EconomicFundamentals>();
                foreach (var symbolIDItem in symbolList)
                {
                    for (int i = 0; i < dataList.Count; i++)
                    {
                        var econoData = AutoMapper.Mapper.Map<EconomicFundamentals>(dataList[i]);
                        econoData.AssociatedSymbolID = symbolIDItem;
                        finalDataList.Add(econoData);
                    }
                }

                if (symbolList.Any() == false)
                {
                    for (int i = 0; i < dataList.Count; i++)
                    {
                        var econoData = AutoMapper.Mapper.Map<EconomicFundamentals>(dataList[i]);
                        econoData.AssociatedSymbolID = "None";
                        finalDataList.Add(econoData);
                    }
                }
            }
            //assumption made here
            //dataList.ForEach(n => n.AssociatedSymbolID = symbolList.FirstOrDefault());

            return finalDataList;
        }

        private bool RetrievalCheck(List<String> symbolList, DateTime startDate, DateTime endDate, String exchange, String timeFrame, List<TradeSummary> dtList, bool bCheck)
        {
            //var itemSym = symbolList.FirstOrDefault();

            String symbolData = symbolList.FirstOrDefault();
            long selectedTicks = 864000000000;
            var lookUpTradeSummaries = SelectTradeSummary(symbolList.FirstOrDefault(), timeFrame, out selectedTicks);

            if (symbolData.ToUpper() == "ALLCURRENCYPAIRS")
            {
                symbolList = (lookUpTradeSummaries.Select(m => m.Value.SymbolID)).Distinct().ToList();
            }


            foreach (var itemSym in symbolList)
            {
                selectedTicks = 864000000000;

                lookUpTradeSummaries = SelectTradeSummary(itemSym, timeFrame, out selectedTicks);

                var yearsBack = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["YEARSBACK"].ToString());
                var currentStartDate = DateTime.Now.AddYears(-1 * yearsBack);

                if (timeFrame == "15min" ||
                    timeFrame == "10min" ||
                    timeFrame == "5min")
                {
                    currentStartDate = currentStartDate.AddYears(-3);
                }




                //if the startdate is earlier than what is available in memory then
                //go to the database to fetc the data
                if (startDate < currentStartDate)
                {
                    CheckMainDataStore(symbolList, startDate, GetExchangeLookUp(itemSym), timeFrame, lookUpTradeSummaries);
                }

                if (lookUpTradeSummaries != null)
                {

                    List<String> lookUpIDList = new List<String>();

                    //for (int f = 0; f < symbolList.Count; f++)
                    {
                        //var temp = symbolList[f] + "#";
                        var temp = itemSym + "#";

                        long tempCurrent = startDate.Ticks;
                        while (tempCurrent <= endDate.Ticks)
                        {
                            String lookUpIDTemp = temp + tempCurrent.ToString();
                            lookUpIDList.Add(lookUpIDTemp);

                            tempCurrent = tempCurrent + selectedTicks;
                        }
                    }

                    var currentBag = new System.Collections.Concurrent.ConcurrentBag<TradeSummary>();
                    System.Threading.Tasks.Parallel.ForEach(lookUpIDList, (current) =>
                    {
                        TradeSummary resultDayTradeSum = new TradeSummary();

                        bool bFound = lookUpTradeSummaries.TryGetValue(current, out resultDayTradeSum);
                        if (bFound)
                        {
                            if (bCheck == false)
                            {
                                currentBag.Add(resultDayTradeSum);
                            }
                        }
                    });
                    if (currentBag.Any())
                    {
                        //dtList = currentBag.OrderBy(m => m.DateTime).ToList();

                        foreach (var item in currentBag)
                        {
                            dtList.Add(item);
                        }
                    }
                }
            }
            return true;
        }


        private bool RetrievalCheckOld(List<String> symbolList, DateTime startDate, DateTime endDate, String exchange, String timeFrame, List<TradeSummary> dtList, bool bCheck)
        {
            long selectedTicks = 864000000000;

            Dictionary<String, TradeSummary> lookUpTradeSummaries = SelectTradeSummary(symbolList.FirstOrDefault(), timeFrame, out selectedTicks);

            var yearsBack = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["YEARSBACK"].ToString());
            var currentStartDate = DateTime.Now.AddYears(-1 * yearsBack);


            if (timeFrame == "15min" || timeFrame == "10min" || timeFrame == "5min")
            {
                currentStartDate = DateTime.Now.AddYears(-3);
            }

            //if the startdate is earlier than what is available in memory then
            //go to the database to fetc the data
            if (startDate < currentStartDate)
            {
                CheckMainDataStore(symbolList, startDate, exchange, timeFrame, lookUpTradeSummaries);
            }




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

            selector += "_" + stockExchange;

            var settings = System.Configuration.ConfigurationManager.ConnectionStrings[selector];

            foreach (var item in criteria)
            {
                try
                {
                    using (SqlConnection c = new SqlConnection(settings.ToString()))
                    {
                        c.Open();

                        String query = "SELECT * FROM dbo.tblTrades WHERE dbo.tblTrades.StockExchange = '" + stockExchange + "' AND dbo.tblTrades.DateTime > '"
                            + item.StartDateTimeStr + "' ORDER BY dbo.tblTrades.SymbolID, dbo.tblTrades.DateTime ASC";

                        if (stockExchange == "Forex") query = "SELECT * FROM dbo.tblTrades WHERE dbo.tblTrades.TimeFrame = '" + item.TimeFrame
                            + "' AND dbo.tblTrades.SymbolID = '" + item.SymbolID + "' AND dbo.tblTrades.DateTime > '" + item.StartDateTimeStr + "' ORDER BY dbo.tblTrades.SymbolID, dbo.tblTrades.DateTime ASC";

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



                            TradeSummary tradeSum = null;
                            long ticks = 0;
                            var tradeCollection = SelectTradeSummary(symbolID, item.TimeFrame, out ticks);

                            if (tradeCollection.TryGetValue(lookUpID, out tradeSum) == false)
                            {
                                tradeCollection.Add(lookUpID, dayTrade);
                            }




                            //switch (stockExchange)
                            //{
                            //    case "LSE":
                            //        {
                            //            dayTradeSummaries_LSE_LOOKUP.Add(lookUpID, dayTrade);
                            //        } break;

                            //    case "NASDAQ":
                            //        {
                            //            dayTradeSummaries_NASDAQ_LOOKUP.Add(lookUpID, dayTrade);
                            //        } break;

                            //    case "NYSE":
                            //        {
                            //            dayTradeSummaries_NYSE_LOOKUP.Add(lookUpID, dayTrade);
                            //        } break;

                            //    case "AMEX":
                            //        {
                            //            dayTradeSummaries_AMEX_LOOKUP.Add(lookUpID, dayTrade);
                            //        } break;

                            //    case "Forex":
                            //        {
                            //            TradeSummary tradeSum = null;
                            //            long ticks = 0;

                            //            var tradeCollection = SelectTradeSummary(item.TimeFrame, out ticks);

                            //            if (tradeCollection.TryGetValue(lookUpID, out tradeSum) == false)
                            //            {
                            //                tradeCollection.Add(lookUpID, dayTrade);
                            //            }                                  

                            //        } break;
                            //}
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

        private string GetExchangeLookUp(string symbolID)
        {
            var sym = nameLookUp.Where(n => n.Value.ToUpper() == symbolID).Select(f => f.Key).FirstOrDefault();
            if (string.IsNullOrEmpty(sym) == false)
            {
                symbolID = sym;
            }

            string output = "";
            if (exchangeLookUp.TryGetValue(symbolID.ToLower(), out output))
            {
                return output;
            }
            return "FOREX";

            //String exchange = "";
            //String selector = "JobsManager";
            //switch (Mode)
            //{
            //    case MODE.Test:
            //        {
            //            selector += "_TEST";
            //        }
            //        break;

            //    case MODE.Live:
            //        {
            //            selector += "_LIVE";
            //        }
            //        break;
            //}

            //var settings = System.Configuration.ConfigurationManager.ConnectionStrings[selector];

            //try
            //{
            //    using (SqlConnection c = new SqlConnection(settings.ToString()))
            //    {
            //        c.Open();
            //        String query = "proc_GetExchangeLookUp";

            //        SqlCommand sqlCommand = new SqlCommand(query, c);
            //        sqlCommand.CommandType = CommandType.StoredProcedure;
            //        sqlCommand.Parameters.AddWithValue("@keywordvar", symbolID);

            //        SqlDataReader reader = sqlCommand.ExecuteReader();

            //        while (reader.Read())
            //        {
            //            exchange = reader["Exchange"].ToString();
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //}
            //return exchange;
        }

        private void LoadExchangeLookUp()
        {
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

            try
            {
                using (SqlConnection c = new SqlConnection(settings.ToString()))
                {
                    c.Open();
                    String query = "proc_GetAllExchangeLookUp";

                    SqlCommand sqlCommand = new SqlCommand(query, c);
                    sqlCommand.CommandType = CommandType.StoredProcedure;

                    SqlDataReader reader = sqlCommand.ExecuteReader();

                    while (reader.Read())
                    {
                        var symbolID = reader["SymbolID"].ToString();
                        var exchange = reader["Exchange"].ToString();

                        exchangeLookUp.Add(symbolID, exchange);
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

        private Dictionary<String, TradeSummary> SelectTradeSummary(String symbolID, string timeFrame, out long selectedTicks)
        {
            Dictionary<String, TradeSummary> lookUpTradeSummaries = new Dictionary<string, TradeSummary>();
            selectedTicks = new TimeSpan(1, 0, 0, 0).Ticks;

            var exchange = GetExchangeLookUp(symbolID);

            //t6urn this into a method
            switch (timeFrame)
            {
                case "1min":
                    {
                        if (exchange == "NYMEX") lookUpTradeSummaries = oneMinuteTradeSummaries_COMMODITIES_LOOKUP;
                        else if (exchange == "Forex" || exchange == "FOREX") lookUpTradeSummaries = oneMinuteTradeSummaries_FOREX_LOOKUP_X;
                        else lookUpTradeSummaries = oneMinuteTradeSummaries_INDICES_LOOKUP;

                        //lookUpTradeSummaries = oneMinuteTradeSummaries_FOREX_LOOKUP_X;
                        selectedTicks = new TimeSpan(0, 1, 0).Ticks;
                    } break;

                case "5min":
                    {
                        if (exchange == "NYMEX") lookUpTradeSummaries = fiveMinuteTradeSummaries_COMMODITIES_LOOKUP;
                        else if (exchange == "Forex" || exchange == "FOREX") lookUpTradeSummaries = fiveMinuteTradeSummaries_FOREX_LOOKUP_X;
                        else lookUpTradeSummaries = fiveMinuteTradeSummaries_INDICES_LOOKUP;


                        //lookUpTradeSummaries = fiveMinuteTradeSummaries_FOREX_LOOKUP_X;
                        selectedTicks = new TimeSpan(0, 5, 0).Ticks;
                    } break;

                case "10min":
                    {
                        if (exchange == "NYMEX") lookUpTradeSummaries = tenMinuteTradeSummaries_COMMODITIES_LOOKUP;
                        else if (exchange == "Forex" || exchange == "FOREX") lookUpTradeSummaries = tenMinuteTradeSummaries_FOREX_LOOKUP_X;
                        else lookUpTradeSummaries = tenMinuteTradeSummaries_INDICES_LOOKUP;

                        //lookUpTradeSummaries = fifteenMinuteTradeSummaries_FOREX_LOOKUPX;
                        selectedTicks = new TimeSpan(0, 15, 0).Ticks;

                    } break;

                case "15min":
                    {
                        if (exchange == "NYMEX") lookUpTradeSummaries = fifteenMinuteTradeSummaries_INDICES_LOOKUP;
                        else if (exchange == "Forex" || exchange == "FOREX") lookUpTradeSummaries = fifteenMinuteTradeSummaries_FOREX_LOOKUPX;
                        else lookUpTradeSummaries = fifteenMinuteTradeSummaries_INDICES_LOOKUP;

                        //lookUpTradeSummaries = fifteenMinuteTradeSummaries_FOREX_LOOKUPX;
                        selectedTicks = new TimeSpan(0, 15, 0).Ticks;

                    } break;

                case "30min":
                    {
                        if (exchange == "NYMEX") lookUpTradeSummaries = thrityMinuteTradeSummaries_COMMODITIES_LOOKUP;
                        else if (exchange == "Forex" || exchange == "FOREX") lookUpTradeSummaries = thrityMinuteTradeSummaries_FOREX_LOOKUPX;
                        else lookUpTradeSummaries = thrityMinuteTradeSummaries_INDICES_LOOKUP;

                        //lookUpTradeSummaries = thrityMinuteTradeSummaries_FOREX_LOOKUPX;
                        selectedTicks = new TimeSpan(0, 30, 0).Ticks;

                    } break;

                case "1hour":
                    {
                        if (exchange == "NYMEX") lookUpTradeSummaries = oneHourTradeSummaries_COMMODITIES_LOOKUP;
                        else if (exchange == "Forex" || exchange == "FOREX") lookUpTradeSummaries = oneHourTradeSummaries_FOREX_LOOKUPX;
                        else lookUpTradeSummaries = oneHourTradeSummaries_INDICES_LOOKUP;

                        //lookUpTradeSummaries = oneHourTradeSummaries_FOREX_LOOKUPX;
                        selectedTicks = new TimeSpan(1, 0, 0).Ticks;

                    } break;

                case "2hour":
                    {
                        if (exchange == "NYMEX") lookUpTradeSummaries = twoHourTradeSummaries_COMMODITIES_LOOKUP;
                        else if (exchange == "Forex" || exchange == "FOREX") lookUpTradeSummaries = twoHourTradeSummaries_FOREX_LOOKUPX;
                        else lookUpTradeSummaries = twoHourTradeSummaries_INDICES_LOOKUP;

                        //lookUpTradeSummaries = twoHourTradeSummaries_FOREX_LOOKUPX;
                        selectedTicks = new TimeSpan(2, 0, 0).Ticks;

                    } break;

                case "3hour":
                    {
                        if (exchange == "NYMEX") lookUpTradeSummaries = threeHourTradeSummaries_COMMODITIES_LOOKUP;
                        else if (exchange == "Forex" || exchange == "FOREX") lookUpTradeSummaries = threeHourTradeSummaries_FOREX_LOOKUPX;
                        else lookUpTradeSummaries = threeHourTradeSummaries_INDICES_LOOKUP;

                        // lookUpTradeSummaries = threeHourTradeSummaries_FOREX_LOOKUPX;
                        selectedTicks = new TimeSpan(3, 0, 0).Ticks;

                    } break;

                case "4hour":
                    {
                        if (exchange == "NYMEX") lookUpTradeSummaries = fourHourTradeSummaries_COMMODITIES_LOOKUP;
                        else if (exchange == "Forex" || exchange == "FOREX") lookUpTradeSummaries = fourHourTradeSummaries_FOREX_LOOKUPX;
                        else lookUpTradeSummaries = fourHourTradeSummaries_INDICES_LOOKUP;

                        //lookUpTradeSummaries = fourHourTradeSummaries_FOREX_LOOKUPX;
                        selectedTicks = new TimeSpan(4, 0, 0).Ticks;

                    } break;

                case "EndOfDay":
                    {
                        if (exchange == "NYMEX") lookUpTradeSummaries = dayTradeSummaries_COMMODITIES_LOOKUP;
                        else if (exchange == "Forex" || exchange == "FOREX") lookUpTradeSummaries = dayTradeSummaries_FOREX_LOOKUP;
                        else lookUpTradeSummaries = dayTradeSummaries_INDICES_LOOKUP;


                        // lookUpTradeSummaries = dayTradeSummaries_FOREX_LOOKUP;
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

                case "economic activity":
                    {
                        lookUpEconomicFundamentals = economicActivityFundamentals;
                    } break;

                case "confidence index":
                    {
                        lookUpEconomicFundamentals = confidenceIndexFundamentals;
                    } break;

                case "inflation":
                    {
                        lookUpEconomicFundamentals = inflationFundamentals;
                    } break;

                case "bonds":
                    {
                        lookUpEconomicFundamentals = bondsFundamentals;
                    } break;

                case "balance":
                    {
                        lookUpEconomicFundamentals = balanceFundamentals;
                    } break;

                case "credit":
                    {
                        lookUpEconomicFundamentals = creditFundamentals;
                    } break;

                case "sanction":
                    {
                       lookUpEconomicFundamentals = sanctionEvents;
                    } break;
                case "pandemic":
                    {
                        lookUpEconomicFundamentals = pandemicEvents;
                    } break;
                case "war":
                    {
                        lookUpEconomicFundamentals = warEvents;
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
}
