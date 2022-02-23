using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

using System.Data;
using System.Data.SqlClient;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using System.IO;
using Receiver;
using System.Collections.Concurrent;
using Microsoft.Win32;
using System.Timers;
using System.Globalization;

namespace RealTimeRawDataService
{
    public enum MODE
    {
        Live = 0,
        Test = 1,
        LiveTest = 2
    }

    //public struct TradeSummaryPairing
    //{
    //    public int LastIndex;
    //    public bool Updated;
    //    public List<TradeSummary> TradeList;
    //}

    public struct TradeSummaryPairing
    {
        public int LastIndex;
        public bool Updated;
        public Dictionary<String, TradeSummary> TradeList;
    }

    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in both code and config file together.
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class RealTimeRawDataService : IRealTimeRawDataService
    {

        public static Dictionary<String, TradeSummary> dayTradeSummaries_AMEX_LOOKUP = new Dictionary<String, TradeSummary>();
        public static Dictionary<String, TradeSummary> dayTradeSummaries_NYSE_LOOKUP = new Dictionary<String, TradeSummary>();
        public static Dictionary<String, TradeSummary> dayTradeSummaries_NASDAQ_LOOKUP = new Dictionary<String, TradeSummary>();
        public static Dictionary<String, TradeSummary> dayTradeSummaries_LSE_LOOKUP = new Dictionary<String, TradeSummary>();

        public static Dictionary<String, TradeSummary> oneMinuteTradeSummaries_FOREX_LOOKUPX = new Dictionary<String, TradeSummary>();
        public static Dictionary<String, TradeSummary> twoMinuteTradeSummaries_FOREX_LOOKUP = new Dictionary<String, TradeSummary>();

        public static Dictionary<String, List<TradeSummary>> oneMinuteTradeSummaries_FOREX_LOOKUP = new Dictionary<String, List<TradeSummary>>();


        public static Dictionary<String, TradeSummaryPairing> tempMinuteTradeSummaries_FOREX_LOOKUP = new Dictionary<String, TradeSummaryPairing>();


        public static Dictionary<String, TradeSummaryPairing> oneMinuteTradeSummaries_FOREX_LOOKUP_X = new Dictionary<String, TradeSummaryPairing>();
        public static Dictionary<String, TradeSummaryPairing> fiveMinuteTradeSummaries_FOREX_LOOKUP_X = new Dictionary<String, TradeSummaryPairing>();
        public static Dictionary<String, TradeSummaryPairing> tenMinuteTradeSummaries_FOREX_LOOKUP_X = new Dictionary<String, TradeSummaryPairing>();

        public static Dictionary<String, TradeSummaryPairing> fifteenMinuteTradeSummaries_FOREX_LOOKUPX = new Dictionary<String, TradeSummaryPairing>();
        public static Dictionary<String, TradeSummaryPairing> thrityMinuteTradeSummaries_FOREX_LOOKUPX = new Dictionary<String, TradeSummaryPairing>();
        public static Dictionary<String, TradeSummaryPairing> oneHourTradeSummaries_FOREX_LOOKUPX = new Dictionary<String, TradeSummaryPairing>();
        public static Dictionary<String, TradeSummaryPairing> twoHourTradeSummaries_FOREX_LOOKUPX = new Dictionary<String, TradeSummaryPairing>();
        public static Dictionary<String, TradeSummaryPairing> threeHourTradeSummaries_FOREX_LOOKUPX = new Dictionary<String, TradeSummaryPairing>();
        public static Dictionary<String, TradeSummaryPairing> fourHourTradeSummaries_FOREX_LOOKUPX = new Dictionary<String, TradeSummaryPairing>();
        public static Dictionary<String, TradeSummaryPairing> dailyTradeSummaries_FOREX_LOOKUPX = new Dictionary<String, TradeSummaryPairing>();


        //INDICES Lookups
        public static Dictionary<String, TradeSummaryPairing> oneMinuteTradeSummaries_INDICES_LOOKUP = new Dictionary<String, TradeSummaryPairing>();
        public static Dictionary<String, TradeSummaryPairing> fiveMinuteTradeSummaries_INDICES_LOOKUP = new Dictionary<String, TradeSummaryPairing>();
        public static Dictionary<String, TradeSummaryPairing> tenMinuteTradeSummaries_INDICES_LOOKUP = new Dictionary<String, TradeSummaryPairing>();
        public static Dictionary<String, TradeSummaryPairing> fifteenMinuteTradeSummaries_INDICES_LOOKUP = new Dictionary<String, TradeSummaryPairing>();
        public static Dictionary<String, TradeSummaryPairing> thrityMinuteTradeSummaries_INDICES_LOOKUP = new Dictionary<String, TradeSummaryPairing>();
        public static Dictionary<String, TradeSummaryPairing> oneHourTradeSummaries_INDICES_LOOKUP = new Dictionary<String, TradeSummaryPairing>();
        public static Dictionary<String, TradeSummaryPairing> twoHourTradeSummaries_INDICES_LOOKUP = new Dictionary<String, TradeSummaryPairing>();
        public static Dictionary<String, TradeSummaryPairing> threeHourTradeSummaries_INDICES_LOOKUP = new Dictionary<String, TradeSummaryPairing>();
        public static Dictionary<String, TradeSummaryPairing> fourHourTradeSummaries_INDICES_LOOKUP = new Dictionary<String, TradeSummaryPairing>();
        public static Dictionary<String, TradeSummaryPairing> dayTradeSummaries_INDICES_LOOKUP = new Dictionary<String, TradeSummaryPairing>();


        //COMMODITIES Lookups
        public static Dictionary<String, TradeSummaryPairing> oneMinuteTradeSummaries_COMMODITIES_LOOKUP = new Dictionary<String, TradeSummaryPairing>();
        public static Dictionary<String, TradeSummaryPairing> fiveMinuteTradeSummaries_COMMODITIES_LOOKUP = new Dictionary<String, TradeSummaryPairing>();
        public static Dictionary<String, TradeSummaryPairing> tenMinuteTradeSummaries_COMMODITIES_LOOKUP = new Dictionary<String, TradeSummaryPairing>();
        public static Dictionary<String, TradeSummaryPairing> fifteenMinuteTradeSummaries_COMMODITIES_LOOKUP = new Dictionary<String, TradeSummaryPairing>();
        public static Dictionary<String, TradeSummaryPairing> thrityMinuteTradeSummaries_COMMODITIES_LOOKUP = new Dictionary<String, TradeSummaryPairing>();
        public static Dictionary<String, TradeSummaryPairing> oneHourTradeSummaries_COMMODITIES_LOOKUP = new Dictionary<String, TradeSummaryPairing>();
        public static Dictionary<String, TradeSummaryPairing> twoHourTradeSummaries_COMMODITIES_LOOKUP = new Dictionary<String, TradeSummaryPairing>();
        public static Dictionary<String, TradeSummaryPairing> threeHourTradeSummaries_COMMODITIES_LOOKUP = new Dictionary<String, TradeSummaryPairing>();
        public static Dictionary<String, TradeSummaryPairing> fourHourTradeSummaries_COMMODITIES_LOOKUP = new Dictionary<String, TradeSummaryPairing>();
        public static Dictionary<String, TradeSummaryPairing> dayTradeSummaries_COMMODITIES_LOOKUP = new Dictionary<String, TradeSummaryPairing>();


        List<int> _intervalCount = new List<int>() { 1, 5, 10, 15, 30, 60, 120, 180, 240, 1440 };

        private static RealTimeDataHandler _realTimeDataHandler = null;

        private static bool _synchronizationMode = false;
        private static int _synchronizationCount = 0;

        private static Dictionary<String, bool> _synchDictionary = new Dictionary<String, bool>();
        public static MODE Mode;

        System.Timers.Timer actionTimer;

        public RealTimeRawDataService()
        {
            TradeSummaryDelegate tradeSummaryDelegate = new TradeSummaryDelegate(SetTradeSummaryHandler);

            _realTimeDataHandler = new RealTimeDataHandler(tradeSummaryDelegate, Receiver.RealTimeDataHandler.RecipientAppID);

            var dataFeedMode = System.Configuration.ConfigurationManager.AppSettings["DATAFEED_MODE"];

            if (dataFeedMode != "FILE")
            {
                //Synchronize with the market
                if (oneMinuteTradeSummaries_FOREX_LOOKUP_X.Count() == 0)
                {
                    _synchronizationMode = true;

                    Console.WriteLine("Synchronization Request Mode");

                    ThreadStart threadStart = new ThreadStart(Synchronize);
                    Thread synchronizeThread = new Thread(threadStart);
                    synchronizeThread.Start();
                }
            }
            //Temp
            Mode = MODE.Test;
            ApplyScheduler();
        }

        public string GetData(int value)
        {
            return string.Format("You entered: {0}", value);
        }

        private void Synchronize()
        {
            DateTime start = DateTime.Now;

            //Call datafeed fetch latest
            List<string> symbolList = new List<string>();
            symbolList.AddRange(_realTimeDataHandler.GetSymbolList("FOREX"));
            //symbolList.AddRange(_realTimeDataHandler.GetSymbolList("NYMEX"));
            //symbolList.AddRange(_realTimeDataHandler.GetSymbolList("NASDAQ"));
            //symbolList.AddRange(_realTimeDataHandler.GetSymbolList("EUREX"));
            //symbolList.AddRange(_realTimeDataHandler.GetSymbolList("INDEX"));


            _synchronizationCount = symbolList.Count * _intervalCount.Count;

            if (symbolList != null)
            {
                Console.WriteLine("StartTime :: {0} Number of Symbols {1}:: ", start.ToString(), symbolList.Count);
                Library.WriteErrorLog("StartTime :: "+start.ToString()+" Number of Symbols "+symbolList.Count);

                foreach (var interval in _intervalCount)
                {
                    int seconds = interval * 60;

                    foreach (var item in symbolList)
                    {
                        String timeFrame = "";

                        switch (seconds)
                        {
                            case 60:
                                {
                                    timeFrame = "1min";
                                } break;

                            //case 120:
                            //    {
                            //        timeFrame = "2min";
                            //    } break;

                            //case 180:
                            //    {
                            //        timeFrame = "3min";
                            //    } break;

                            //case 240:
                            //    {
                            //        timeFrame = "4min";
                            //    } break;

                            case 300:
                                {
                                    timeFrame = "5min";
                                } break;

                            //case 360:
                            //    {
                            //        timeFrame = "6min";
                            //    } break;

                            case 600:
                                {
                                    timeFrame = "10min";
                                } break;

                            case 900:
                                {
                                    timeFrame = "15min";
                                } break;

                            case 1800:
                                {
                                    timeFrame = "30min";
                                } break;

                            case 3600:
                                {
                                    timeFrame = "1hour";
                                } break;

                            case 7200:
                                {
                                    timeFrame = "2hour";
                                } break;

                            case 10800:
                                {
                                    timeFrame = "3hour";
                                } break;

                            case 14400:
                                {
                                    timeFrame = "4hour";
                                } break;

                            case 86400:
                                {
                                    timeFrame = "EndOfDay";
                                } break;
                        }


                        try
                        {
                            if (String.IsNullOrEmpty(timeFrame) == false)
                            {
                                _synchDictionary.Add(item + timeFrame, false);
                                //_synchronizationCount++;

                                CultureInfo InvC = new CultureInfo("en-GB");

                                DateTime dateItem = DateTime.Parse(System.Configuration.ConfigurationManager.AppSettings["DateStart"].ToString(), InvC);

                                if (Convert.ToBoolean(System.Configuration.ConfigurationManager.AppSettings["PREVIOUSTRADINGDAY"].ToString()))
                                {
                                    dateItem = DateTime.Now.AddDays(-1);
                                }
                                _realTimeDataHandler.GetIntradayDataAsynch(item, seconds, "FOREX", dateItem);

                                Thread.Sleep(1200);

                                //Thread.Sleep(4000);

                                //Thread.Sleep(300);

                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());

                            Logger.log.Error("Failure during Synchronization Process :: " + ex.ToString());
                            Library.WriteErrorLog(DateTime.Now + "::" + "Failure during Synchronization Process :: " + ex.ToString());

                        }
                    }
                }
                //_synchronizationMode = false;

                DateTime endTime = DateTime.Now;
                TimeSpan timeSpan = endTime - start;

                Console.WriteLine("EndTime :: {0} Total Time in Minutes:: {1}", DateTime.Now.ToString(), timeSpan.TotalMinutes);
                Library.WriteErrorLog(DateTime.Now + "::" + "EndTime :: "+ DateTime.Now.ToString()+"Total Time in Minutes:: "+timeSpan.TotalMinutes);


                Console.WriteLine("Synchronization Request Complete");
                Library.WriteErrorLog(DateTime.Now + "::" + "Synchronization Request Complete");

            }
        }


        //private void Synchronize()
        //{
        //    DateTime start = DateTime.Now;

        //    //Call datafeed fetch latest
        //    var symbolList = _realTimeDataHandler.GetSymbolList("FOREX");

        //    Console.WriteLine("StartTime :: {0} Number of Symbols {1}:: ", start.ToString(), symbolList.Count);

        //    foreach (var interval in _intervalCount)
        //    {
        //        Dictionary<String, TradeSummaryPairing> tradeSummariesSynchBackLog_FOREX_LOOKUP = new Dictionary<String, TradeSummaryPairing>();

        //        int seconds = interval * 60;

        //        foreach (var item in symbolList)
        //        {
        //            _synchDictionary.Add(item, false);

        //            //var tradeSummaryList = _realTimeDataHandler.GetIntradayData(item, seconds, "FOREX", new DateTime(2014, 12, 26));
        //            DateTime dateItem = DateTime.Parse(System.Configuration.ConfigurationManager.AppSettings["DateStart"].ToString());

        //            var tradeSummaryList = _realTimeDataHandler.GetIntradayDataAsynch(item, seconds, "FOREX", dateItem);

        //            TradeSummaryPairing tradeSummaryPairing = new TradeSummaryPairing();
        //            tradeSummaryPairing.TradeList = new Dictionary<String, TradeSummary>();

        //            foreach (var tradeItem in tradeSummaryList)
        //            {
        //                String lookUpID = tradeItem.SymbolID + "#" + tradeItem.DateTime.Ticks.ToString();

        //                TradeSummary tester = new TradeSummary();
        //                if (tradeSummaryPairing.TradeList.TryGetValue(lookUpID, out tester) == false)
        //                tradeSummaryPairing.TradeList.Add(lookUpID, tradeItem);
        //            }
        //            tradeSummariesSynchBackLog_FOREX_LOOKUP.Add(item, tradeSummaryPairing);
        //        }

        //        if (interval == 1)
        //        {
        //            foreach (var item in tempMinuteTradeSummaries_FOREX_LOOKUP)
        //            {
        //                TradeSummaryPairing tradeSummaryPairing = new TradeSummaryPairing();

        //                if (tradeSummariesSynchBackLog_FOREX_LOOKUP.TryGetValue(item.Key, out tradeSummaryPairing))
        //                {
        //                    //Update with the latest real time updates for when the synchronization process was busy
        //                    foreach (var tradeSumItem in item.Value.TradeList.Values)
        //                    {
        //                        String lookUpID = tradeSumItem.SymbolID + "#" + tradeSumItem.DateTime.Ticks.ToString();

        //                        TradeSummary tradeSum = new TradeSummary();
        //                        if (tradeSummaryPairing.TradeList.TryGetValue(lookUpID, out tradeSum) == false)
        //                        {
        //                            if (tradeSummaryPairing.TradeList.Count() > 0)
        //                            {
        //                                if (tradeSumItem.DateTime > tradeSummaryPairing.TradeList.Last().Value.DateTime)
        //                                    tradeSummaryPairing.TradeList.Add(lookUpID, tradeSumItem);
        //                            }
        //                            else
        //                            {
        //                                tradeSummaryPairing.TradeList.Add(lookUpID, tradeSumItem);
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }

        //        switch (interval)
        //        {
        //            case 1:
        //                {
        //                    oneMinuteTradeSummaries_FOREX_LOOKUP_X = tradeSummariesSynchBackLog_FOREX_LOOKUP;
        //                } break;

        //            case 2:
        //                {

        //                } break;

        //            case 3:
        //                {

        //                } break;

        //            case 4:
        //                {

        //                } break;

        //            case 5:
        //                {
        //                    fiveMinuteTradeSummaries_FOREX_LOOKUP_X = tradeSummariesSynchBackLog_FOREX_LOOKUP;
        //                } break;

        //            case 10:
        //                {

        //                } break;

        //            case 15:
        //                {
        //                    fifteenMinuteTradeSummaries_FOREX_LOOKUPX = tradeSummariesSynchBackLog_FOREX_LOOKUP;
        //                } break;

        //            case 30:
        //                {
        //                    thrityMinuteTradeSummaries_FOREX_LOOKUPX = tradeSummariesSynchBackLog_FOREX_LOOKUP;
        //                } break;

        //            case 60:
        //                {
        //                    oneHourTradeSummaries_FOREX_LOOKUPX = tradeSummariesSynchBackLog_FOREX_LOOKUP;
        //                } break;

        //            case 120:
        //                {
        //                    twoHourTradeSummaries_FOREX_LOOKUPX = tradeSummariesSynchBackLog_FOREX_LOOKUP;
        //                } break;

        //            case 180:
        //                {
        //                    threeHourTradeSummaries_FOREX_LOOKUPX = tradeSummariesSynchBackLog_FOREX_LOOKUP;
        //                } break;
        //        }
        //    }
        //    //_synchronizationMode = false;

        //    DateTime endTime = DateTime.Now;
        //    TimeSpan timeSpan = endTime - start;

        //    Console.WriteLine("EndTime :: {0} Total Time in Minutes:: {1}", DateTime.Now.ToString(), timeSpan.TotalMinutes);

        //    Console.WriteLine("Synchronization Request Complete");
        //}

        public void InitialseDataTables()
        {
            PopulateDataTables();
        }

        private void PopulateDataTables()
        {
            Console.WriteLine("\n\n");

            //Starttime
            DateTime startTime = DateTime.Now;
            String triggerlogFileName = "GetAllDataX.txt";

            if (!System.IO.File.Exists(triggerlogFileName))
            {
                //crude approach
                using (System.IO.FileStream fs = System.IO.File.Create(triggerlogFileName)) { }
            }

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(triggerlogFileName, true))
            {
                file.WriteLine(String.Format("StartTime  {0}  {1}", DateTime.Now.TimeOfDay, DateTime.Now));
            }

            Console.WriteLine("Data fetching in progress");

            InitializeData();


            //end time
            TimeSpan span = DateTime.Now - startTime;

            Console.WriteLine(String.Format("EndTime {0} {1} Total :: {2}", DateTime.Now.TimeOfDay, DateTime.Now, span));

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(triggerlogFileName, true))
            {
                file.WriteLine(String.Format("EndTime  {0}  {1} Total Duration::{2}", DateTime.Now.TimeOfDay, DateTime.Now, span));
                file.WriteLine("");
            }
        }

        private void InitializeData()
        {
            String currentTitle = Console.Title;

            try
            {

                //using (StreamReader reader = new StreamReader("C:\\Users\\dowusu-ansah\\Downloads\\Data\\Template\\"
                //    + "RealTimeChartPatternRecognition\\PatternData\\HeadAndShoulders\\FOREX_EURGBP_Period 20000104 - 20140225\\EURGBP_Period 20000104 - 20140225.csv"))

                //using (StreamReader reader = new StreamReader("..\\..\\..\\..\\"
                //    + "..\\PatternData\\HeadAndShoulders\\FOREX_EURGBP_Period 20000104 - 20140225\\EURGBP_Data.csv"))

                List<String> fileList = new List<String>() { 
                    "..\\..\\..\\..\\EURUSD_15min.csv", 
                    "..\\..\\..\\..\\GBPUSD_15min.csv", 
                    "..\\..\\..\\..\\USDJPY_30min.csv" 
                };

                foreach (var item in fileList)
                {

                    using (StreamReader reader = new StreamReader(item))
                    {
                        CsvReader csvReader = new CsvReader(reader);
                        while (csvReader.Read())
                        {
                            TradeSummary tradeSummary = new TradeSummary();

                            tradeSummary.DateTime = csvReader.GetField<DateTime>("DateTime");
                            tradeSummary.Open = csvReader.GetField<Double>("Open");
                            tradeSummary.High = csvReader.GetField<Double>("High");

                            tradeSummary.Low = csvReader.GetField<Double>("Low");
                            tradeSummary.Close = csvReader.GetField<Double>("Close");
                            tradeSummary.Volume = csvReader.GetField<int>("Volume");
                            tradeSummary.TimeFrame = csvReader.GetField<String>("TimeFrame");
                            tradeSummary.SymbolID = csvReader.GetField<String>("SymbolID");

                            SetTradeSummaryX(tradeSummary);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error :: " + ex.ToString());
                Logger.log.Error("File Read Dummy Data Failure :: " + ex.ToString());
            }
        }

        private void GenerateIntervals(String symbolID)
        {
            foreach (var item in _intervalCount)
            {
                if (item != 1)
                {
                    CheckForIntervalUpdates(item, symbolID);
                }
            }
        }

        private void CheckForIntervalUpdates(int intervalCount, String symbolID)
        {
            Dictionary<String, TradeSummaryPairing> timeFrameLookup = null;

            timeFrameLookup = oneMinuteTradeSummaries_FOREX_LOOKUP_X;
            int lastIndex = 0;
            string operatingTimeFrame = "";

            TradeSummaryPairing tradeSumPairing = new TradeSummaryPairing();
            Dictionary<String, TradeSummaryPairing> baseTradeSummaryCollection = null;

            switch (intervalCount)
            {
                case 5:
                    {
                        if (fiveMinuteTradeSummaries_FOREX_LOOKUP_X.Count > 0)
                        {
                            if (fiveMinuteTradeSummaries_FOREX_LOOKUP_X.TryGetValue(symbolID, out tradeSumPairing))
                                lastIndex = fiveMinuteTradeSummaries_FOREX_LOOKUP_X[symbolID].LastIndex;
                        }
                        baseTradeSummaryCollection = fiveMinuteTradeSummaries_FOREX_LOOKUP_X;
                        operatingTimeFrame = "5min";
                    } break;

                case 10:
                    {
                        if (tenMinuteTradeSummaries_FOREX_LOOKUP_X.Count > 0)
                        {
                            if (tenMinuteTradeSummaries_FOREX_LOOKUP_X.TryGetValue(symbolID, out tradeSumPairing))
                                lastIndex = tenMinuteTradeSummaries_FOREX_LOOKUP_X[symbolID].LastIndex;
                        }
                        baseTradeSummaryCollection = tenMinuteTradeSummaries_FOREX_LOOKUP_X;
                        operatingTimeFrame = "10min";
                    } break;

                case 15:
                    {
                        if (fifteenMinuteTradeSummaries_FOREX_LOOKUPX.Count > 0)
                        {
                            if (fifteenMinuteTradeSummaries_FOREX_LOOKUPX.TryGetValue(symbolID, out tradeSumPairing))
                                lastIndex = fifteenMinuteTradeSummaries_FOREX_LOOKUPX[symbolID].LastIndex;
                        }
                        baseTradeSummaryCollection = fifteenMinuteTradeSummaries_FOREX_LOOKUPX;
                        operatingTimeFrame = "15min";

                    } break;

                case 30:
                    {
                        if (thrityMinuteTradeSummaries_FOREX_LOOKUPX.Count > 0)
                        {
                            if (thrityMinuteTradeSummaries_FOREX_LOOKUPX.TryGetValue(symbolID, out tradeSumPairing))
                                lastIndex = thrityMinuteTradeSummaries_FOREX_LOOKUPX[symbolID].LastIndex;
                        }
                        baseTradeSummaryCollection = thrityMinuteTradeSummaries_FOREX_LOOKUPX;
                        operatingTimeFrame = "30min";


                    } break;

                case 60:
                    {
                        if (oneHourTradeSummaries_FOREX_LOOKUPX.Count > 0)
                        {
                            if (oneHourTradeSummaries_FOREX_LOOKUPX.TryGetValue(symbolID, out tradeSumPairing))
                                lastIndex = oneHourTradeSummaries_FOREX_LOOKUPX[symbolID].LastIndex;
                        }
                        baseTradeSummaryCollection = oneHourTradeSummaries_FOREX_LOOKUPX;
                        operatingTimeFrame = "1hour";


                    } break;

                case 120:
                    {
                        if (twoHourTradeSummaries_FOREX_LOOKUPX.Count > 0)
                        {
                            if (twoHourTradeSummaries_FOREX_LOOKUPX.TryGetValue(symbolID, out tradeSumPairing))
                                lastIndex = twoHourTradeSummaries_FOREX_LOOKUPX[symbolID].LastIndex;
                        }
                        baseTradeSummaryCollection = twoHourTradeSummaries_FOREX_LOOKUPX;
                        operatingTimeFrame = "2hour";


                    } break;

                case 180:
                    {
                        if (threeHourTradeSummaries_FOREX_LOOKUPX.Count > 0)
                        {
                            if (threeHourTradeSummaries_FOREX_LOOKUPX.TryGetValue(symbolID, out tradeSumPairing))
                                lastIndex = threeHourTradeSummaries_FOREX_LOOKUPX[symbolID].LastIndex;
                        }
                        baseTradeSummaryCollection = threeHourTradeSummaries_FOREX_LOOKUPX;
                        operatingTimeFrame = "3hour";

                    } break;

                case 240:
                    {
                        if (fourHourTradeSummaries_FOREX_LOOKUPX.Count > 0)
                        {
                            if (fourHourTradeSummaries_FOREX_LOOKUPX.TryGetValue(symbolID, out tradeSumPairing))
                                lastIndex = fourHourTradeSummaries_FOREX_LOOKUPX[symbolID].LastIndex;
                        }
                        baseTradeSummaryCollection = fourHourTradeSummaries_FOREX_LOOKUPX;

                        operatingTimeFrame = "4hour";

                    } break;

                case 1440:
                    {
                        if (dailyTradeSummaries_FOREX_LOOKUPX.Count > 0)
                        {
                            if (dailyTradeSummaries_FOREX_LOOKUPX.TryGetValue(symbolID, out tradeSumPairing))
                                lastIndex = dailyTradeSummaries_FOREX_LOOKUPX[symbolID].LastIndex;
                        }
                        baseTradeSummaryCollection = dailyTradeSummaries_FOREX_LOOKUPX;

                        operatingTimeFrame = "EndOfDay";

                    } break;
            }

            bool bUpdated = false;
            int savedIndex = 0;

            if (timeFrameLookup != null)
            {
                Dictionary<String, TradeSummaryPairing> tempCollection = new Dictionary<String, TradeSummaryPairing>();

                TradeSummary tradeSummary = new TradeSummary();

                //holds one minute data
                var list = timeFrameLookup[symbolID];
                int iter = 0;


                for (int t = lastIndex; t < list.TradeList.Count; t++)
                {
                    if (iter == intervalCount - 1)
                    {
                        var tradeSummaryItem = list.TradeList.ElementAt(t);
                        savedIndex = t;
                        bUpdated = true;

                        //make a copy and then convert to the right timeframe as all incoming is 1min timeframe
                        AutoMapper.Mapper.CreateMap<TradeSummary, TradeSummary>();
                        tradeSummary = AutoMapper.Mapper.Map<TradeSummary>(tradeSummaryItem.Value);  
                        //new
                        tradeSummary.DateTime = RoundToNearestTimeFrame(tradeSummary.DateTime, operatingTimeFrame);
                        tradeSummary.TimeFrame = operatingTimeFrame;


                        if (tradeSummary.SymbolID == "EURUSD" && tradeSummary.TimeFrame == "1hour"
                        || tradeSummary.SymbolID == "USDCAD" && tradeSummary.TimeFrame == "1hour"
                        ||
                        tradeSummary.SymbolID == "EURUSD" && tradeSummary.TimeFrame == "5min"
                        || tradeSummary.SymbolID == "USDCAD" && tradeSummary.TimeFrame == "5min"
                        || tradeSummary.SymbolID == "EURUSD" && tradeSummary.TimeFrame == "10min"
                        || tradeSummary.SymbolID == "USDCAD" && tradeSummary.TimeFrame == "10min")
                        {
                            string msg = tradeSummary.SymbolID + ", " + tradeSummary.TimeFrame + ", " + string.Format("{0}-{1}-{2} {3}:{4}",
                                        tradeSummary.DateTime.Year,
                                        tradeSummary.DateTime.Month,
                                        tradeSummary.DateTime.Day,
                                        tradeSummary.DateTime.Hour,
                                        tradeSummary.DateTime.Minute);

                            var name = tradeSummary.SymbolID + tradeSummary.TimeFrame;
                            Library.WriteInfoLog(name, msg);
                        }

                        iter = 0;
                    }
                    iter++;
                }

                String lookUpID = tradeSummary.SymbolID + "#" + tradeSummary.DateTime.Ticks.ToString();

                if (bUpdated)
                {
                    TradeSummaryPairing tradeSummaryPairing = new TradeSummaryPairing();
                    if (baseTradeSummaryCollection.TryGetValue(symbolID, out tradeSummaryPairing))
                    {
                        TradeSummary tester = new TradeSummary();
                        if (tradeSummaryPairing.TradeList.TryGetValue(lookUpID, out tester) == false)
                        {
                            tradeSummaryPairing.LastIndex = savedIndex;
                            tradeSummaryPairing.TradeList.Add(lookUpID, tradeSummary);
                            tradeSummaryPairing.Updated = true;
                            baseTradeSummaryCollection[symbolID] = tradeSummaryPairing;

                            if (intervalCount == 1440)
                            {
                                Logger.log.Info(tradeSummary.TimeFrame + " Update :: " + tradeSummary.SymbolID + " " + tradeSummary.DateTime);
                            }
                        }
                    }
                    else
                    {
                        tradeSummaryPairing = new TradeSummaryPairing();

                        tradeSummaryPairing.TradeList = new Dictionary<String, TradeSummary>();
                        tradeSummaryPairing.LastIndex = savedIndex;
                        tradeSummaryPairing.Updated = true;

                        tradeSummaryPairing.TradeList.Add(lookUpID, tradeSummary);
                        baseTradeSummaryCollection.Add(symbolID, tradeSummaryPairing);

                        if (intervalCount == 1440)
                        {
                            Logger.log.Info(tradeSummary.TimeFrame + " Update :: " + tradeSummary.SymbolID + " " + tradeSummary.DateTime);
                        }
                    }
                }
            }
        }

        //After the RT JM has generated the minute level tickets..it must call this method
        public List<UpdatedTimeFrame> GetUpdatedTimeFrameList()
        {
            //1min timeframe gets updated automatically
            //RT JM creates a 1min irrespective of the timeframe flags for 1min

            List<UpdatedTimeFrame> updatedTimeFrameList = new List<UpdatedTimeFrame>();

            foreach (var item in _intervalCount)
            {
                //Go through time frames with updates and reset flags tp false
                Dictionary<String, TradeSummaryPairing> timeFrameLookup = null;

                String timeFrameStr = "";
                switch (item)
                {
                    case 5:
                        {
                            timeFrameLookup = fiveMinuteTradeSummaries_FOREX_LOOKUP_X;
                            timeFrameStr = "5min";
                        } break;

                    case 10:
                        {
                            timeFrameLookup = tenMinuteTradeSummaries_FOREX_LOOKUP_X;
                            timeFrameStr = "10min";
                        } break;

                    case 15:
                        {
                            timeFrameLookup = fifteenMinuteTradeSummaries_FOREX_LOOKUPX;
                            timeFrameStr = "15min";
                        } break;

                    case 30:
                        {
                            timeFrameLookup = thrityMinuteTradeSummaries_FOREX_LOOKUPX;
                            timeFrameStr = "30min";

                        } break;

                    case 60:
                        {
                            timeFrameLookup = oneHourTradeSummaries_FOREX_LOOKUPX;
                            timeFrameStr = "1hour";

                        } break;

                    case 120:
                        {
                            timeFrameLookup = twoHourTradeSummaries_FOREX_LOOKUPX;
                            timeFrameStr = "2hour";

                        } break;

                    case 180:
                        {
                            timeFrameLookup = threeHourTradeSummaries_FOREX_LOOKUPX;
                            timeFrameStr = "3hour";

                        } break;

                    case 240:
                        {
                            timeFrameLookup = fourHourTradeSummaries_FOREX_LOOKUPX;
                            timeFrameStr = "4hour";

                        } break;

                    case 1440:
                        {
                            timeFrameLookup = dailyTradeSummaries_FOREX_LOOKUPX;
                            timeFrameStr = "EndOfDay";

                        } break;
                }

                if (timeFrameLookup != null)
                {
                    for (int j = 0; j < timeFrameLookup.Count; j++)
                    {
                        var key = timeFrameLookup.Keys.ElementAt(j);
                        var list = timeFrameLookup[key];
                        TradeSummaryPairing tradePairing = list;

                        if (tradePairing.Updated)
                        {
                            UpdatedTimeFrame updatedTimeFrame = new UpdatedTimeFrame();
                            updatedTimeFrame.SymbolID = key;
                            updatedTimeFrame.TimeFrame = timeFrameStr;

                            updatedTimeFrameList.Add(updatedTimeFrame);
                            tradePairing.Updated = false;

                            timeFrameLookup[key] = tradePairing;
                        }
                    }
                }
            }
            return updatedTimeFrameList;
        }

        public void SetTradeSummaryX(TradeSummary tradeSummary)
        {
            DateTime tempNewDateTime = new DateTime(tradeSummary.DateTime.Year, tradeSummary.DateTime.Month,
                                                        tradeSummary.DateTime.Day, tradeSummary.DateTime.Hour,
                                                        tradeSummary.DateTime.Minute, 0, 0);
            tradeSummary.DateTime = tempNewDateTime;

            String lookUpID = tradeSummary.SymbolID + "#" + tradeSummary.DateTime.Ticks.ToString();

            String log = tradeSummary.SymbolID + " :: " + tempNewDateTime.ToString() + " :: CurrentTime Log ->" + DateTime.Now.ToString();
            Console.WriteLine(log);

            String symbolID = tradeSummary.SymbolID;

            try
            {

                switch (tradeSummary.TimeFrame)
                {
                    case "5min":
                        {
                            TradeSummaryPairing tradeSummaryPairing = new TradeSummaryPairing();

                            if (fiveMinuteTradeSummaries_FOREX_LOOKUP_X.TryGetValue(symbolID, out tradeSummaryPairing))
                            {
                                tradeSummaryPairing.LastIndex = 0;
                                tradeSummaryPairing.Updated = true;

                                TradeSummary tester = new TradeSummary();
                                if (tradeSummaryPairing.TradeList.TryGetValue(lookUpID, out tester) == false)
                                    tradeSummaryPairing.TradeList.Add(lookUpID, tradeSummary);

                                fiveMinuteTradeSummaries_FOREX_LOOKUP_X[symbolID] = tradeSummaryPairing;
                            }
                            else
                            {
                                tradeSummaryPairing = new TradeSummaryPairing();

                                tradeSummaryPairing.TradeList = new Dictionary<String, TradeSummary>();
                                tradeSummaryPairing.LastIndex = 0;
                                tradeSummaryPairing.Updated = false;

                                TradeSummary tester = new TradeSummary();
                                if (tradeSummaryPairing.TradeList.TryGetValue(lookUpID, out tester) == false)
                                    tradeSummaryPairing.TradeList.Add(lookUpID, tradeSummary);

                                fiveMinuteTradeSummaries_FOREX_LOOKUP_X.Add(symbolID, tradeSummaryPairing);
                            }
                        }
                        break;

                    case "10min":
                        {
                            TradeSummaryPairing tradeSummaryPairing = new TradeSummaryPairing();

                            if (tenMinuteTradeSummaries_FOREX_LOOKUP_X.TryGetValue(symbolID, out tradeSummaryPairing))
                            {
                                tradeSummaryPairing.LastIndex = 0;
                                tradeSummaryPairing.Updated = true;

                                TradeSummary tester = new TradeSummary();
                                if (tradeSummaryPairing.TradeList.TryGetValue(lookUpID, out tester) == false)
                                    tradeSummaryPairing.TradeList.Add(lookUpID, tradeSummary);

                                tenMinuteTradeSummaries_FOREX_LOOKUP_X[symbolID] = tradeSummaryPairing;
                            }
                            else
                            {
                                tradeSummaryPairing = new TradeSummaryPairing();

                                tradeSummaryPairing.TradeList = new Dictionary<String, TradeSummary>();
                                tradeSummaryPairing.LastIndex = 0;
                                tradeSummaryPairing.Updated = false;

                                TradeSummary tester = new TradeSummary();
                                if (tradeSummaryPairing.TradeList.TryGetValue(lookUpID, out tester) == false)
                                    tradeSummaryPairing.TradeList.Add(lookUpID, tradeSummary);

                                tenMinuteTradeSummaries_FOREX_LOOKUP_X.Add(symbolID, tradeSummaryPairing);
                            }

                        }
                        break;

                    case "15min":
                        {
                            TradeSummaryPairing tradeSummaryPairing = new TradeSummaryPairing();

                            if (fifteenMinuteTradeSummaries_FOREX_LOOKUPX.TryGetValue(symbolID, out tradeSummaryPairing))
                            {
                                tradeSummaryPairing.LastIndex = 0;
                                tradeSummaryPairing.Updated = true;

                                TradeSummary tester = new TradeSummary();
                                if (tradeSummaryPairing.TradeList.TryGetValue(lookUpID, out tester) == false)
                                    tradeSummaryPairing.TradeList.Add(lookUpID, tradeSummary);

                                fifteenMinuteTradeSummaries_FOREX_LOOKUPX[symbolID] = tradeSummaryPairing;
                            }
                            else
                            {
                                tradeSummaryPairing = new TradeSummaryPairing();

                                tradeSummaryPairing.TradeList = new Dictionary<String, TradeSummary>();
                                tradeSummaryPairing.LastIndex = 0;
                                tradeSummaryPairing.Updated = false;

                                TradeSummary tester = new TradeSummary();
                                if (tradeSummaryPairing.TradeList.TryGetValue(lookUpID, out tester) == false)
                                    tradeSummaryPairing.TradeList.Add(lookUpID, tradeSummary);

                                fifteenMinuteTradeSummaries_FOREX_LOOKUPX.Add(symbolID, tradeSummaryPairing);
                            }

                        }
                        break;

                    case "30min":
                        {
                            TradeSummaryPairing tradeSummaryPairing = new TradeSummaryPairing();
                            if (thrityMinuteTradeSummaries_FOREX_LOOKUPX.TryGetValue(symbolID, out tradeSummaryPairing))
                            {
                                tradeSummaryPairing.LastIndex = 0;
                                tradeSummaryPairing.Updated = true;

                                TradeSummary tester = new TradeSummary();
                                if (tradeSummaryPairing.TradeList.TryGetValue(lookUpID, out tester) == false)
                                    tradeSummaryPairing.TradeList.Add(lookUpID, tradeSummary);

                                thrityMinuteTradeSummaries_FOREX_LOOKUPX[symbolID] = tradeSummaryPairing;
                            }
                            else
                            {

                                tradeSummaryPairing = new TradeSummaryPairing();

                                tradeSummaryPairing.TradeList = new Dictionary<String, TradeSummary>();
                                tradeSummaryPairing.LastIndex = 0;
                                tradeSummaryPairing.Updated = false;

                                TradeSummary tester = new TradeSummary();
                                if (tradeSummaryPairing.TradeList.TryGetValue(lookUpID, out tester) == false)
                                    tradeSummaryPairing.TradeList.Add(lookUpID, tradeSummary);

                                thrityMinuteTradeSummaries_FOREX_LOOKUPX.Add(symbolID, tradeSummaryPairing);
                            }
                        }
                        break;

                    case "1hour":
                        {
                            TradeSummaryPairing tradeSummaryPairing = new TradeSummaryPairing();
                            if (oneHourTradeSummaries_FOREX_LOOKUPX.TryGetValue(symbolID, out tradeSummaryPairing))
                            {
                                tradeSummaryPairing.LastIndex = 0;
                                tradeSummaryPairing.Updated = true;

                                TradeSummary tester = new TradeSummary();
                                if (tradeSummaryPairing.TradeList.TryGetValue(lookUpID, out tester) == false)
                                    tradeSummaryPairing.TradeList.Add(lookUpID, tradeSummary);

                                oneHourTradeSummaries_FOREX_LOOKUPX[symbolID] = tradeSummaryPairing;
                            }
                            else
                            {
                                tradeSummaryPairing = new TradeSummaryPairing();

                                tradeSummaryPairing.TradeList = new Dictionary<String, TradeSummary>();
                                tradeSummaryPairing.LastIndex = 0;
                                tradeSummaryPairing.Updated = false;

                                TradeSummary tester = new TradeSummary();
                                if (tradeSummaryPairing.TradeList.TryGetValue(lookUpID, out tester) == false)
                                    tradeSummaryPairing.TradeList.Add(lookUpID, tradeSummary);

                                oneHourTradeSummaries_FOREX_LOOKUPX.Add(symbolID, tradeSummaryPairing);
                            }

                        }
                        break;

                    case "2hour":
                        {
                            TradeSummaryPairing tradeSummaryPairing = new TradeSummaryPairing();
                            if (twoHourTradeSummaries_FOREX_LOOKUPX.TryGetValue(symbolID, out tradeSummaryPairing))
                            {
                                tradeSummaryPairing.LastIndex = 0;
                                tradeSummaryPairing.Updated = true;

                                TradeSummary tester = new TradeSummary();
                                if (tradeSummaryPairing.TradeList.TryGetValue(lookUpID, out tester) == false)
                                    tradeSummaryPairing.TradeList.Add(lookUpID, tradeSummary);

                                twoHourTradeSummaries_FOREX_LOOKUPX[symbolID] = tradeSummaryPairing;
                            }
                            else
                            {
                                tradeSummaryPairing = new TradeSummaryPairing();

                                tradeSummaryPairing.TradeList = new Dictionary<String, TradeSummary>();
                                tradeSummaryPairing.LastIndex = 0;
                                tradeSummaryPairing.Updated = false;

                                TradeSummary tester = new TradeSummary();
                                if (tradeSummaryPairing.TradeList.TryGetValue(lookUpID, out tester) == false)
                                    tradeSummaryPairing.TradeList.Add(lookUpID, tradeSummary);

                                twoHourTradeSummaries_FOREX_LOOKUPX.Add(symbolID, tradeSummaryPairing);
                            }

                        }
                        break;

                    case "3hour":
                        {
                            TradeSummaryPairing tradeSummaryPairing = new TradeSummaryPairing();
                            if (threeHourTradeSummaries_FOREX_LOOKUPX.TryGetValue(symbolID, out tradeSummaryPairing))
                            {
                                tradeSummaryPairing.LastIndex = 0;

                                TradeSummary tester = new TradeSummary();
                                if (tradeSummaryPairing.TradeList.TryGetValue(lookUpID, out tester) == false)
                                    tradeSummaryPairing.TradeList.Add(lookUpID, tradeSummary);

                                tradeSummaryPairing.Updated = true;

                                threeHourTradeSummaries_FOREX_LOOKUPX[symbolID] = tradeSummaryPairing;
                            }
                            else
                            {
                                tradeSummaryPairing = new TradeSummaryPairing();

                                tradeSummaryPairing.TradeList = new Dictionary<String, TradeSummary>();
                                tradeSummaryPairing.LastIndex = 0;
                                tradeSummaryPairing.Updated = false;

                                TradeSummary tester = new TradeSummary();
                                if (tradeSummaryPairing.TradeList.TryGetValue(lookUpID, out tester) == false)
                                    tradeSummaryPairing.TradeList.Add(lookUpID, tradeSummary);

                                threeHourTradeSummaries_FOREX_LOOKUPX.Add(symbolID, tradeSummaryPairing);
                            }
                        }
                        break;

                    case "4hour":
                        {
                            TradeSummaryPairing tradeSummaryPairing = new TradeSummaryPairing();
                            if (fourHourTradeSummaries_FOREX_LOOKUPX.TryGetValue(symbolID, out tradeSummaryPairing))
                            {
                                tradeSummaryPairing.LastIndex = 0;

                                TradeSummary tester = new TradeSummary();
                                if (tradeSummaryPairing.TradeList.TryGetValue(lookUpID, out tester) == false)
                                    tradeSummaryPairing.TradeList.Add(lookUpID, tradeSummary);

                                tradeSummaryPairing.Updated = true;

                                fourHourTradeSummaries_FOREX_LOOKUPX[symbolID] = tradeSummaryPairing;
                            }
                            else
                            {
                                tradeSummaryPairing = new TradeSummaryPairing();

                                tradeSummaryPairing.TradeList = new Dictionary<String, TradeSummary>();
                                tradeSummaryPairing.LastIndex = 0;
                                tradeSummaryPairing.Updated = false;

                                TradeSummary tester = new TradeSummary();
                                if (tradeSummaryPairing.TradeList.TryGetValue(lookUpID, out tester) == false)
                                    tradeSummaryPairing.TradeList.Add(lookUpID, tradeSummary);

                                fourHourTradeSummaries_FOREX_LOOKUPX.Add(symbolID, tradeSummaryPairing);
                            }
                        }
                        break;

                    case "EndOfDay":
                        {
                            TradeSummaryPairing tradeSummaryPairing = new TradeSummaryPairing();
                            if (dailyTradeSummaries_FOREX_LOOKUPX.TryGetValue(symbolID, out tradeSummaryPairing))
                            {
                                tradeSummaryPairing.LastIndex = 0;

                                TradeSummary tester = new TradeSummary();
                                if (tradeSummaryPairing.TradeList.TryGetValue(lookUpID, out tester) == false)
                                    tradeSummaryPairing.TradeList.Add(lookUpID, tradeSummary);

                                tradeSummaryPairing.Updated = true;

                                dailyTradeSummaries_FOREX_LOOKUPX[symbolID] = tradeSummaryPairing;
                            }
                            else
                            {
                                tradeSummaryPairing = new TradeSummaryPairing();

                                tradeSummaryPairing.TradeList = new Dictionary<String, TradeSummary>();
                                tradeSummaryPairing.LastIndex = 0;
                                tradeSummaryPairing.Updated = false;

                                TradeSummary tester = new TradeSummary();
                                if (tradeSummaryPairing.TradeList.TryGetValue(lookUpID, out tester) == false)
                                    tradeSummaryPairing.TradeList.Add(lookUpID, tradeSummary);

                                dailyTradeSummaries_FOREX_LOOKUPX.Add(symbolID, tradeSummaryPairing);
                            }
                        }
                        break;
                }
            }
            catch (Exception exs)
            {
                Console.WriteLine("Error :: " + exs.ToString());
                Logger.log.Error("TradeSummary population failure :: " + exs.ToString());

            }
            //oneMinuteTradeSummaries_FOREX_LOOKUPX.Add(lookUpID, tradeSummary);


        }

        private void SetTradeSummaryHandler(List<TradeSummary> tradeSummary)
        {
            if (tradeSummary.Count > 0)
            {
                if (_synchronizationMode)
                {
                    if (tradeSummary.Count == 1 && tradeSummary.FirstOrDefault().Exchange == "SynchComplete") //Temp hack
                    {
                        if (_synchronizationCount == 1)
                        {
                            _synchronizationMode = false;
                            Console.WriteLine("Synchronization Complete!!");

                            Library.WriteErrorLog(DateTime.Now + "::" + "Synchronization Complete!! READY FOR QUERIES");
                        }
                        else
                        {
                            _synchronizationCount--;
                        }
                    }
                    else
                    {
                        if (_synchronizationMode)
                        {
                            Dictionary<String, TradeSummaryPairing> baseTradeSummaryCollection = null;

                            #region assignment
                            switch (tradeSummary.FirstOrDefault().TimeFrame)
                            {
                                case "1min":
                                    {
                                        baseTradeSummaryCollection = oneMinuteTradeSummaries_FOREX_LOOKUP_X;
                                    } break;

                                //case "2min":
                                //    {

                                //    } break;

                                //case "3min":
                                //    {

                                //    } break;

                                //case "4min":
                                //    {

                                //    } break;

                                case "5min":
                                    {
                                        baseTradeSummaryCollection = fiveMinuteTradeSummaries_FOREX_LOOKUP_X;
                                    } break;

                                case "10min":
                                    {
                                        baseTradeSummaryCollection = tenMinuteTradeSummaries_FOREX_LOOKUP_X;

                                    } break;

                                case "15min":
                                    {
                                        baseTradeSummaryCollection = fifteenMinuteTradeSummaries_FOREX_LOOKUPX;
                                    } break;

                                case "30min":
                                    {
                                        baseTradeSummaryCollection = thrityMinuteTradeSummaries_FOREX_LOOKUPX;
                                    } break;

                                case "1hour":
                                    {
                                        baseTradeSummaryCollection = oneHourTradeSummaries_FOREX_LOOKUPX;
                                    } break;

                                case "2hour":
                                    {
                                        baseTradeSummaryCollection = twoHourTradeSummaries_FOREX_LOOKUPX;
                                    } break;

                                case "3hour":
                                    {
                                        baseTradeSummaryCollection = threeHourTradeSummaries_FOREX_LOOKUPX;
                                    } break;

                                case "4hour":
                                    {
                                        baseTradeSummaryCollection = fourHourTradeSummaries_FOREX_LOOKUPX;
                                    } break;

                                case "EndOfDay":
                                    {
                                        baseTradeSummaryCollection = dailyTradeSummaries_FOREX_LOOKUPX;
                                    } break;
                            }
                            #endregion

                            Dictionary<String, TradeSummaryPairing> tradeSummariesSynchBackLog_FOREX_LOOKUP = new Dictionary<String, TradeSummaryPairing>();

                            TradeSummaryPairing tradeSummaryPairingTemp = new TradeSummaryPairing();
                            tradeSummaryPairingTemp.TradeList = new Dictionary<String, TradeSummary>();

                            TradeSummaryPairing tradeSummaryPairingCheck = new TradeSummaryPairing();
                            tradeSummaryPairingCheck.TradeList = new Dictionary<String, TradeSummary>();


                            var tradeSummaryList = tradeSummary;

                            if (baseTradeSummaryCollection.TryGetValue(tradeSummaryList.FirstOrDefault().SymbolID, out tradeSummaryPairingCheck))
                            {
                                foreach (var tradeItem in tradeSummaryList)
                                {
                                    String lookUpID = tradeItem.SymbolID + "#" + tradeItem.DateTime.Ticks.ToString();

                                    TradeSummary tester = new TradeSummary();
                                    if (tradeSummaryPairingTemp.TradeList.TryGetValue(lookUpID, out tester) == false)
                                        tradeSummaryPairingTemp.TradeList.Add(lookUpID, tradeItem);
                                }


                                //Sorting process
                                var tempListForSorting = tradeSummaryPairingCheck.TradeList.Values.ToList();
                                tempListForSorting.AddRange(tradeSummaryPairingTemp.TradeList.Values);

                                var sortedListByDateTimeAsc = tempListForSorting.OrderBy(c => c.DateTime);

                                if (sortedListByDateTimeAsc.Count() > 0 && sortedListByDateTimeAsc != null)
                                {
                                    TradeSummaryPairing sortedtradeSummaryPairingTemp = new TradeSummaryPairing();
                                    sortedtradeSummaryPairingTemp.TradeList = new Dictionary<string, TradeSummary>();

                                    foreach (var tradeItem in sortedListByDateTimeAsc)
                                    {
                                        String lookUpID = tradeItem.SymbolID + "#" + tradeItem.DateTime.Ticks.ToString();

                                        TradeSummary tester = new TradeSummary();
                                        if (sortedtradeSummaryPairingTemp.TradeList.TryGetValue(lookUpID, out tester) == false)
                                            sortedtradeSummaryPairingTemp.TradeList.Add(lookUpID, tradeItem);
                                    }
                                    tradeSummaryPairingCheck = sortedtradeSummaryPairingTemp;
                                }
                            }
                            else
                            {
                                //Adding for the first time
                                tradeSummaryPairingCheck.TradeList = new Dictionary<String, TradeSummary>();
                                foreach (var tradeItem in tradeSummaryList)
                                {
                                    String lookUpID = tradeItem.SymbolID + "#" + tradeItem.DateTime.Ticks.ToString();

                                    TradeSummary tester = new TradeSummary();
                                    if (tradeSummaryPairingCheck.TradeList.TryGetValue(lookUpID, out tester) == false)
                                        tradeSummaryPairingCheck.TradeList.Add(lookUpID, tradeItem);
                                }

                                baseTradeSummaryCollection.Add(tradeSummary.FirstOrDefault().SymbolID, tradeSummaryPairingCheck);
                            }



                            foreach (var item in tempMinuteTradeSummaries_FOREX_LOOKUP)
                            {
                                TradeSummaryPairing tradeSummaryPairing = new TradeSummaryPairing();

                                if (tradeSummariesSynchBackLog_FOREX_LOOKUP.TryGetValue(item.Key, out tradeSummaryPairing))
                                {
                                    //Update with the latest real time updates for when the synchronization process was busy
                                    foreach (var tradeSumItem in item.Value.TradeList.Values)
                                    {
                                        String lookUpID = tradeSumItem.SymbolID + "#" + tradeSumItem.DateTime.Ticks.ToString();

                                        TradeSummary tradeSum = new TradeSummary();
                                        if (tradeSummaryPairing.TradeList.TryGetValue(lookUpID, out tradeSum) == false)
                                        {
                                            if (tradeSummaryPairing.TradeList.Count() > 0)
                                            {
                                                if (tradeSumItem.DateTime > tradeSummaryPairing.TradeList.Last().Value.DateTime)
                                                    tradeSummaryPairing.TradeList.Add(lookUpID, tradeSumItem);
                                            }
                                            else
                                            {
                                                tradeSummaryPairing.TradeList.Add(lookUpID, tradeSumItem);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (tradeSummary.Count >= 1 && tradeSummary.FirstOrDefault().Exchange != "SynchComplete") //Temp hack
                    {
                        foreach (var item in tradeSummary)
                        {
                            SetTradeSummary(item);
                        }
                    }
                }
            }
        }

        private DateTime RoundToNearestTimeFrame(DateTime matchDate, string timeframe)
        {
            var dateMatcher = new DateTime();
            dateMatcher = matchDate;

            switch (timeframe)
            {
                case "5min":
                    {
                        dateMatcher = new DateTime(matchDate.Year,
                            matchDate.Month,
                            matchDate.Day,
                            matchDate.Hour,
                            ForcedRoundDownToNearestIntSpec(matchDate.Minute, 5),
                            0
                            );
                    } break;//?
                case "10min":
                    {
                        dateMatcher = new DateTime(matchDate.Year,
                            matchDate.Month,
                            matchDate.Day,
                            matchDate.Hour,
                            ForcedRoundDownToNearestIntSpec(matchDate.Minute, 10),
                            0
                            );
                    } break;

                case "15min":
                    {
                        dateMatcher = new DateTime(matchDate.Year,
                        matchDate.Month,
                        matchDate.Day,
                        matchDate.Hour,
                        ForcedRoundDownToNearestIntSpec(matchDate.Minute, 15),
                        0
                        );
                    } break;
                case "30min":
                    {
                        dateMatcher = new DateTime(matchDate.Year,
                            matchDate.Month,
                            matchDate.Day,
                            matchDate.Hour,
                            ForcedRoundDownToNearestIntSpec(matchDate.Minute, 30),
                            0
                            );
                    } break;
                case "1hour":
                    {
                        dateMatcher = new DateTime(matchDate.Year,
                            matchDate.Month,
                            matchDate.Day,
                            ForcedRoundDownToNearestIntSpec(matchDate.Hour, 1),
                            0,
                            0
                            );
                    } break;
                case "2hour":
                    {
                        dateMatcher = new DateTime(matchDate.Year,
                            matchDate.Month,
                            matchDate.Day,
                            ForcedRoundDownToNearestIntSpec(matchDate.Hour, 2),
                            0,
                            0
                            );
                    } break;
                case "4hour":
                    {
                        dateMatcher = new DateTime(matchDate.Year,
                            matchDate.Month,
                            matchDate.Day,
                            ForcedRoundDownToNearestIntSpec(matchDate.Hour, 4),
                            0,
                            0
                            );
                    } break;
                case "EndOfDay":
                    {
                        dateMatcher = new DateTime(matchDate.Year,
                            matchDate.Month,
                            matchDate.Day,
                            0,
                            0,
                            0
                            );
                    } break;
            }
            return dateMatcher;
        }

        public static int ForcedRoundDownToNearestIntSpec(int i, int nearestInt)
        {
            var changedToDouble = Convert.ToDouble(nearestInt);
            return ((int)Math.Ceiling(i / changedToDouble)) * nearestInt;
        }

        public void SetTradeSummary(TradeSummary tradeSummary)
        {
            //newly added
            tradeSummary.DateTime = RoundToNearestTimeFrame(tradeSummary.DateTime, tradeSummary.TimeFrame);

            //if (tradeSummary.SymbolID == "EURUSD" && tradeSummary.TimeFrame == "1min"
            //    || tradeSummary.SymbolID == "USDCAD" && tradeSummary.TimeFrame == "1min")
            //{
            //    string msg = tradeSummary.SymbolID + ", " + tradeSummary.TimeFrame + ", " + string.Format("{0}-{1}-{2} {3}:{4}",
            //                tradeSummary.DateTime.Year,
            //                tradeSummary.DateTime.Month,
            //                tradeSummary.DateTime.Day,
            //                tradeSummary.DateTime.Hour,
            //                tradeSummary.DateTime.Minute);

            //    var name = tradeSummary.SymbolID + tradeSummary.TimeFrame;

            //    Library.WriteInfoLog(name, msg);
            //}



            //if (DateTime.Now.DayOfWeek != DayOfWeek.Saturday && DateTime.Now.DayOfWeek != DayOfWeek.Sunday)
            {
                DateTime tempNewDateTime = new DateTime(tradeSummary.DateTime.Year, tradeSummary.DateTime.Month,
                                                tradeSummary.DateTime.Day, tradeSummary.DateTime.Hour,
                                                tradeSummary.DateTime.Minute, tradeSummary.DateTime.Second, 0);

                tradeSummary.DateTime = tempNewDateTime;

                String lookUpID = tradeSummary.SymbolID + "#" + tradeSummary.DateTime.Ticks.ToString();

                String log = tradeSummary.SymbolID + " :: " + tempNewDateTime.ToString() + " :: CurrentTime Log ->" + DateTime.Now.ToString();

                TradeSummaryPairing tradeSummaryPairing = new TradeSummaryPairing();

                Dictionary<String, TradeSummary> tradeSummaryList = null;

                Dictionary<String, TradeSummaryPairing> baseTradeSummaryCollection = _synchronizationMode ?
                    tempMinuteTradeSummaries_FOREX_LOOKUP : oneMinuteTradeSummaries_FOREX_LOOKUP_X;


                if (baseTradeSummaryCollection.TryGetValue(tradeSummary.SymbolID, out tradeSummaryPairing))
                {
                    TradeSummary tradeSum = new TradeSummary();
                    if (tradeSummaryPairing.TradeList.TryGetValue(lookUpID, out tradeSum) == false)
                    {
                        tradeSummaryPairing.TradeList.Add(lookUpID, tradeSummary);
                        if (_synchronizationMode == false) Console.WriteLine(log);
                    }
                }
                else
                {
                    tradeSummaryPairing.TradeList = new Dictionary<String, TradeSummary>();

                    TradeSummary tester = new TradeSummary();
                    if (tradeSummaryPairing.TradeList.TryGetValue(lookUpID, out tester) == false)
                        tradeSummaryPairing.TradeList.Add(lookUpID, tradeSummary);

                    if (_synchronizationMode == false) Console.WriteLine(log);

                    baseTradeSummaryCollection.Add(tradeSummary.SymbolID, tradeSummaryPairing);
                }


                if (_synchronizationMode == false)
                {
                    oneMinuteTradeSummaries_FOREX_LOOKUP_X = baseTradeSummaryCollection;
                    GenerateIntervals(tradeSummary.SymbolID);
                }
                else
                {
                    tempMinuteTradeSummaries_FOREX_LOOKUP = baseTradeSummaryCollection;
                }
            }
        }


        private void GenerateOtherTimeFrames()
        {

            if (oneMinuteTradeSummaries_FOREX_LOOKUP.Count() > 1)
            {

            }
        }

        public List<TradeSummary> GetFilteredRealTimeTradeSummaries(List<String> symbolList, String exchange, String timeframe, DateTime startDateTime, DateTime endDateTime)
        {
            List<TradeSummary> dtList = new List<TradeSummary>();

            TimeSpan timeSpan = new TimeSpan(0, 1, 0); //1 min

            long timeTicks = timeSpan.Ticks;

            switch (exchange)
            {
                case "LSE":
                    {
                    } break;

                case "NASDAQ":
                    {
                    } break;

                case "NYSE":
                    {
                    } break;

                case "AMEX":
                    {
                    } break;

                case "FOREX":
                case "Forex":
                    {
                        Dictionary<String, TradeSummaryPairing> lookUpTradeSummaries = new Dictionary<string, TradeSummaryPairing>();

                        switch (timeframe)
                        {
                            case "1min":
                                {
                                    lookUpTradeSummaries = oneMinuteTradeSummaries_FOREX_LOOKUP_X;
                                } break;

                            case "5min":
                                {
                                    lookUpTradeSummaries = fiveMinuteTradeSummaries_FOREX_LOOKUP_X;
                                } break;

                            case "15min":
                                {
                                    lookUpTradeSummaries = fifteenMinuteTradeSummaries_FOREX_LOOKUPX;
                                } break;

                            case "30min":
                                {
                                    lookUpTradeSummaries = thrityMinuteTradeSummaries_FOREX_LOOKUPX;
                                } break;

                            case "1hour":
                                {
                                    lookUpTradeSummaries = oneHourTradeSummaries_FOREX_LOOKUPX;
                                } break;

                            case "2hour":
                                {
                                    lookUpTradeSummaries = twoHourTradeSummaries_FOREX_LOOKUPX;
                                } break;

                            case "3hour":
                                {
                                    lookUpTradeSummaries = threeHourTradeSummaries_FOREX_LOOKUPX;

                                } break;
                        }

                        List<TradeSummary> tempTradeList = new List<TradeSummary>();

                        if (lookUpTradeSummaries != null)
                        {
                            String symbolData = symbolList.FirstOrDefault();

                            if (symbolData == "ALLCURRENCYPAIRS" || symbolData == "allcurrencypairs")
                            {
                                foreach (var dataItem in lookUpTradeSummaries)
                                {
                                    List<TradeSummary> tradeSummaryList = dataItem.Value.TradeList.Values.ToList();

                                    var limit = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["DATALIMIT"].ToString());

                                    int startIndex = tradeSummaryList.Count - limit;
                                    startIndex = limit == 0 ? 0 : startIndex;

                                    for (int j = startIndex; j < tradeSummaryList.Count; j++)
                                    {
                                        tempTradeList.Add(tradeSummaryList[j]);
                                    }
                                }
                            }
                            else
                            {
                                for (int i = 0; i < symbolList.Count; i++)
                                {
                                    TradeSummaryPairing resultDayTradeSum = new TradeSummaryPairing();

                                    if (lookUpTradeSummaries.TryGetValue(symbolList[i], out resultDayTradeSum))
                                    {
                                        List<TradeSummary> tradeSummaryList = resultDayTradeSum.TradeList.Values.ToList();

                                        var limit = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["DATALIMIT"].ToString());

                                        int startIndex = tradeSummaryList.Count - limit;
                                        startIndex = limit == 0 ? 0 : startIndex;

                                        for (int j = startIndex; j < tradeSummaryList.Count; j++)
                                        {
                                            tempTradeList.Add(tradeSummaryList[j]);
                                        }
                                    }
                                }
                            }
                        }
                        dtList = tempTradeList.Where(m => m.DateTime >= startDateTime && m.DateTime >= endDateTime)
                            .OrderBy(o => o.DateTime).ToList();

                    } break;


            }
            return dtList;
        }

        //public List<TradeSummary> GetRealTimeTradeSummaries(List<String> symbolList, List<int> dataPoints, bool bLast, String entity, String exchange, String timeFrame)
        //{
        //    List<TradeSummary> dtList = new List<TradeSummary>();

        //    TimeSpan timeSpan = new TimeSpan(0, 1, 0); //1 min

        //    long timeTicks = timeSpan.Ticks;

        //    switch (exchange)
        //    {
        //        case "LSE":
        //            {
        //            } break;

        //        case "NASDAQ":
        //            {
        //            } break;

        //        case "NYSE":
        //            {
        //            } break;

        //        case "AMEX":
        //            {
        //            } break;

        //        case "FOREX":
        //        case "Forex":
        //            {
        //                Dictionary<String, TradeSummaryPairing> lookUpTradeSummaries = new Dictionary<string, TradeSummaryPairing>();

        //                switch (timeFrame)
        //                {
        //                    case "1min":
        //                        {
        //                            lookUpTradeSummaries = oneMinuteTradeSummaries_FOREX_LOOKUP_X;
        //                        } break;

        //                    case "5min":
        //                        {
        //                            lookUpTradeSummaries = fiveMinuteTradeSummaries_FOREX_LOOKUP_X;
        //                        } break;

        //                    case "15min":
        //                        {
        //                            lookUpTradeSummaries = fifteenMinuteTradeSummaries_FOREX_LOOKUPX;
        //                        } break;

        //                    case "30min":
        //                        {
        //                            lookUpTradeSummaries = thrityMinuteTradeSummaries_FOREX_LOOKUPX;
        //                        } break;

        //                    case "1hour":
        //                        {
        //                            lookUpTradeSummaries = oneHourTradeSummaries_FOREX_LOOKUPX;
        //                        } break;

        //                    case "2hour":
        //                        {
        //                            lookUpTradeSummaries = twoHourTradeSummaries_FOREX_LOOKUPX;
        //                        } break;

        //                    case "3hour":
        //                        {
        //                            lookUpTradeSummaries = threeHourTradeSummaries_FOREX_LOOKUPX;

        //                        } break;
        //                }

        //                if (lookUpTradeSummaries != null)
        //                {
        //                    String symbolData = symbolList.FirstOrDefault();

        //                    if (symbolData == "ALLCURRENCYPAIRS" || symbolData == "allcurrencypairs")
        //                    {
        //                        foreach (var dataItem in lookUpTradeSummaries)
        //                        {
        //                            List<TradeSummary> tradeSummaryList = dataItem.Value.TradeList.Values.ToList();

        //                            int start = tradeSummaryList.Count - dataPoints.FirstOrDefault();
        //                            if (start > 0)
        //                            {
        //                                var limit = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["DATALIMIT"].ToString());

        //                                int startIndex = tradeSummaryList.Count - limit;
        //                                startIndex = limit == 0 ? 0 : startIndex;

        //                                for (int j = startIndex; j < tradeSummaryList.Count; j++)
        //                                {
        //                                    dtList.Add(tradeSummaryList[j]);
        //                                }
        //                            }
        //                        }
        //                    }
        //                    else
        //                    {

        //                        for (int i = 0; i < symbolList.Count; i++)
        //                        {
        //                            TradeSummaryPairing resultDayTradeSum = new TradeSummaryPairing();

        //                            if (lookUpTradeSummaries.TryGetValue(symbolList[i], out resultDayTradeSum))
        //                            {
        //                                List<TradeSummary> tradeSummaryList = resultDayTradeSum.TradeList.Values.ToList();

        //                                int start = tradeSummaryList.Count - dataPoints.FirstOrDefault();

        //                                if (start > 0)
        //                                {
        //                                    var limit = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["DATALIMIT"].ToString());

        //                                    int startIndex = tradeSummaryList.Count - limit;
        //                                    startIndex = limit == 0 ? 0 : startIndex;

        //                                    for (int j = startIndex; j < tradeSummaryList.Count; j++)
        //                                    {
        //                                        dtList.Add(tradeSummaryList[j]);
        //                                    }
        //                                }
        //                            }
        //                        }
        //                    }
        //                }

        //            } break;
        //    }
        //    return dtList;

        //}

        public List<TradeSummary> GetRealTimeTradeSummaries(List<String> symbolList, List<int> dataPoints, bool bLast, String entity, String exchange, String timeFrame)
        {
            Dictionary<String, TradeSummaryPairing> lookUpTradeSummaries = new Dictionary<string, TradeSummaryPairing>();

            List<TradeSummary> dtList = new List<TradeSummary>();

            TimeSpan timeSpan = new TimeSpan(0, 1, 0); //1 min

            long timeTicks = timeSpan.Ticks;


            switch (timeFrame)
            {
                case "1min":
                    {
                        if (exchange == "NYMEX") lookUpTradeSummaries = oneMinuteTradeSummaries_COMMODITIES_LOOKUP;
                        else if (exchange == "Forex" || exchange == "FOREX") lookUpTradeSummaries = oneMinuteTradeSummaries_FOREX_LOOKUP_X;
                        else lookUpTradeSummaries = oneMinuteTradeSummaries_INDICES_LOOKUP;                   

                    } break;

                case "5min":
                    {
                        if (exchange == "NYMEX") lookUpTradeSummaries = fiveMinuteTradeSummaries_COMMODITIES_LOOKUP;
                        else if (exchange == "Forex" || exchange == "FOREX") lookUpTradeSummaries = fiveMinuteTradeSummaries_FOREX_LOOKUP_X;
                        else lookUpTradeSummaries = fiveMinuteTradeSummaries_INDICES_LOOKUP;

                    } break;

                case "10min":
                    {
                        if (exchange == "NYMEX") lookUpTradeSummaries = tenMinuteTradeSummaries_COMMODITIES_LOOKUP;
                        else if (exchange == "Forex" || exchange == "FOREX") lookUpTradeSummaries = tenMinuteTradeSummaries_FOREX_LOOKUP_X;
                        else lookUpTradeSummaries = tenMinuteTradeSummaries_INDICES_LOOKUP;

                    } break;

                case "15min":
                    {
                        if (exchange == "NYMEX") lookUpTradeSummaries = fifteenMinuteTradeSummaries_INDICES_LOOKUP;
                        else if (exchange == "Forex" || exchange == "FOREX") lookUpTradeSummaries = fifteenMinuteTradeSummaries_FOREX_LOOKUPX;
                        else lookUpTradeSummaries = fifteenMinuteTradeSummaries_INDICES_LOOKUP;

                    } break;

                case "30min":
                    {
                        if (exchange == "NYMEX") lookUpTradeSummaries = thrityMinuteTradeSummaries_COMMODITIES_LOOKUP;
                        else if (exchange == "Forex" || exchange == "FOREX") lookUpTradeSummaries = thrityMinuteTradeSummaries_FOREX_LOOKUPX;
                        else lookUpTradeSummaries = thrityMinuteTradeSummaries_INDICES_LOOKUP;

                    } break;

                case "1hour":
                    {
                        if (exchange == "NYMEX") lookUpTradeSummaries = oneHourTradeSummaries_COMMODITIES_LOOKUP;
                        else if (exchange == "Forex" || exchange == "FOREX") lookUpTradeSummaries = oneHourTradeSummaries_FOREX_LOOKUPX;
                        else lookUpTradeSummaries = oneHourTradeSummaries_INDICES_LOOKUP;

                    } break;

                case "2hour":
                    {
                        if (exchange == "NYMEX") lookUpTradeSummaries = twoHourTradeSummaries_COMMODITIES_LOOKUP;
                        else if (exchange == "Forex" || exchange == "FOREX") lookUpTradeSummaries = twoHourTradeSummaries_FOREX_LOOKUPX;
                        else lookUpTradeSummaries = twoHourTradeSummaries_INDICES_LOOKUP;

                    } break;

                case "3hour":
                    {
                        if (exchange == "NYMEX") lookUpTradeSummaries = threeHourTradeSummaries_COMMODITIES_LOOKUP;
                        else if (exchange == "Forex" || exchange == "FOREX") lookUpTradeSummaries = threeHourTradeSummaries_FOREX_LOOKUPX;
                        else lookUpTradeSummaries = threeHourTradeSummaries_INDICES_LOOKUP;

                    } break;

                case "4hour":
                    {
                        if (exchange == "NYMEX") lookUpTradeSummaries = fourHourTradeSummaries_COMMODITIES_LOOKUP;
                        else if (exchange == "Forex" || exchange == "FOREX") lookUpTradeSummaries = fourHourTradeSummaries_FOREX_LOOKUPX;
                        else lookUpTradeSummaries = fourHourTradeSummaries_INDICES_LOOKUP;

                    } break;

                case "EndOfDay":
                    {
                        if (exchange == "NYMEX") lookUpTradeSummaries = dayTradeSummaries_COMMODITIES_LOOKUP;
                        else if (exchange == "Forex" || exchange == "FOREX") lookUpTradeSummaries = dailyTradeSummaries_FOREX_LOOKUPX;
                        else lookUpTradeSummaries = dayTradeSummaries_INDICES_LOOKUP;
                    } break;
            }

            if (lookUpTradeSummaries != null)
            {
                String symbolData = symbolList.FirstOrDefault();

                if (symbolData == "ALLCURRENCYPAIRS" || symbolData == "allcurrencypairs")
                {
                    foreach (var dataItem in lookUpTradeSummaries)
                    {
                        List<TradeSummary> tradeSummaryList = dataItem.Value.TradeList.Values.ToList();

                        int start = tradeSummaryList.Count - dataPoints.FirstOrDefault();
                        if (start > 0)
                        {
                            var limit = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["DATALIMIT"].ToString());

                            int startIndex = tradeSummaryList.Count - limit;
                            startIndex = limit == 0 ? 0 : startIndex;

                            for (int j = startIndex; j < tradeSummaryList.Count; j++)
                            {
                                dtList.Add(tradeSummaryList[j]);
                            }
                        }
                    }
                }
                else
                {

                    for (int i = 0; i < symbolList.Count; i++)
                    {
                        TradeSummaryPairing resultDayTradeSum = new TradeSummaryPairing();

                        if (lookUpTradeSummaries.TryGetValue(symbolList[i], out resultDayTradeSum))
                        {
                            List<TradeSummary> tradeSummaryList = resultDayTradeSum.TradeList.Values.ToList();

                            int start = tradeSummaryList.Count - dataPoints.FirstOrDefault();

                            if (start > 0)
                            {
                                var limit = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["DATALIMIT"].ToString());

                                int startIndex = tradeSummaryList.Count - limit;
                                startIndex = limit == 0 ? 0 : startIndex;

                                for (int j = startIndex; j < tradeSummaryList.Count; j++)
                                {
                                    dtList.Add(tradeSummaryList[j]);
                                }
                            }
                        }
                    }
                }
            }
            return dtList;
        }


        public List<TradeSummary> GetTradeSummariesOld(List<String> symbolList, DateTime startDate, DateTime endDate, String exchange, String timeFrame)
        {
            List<TradeSummary> dtList = new List<TradeSummary>();

            TimeSpan timeSpan = new TimeSpan(0, 1, 0); //1 min

            long timeTicks = timeSpan.Ticks;

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

                                tempCurrent = tempCurrent + timeTicks;
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

                                tempCurrent = tempCurrent + timeTicks;
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

                                tempCurrent = tempCurrent + timeTicks;
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

                                tempCurrent = tempCurrent + timeTicks;
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
                        List<String> lookUpIDList = new List<String>();

                        for (int f = 0; f < symbolList.Count; f++)
                        {
                            var temp = symbolList[f] + "#";

                            long tempCurrent = startDate.Ticks;
                            while (tempCurrent <= endDate.Ticks)
                            {
                                String lookUpIDTemp = temp + tempCurrent.ToString();
                                lookUpIDList.Add(lookUpIDTemp);

                                tempCurrent = tempCurrent + timeTicks;
                            }
                        }


                        for (int i = 0; i < lookUpIDList.Count; i++)
                        {
                            TradeSummary resultDayTradeSum = new TradeSummary();

                            bool bFound = oneMinuteTradeSummaries_FOREX_LOOKUPX.TryGetValue(lookUpIDList[i], out resultDayTradeSum);
                            if (bFound)
                            {
                                dtList.Add(resultDayTradeSum);
                            }
                        }

                    } break;
            }
            return dtList;
        }

        public DateTime GetFirstDateTime(String symbolID, String exchange, String timeFrame)
        {
            DateTime startDateTime = new DateTime();

            List<TradeSummary> dtList = new List<TradeSummary>();

            TimeSpan timeSpan = new TimeSpan(0, 1, 0); //1 min

            long timeTicks = timeSpan.Ticks;

            int intervalCount = TimeFrameStrToNum(timeFrame);

            List<String> lookUpIDList = new List<String>();


            switch (exchange)
            {
                case "LSE":
                    {
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
                        Dictionary<String, TradeSummaryPairing> lookUpTradeSummaries = new Dictionary<string, TradeSummaryPairing>();

                        switch (intervalCount)
                        {
                            case 1:
                                {
                                    lookUpTradeSummaries = oneMinuteTradeSummaries_FOREX_LOOKUP_X;

                                } break;

                            case 5:
                                {
                                    lookUpTradeSummaries = fiveMinuteTradeSummaries_FOREX_LOOKUP_X;

                                } break;

                            case 15:
                                {
                                    lookUpTradeSummaries = fifteenMinuteTradeSummaries_FOREX_LOOKUPX;

                                } break;

                            case 30:
                                {
                                    lookUpTradeSummaries = thrityMinuteTradeSummaries_FOREX_LOOKUPX;
                                } break;

                            case 60:
                                {
                                    lookUpTradeSummaries = oneHourTradeSummaries_FOREX_LOOKUPX;
                                } break;

                            case 120:
                                {
                                    lookUpTradeSummaries = twoHourTradeSummaries_FOREX_LOOKUPX;
                                } break;

                            case 180:
                                {
                                    lookUpTradeSummaries = threeHourTradeSummaries_FOREX_LOOKUPX;

                                } break;
                        }



                        TradeSummaryPairing resultDayTradeSum = new TradeSummaryPairing();

                        bool bFound = lookUpTradeSummaries.TryGetValue(symbolID, out resultDayTradeSum);
                        if (bFound)
                        {
                            TradeSummary resultTrade = resultDayTradeSum.TradeList.FirstOrDefault().Value;
                            startDateTime = resultTrade.DateTime;
                        }

                    } break;
            }
            return startDateTime;
        }


        public List<TradeSummary> GetTradeSummaries(List<String> symbolList, DateTime startDate, DateTime endDate, String exchange, String timeFrame)
        {
            List<TradeSummary> dtList = new List<TradeSummary>();

            int intervalCount = TimeFrameStrToNum(timeFrame);

            TimeSpan timeSpan = new TimeSpan(0, 1, 0); //1 min

            switch (intervalCount)
            {
                case 15:
                    {
                        timeSpan = new TimeSpan(0, 15, 0);
                    } break;

                case 30:
                    {
                        timeSpan = new TimeSpan(0, 30, 0);
                    } break;

                case 60:
                    {
                        timeSpan = new TimeSpan(0, 60, 0);
                    } break;

                case 120:
                    {
                        timeSpan = new TimeSpan(0, 120, 0);

                    } break;

                case 180:
                    {
                        timeSpan = new TimeSpan(0, 180, 0);

                    } break;
            }


            long timeTicks = timeSpan.Ticks;

            List<String> lookUpIDList = new List<String>();

            for (int f = 0; f < symbolList.Count; f++)
            {
                var temp = symbolList[f] + "#";

                long tempCurrent = startDate.Ticks;
                while (tempCurrent <= endDate.Ticks)
                {
                    String lookUpIDTemp = temp + tempCurrent.ToString();
                    lookUpIDList.Add(lookUpIDTemp);

                    tempCurrent = tempCurrent + timeTicks;
                }
            }


            switch (exchange)
            {
                case "LSE":
                    {
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
                        Dictionary<String, TradeSummaryPairing> lookUpTradeSummaries = new Dictionary<string, TradeSummaryPairing>();

                        switch (intervalCount)
                        {
                            case 1:
                                {
                                    lookUpTradeSummaries = oneMinuteTradeSummaries_FOREX_LOOKUP_X;

                                } break;

                            case 10:
                                {
                                    lookUpTradeSummaries = tenMinuteTradeSummaries_FOREX_LOOKUP_X;

                                } break;

                            case 15:
                                {
                                    lookUpTradeSummaries = fifteenMinuteTradeSummaries_FOREX_LOOKUPX;

                                } break;

                            case 30:
                                {
                                    lookUpTradeSummaries = thrityMinuteTradeSummaries_FOREX_LOOKUPX;
                                } break;

                            case 60:
                                {
                                    lookUpTradeSummaries = oneHourTradeSummaries_FOREX_LOOKUPX;
                                } break;

                            case 120:
                                {
                                    lookUpTradeSummaries = twoHourTradeSummaries_FOREX_LOOKUPX;
                                } break;

                            case 180:
                                {
                                    lookUpTradeSummaries = threeHourTradeSummaries_FOREX_LOOKUPX;

                                } break;

                            case 240:
                                {
                                    lookUpTradeSummaries = fourHourTradeSummaries_FOREX_LOOKUPX;

                                } break;
                        }


                        for (int i = 0; i < symbolList.Count; i++)
                        {
                            TradeSummaryPairing resultDayTradeSum = new TradeSummaryPairing();

                            bool bFound = lookUpTradeSummaries.TryGetValue(symbolList[i], out resultDayTradeSum);
                            if (bFound)
                            {
                                //for (int j = 0; j < lookUpIDList.Count; j++)
                                //{
                                //    TradeSummary resultTrade = new TradeSummary();

                                //    bool bFoundTradeData = resultDayTradeSum.TradeList.TryGetValue(lookUpIDList[i], out resultTrade);
                                //    if (bFoundTradeData)
                                //    {
                                //        dtList.Add(resultTrade);
                                //    }
                                // }

                                //List<String> quickLookUpIDList = new List<String>();

                                //for (int f = 0; f < symbolList.Count; f++)
                                //{
                                //    long tempCurrent = startDate.Ticks;
                                //    var temp = symbolList[f] + "#";
                                //    String lookUpIDTemp = temp + tempCurrent.ToString();
                                //    quickLookUpIDList.Add(lookUpIDTemp);
                                //}

                                int m = 0;
                                while (true)
                                {
                                    if (m < resultDayTradeSum.TradeList.Count())
                                    {
                                        var keyValue = resultDayTradeSum.TradeList.ElementAt(m);
                                        String keyItem = keyValue.Key;
                                        var tickArray = keyItem.Split('#');

                                        long dataTick = Convert.ToInt64(tickArray.LastOrDefault());

                                        if (startDate.Ticks <= dataTick && dataTick <= endDate.Ticks)
                                        {
                                            dtList.Add(keyValue.Value);
                                        }
                                        //if (dataTick > endDate.Ticks) break;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                    m++;
                                }


                                //TradeSummary resultTrade = new TradeSummary();

                                //bool bFoundTradeData = resultDayTradeSum.TradeList.TryGetValue(lookUpIDList[i], out resultTrade);
                                //if (bFoundTradeData)
                                //{
                                //    dtList.Add(resultTrade);
                                //}



                            }
                        }

                    } break;
            }
            return dtList;
        }

        private int TimeFrameStrToNum(String timeFrame)
        {
            int timeFrameInt = 0;

            switch (timeFrame)
            {
                case "1min":
                    {
                        timeFrameInt = 1;
                    } break;

                case "10min":
                    {
                        timeFrameInt = 10;
                    } break;

                case "15min":
                    {
                        timeFrameInt = 15;
                    } break;

                case "30min":
                    {
                        timeFrameInt = 30;
                    } break;

                case "1hour":
                    {
                        timeFrameInt = 60;
                    } break;

                case "2hour":
                    {
                        timeFrameInt = 120;
                    } break;

                case "3hour":
                    {
                        timeFrameInt = 180;
                    } break;

                case "4hour":
                    {
                        timeFrameInt = 240;
                    } break;

                case "EndOfDay":
                    {
                        timeFrameInt = 1440;
                    } break;
            }
            return timeFrameInt;
        }

        private void ApplyScheduler()
        {
            SystemEvents.TimeChanged += new EventHandler(SystemEvents_TimeChanged);

            actionTimer = new System.Timers.Timer();

            actionTimer.Elapsed += new ElapsedEventHandler(Task);

            actionTimer.Interval = CalculateInterval();
            actionTimer.AutoReset = true;

            actionTimer.Start();
        }

        public void SystemEvents_TimeChanged(object sender, EventArgs e)
        {
            actionTimer.Stop();
            actionTimer.Enabled = false;

            actionTimer.Interval = CalculateInterval();

            actionTimer.Enabled = true;
            actionTimer.Start();
        }


        private double CalculateInterval()
        {
            var timeArray = System.Configuration.ConfigurationManager.AppSettings["TIMELIST"].Split('@');
            var triggerTime = timeArray.LastOrDefault();

            int hours = Convert.ToInt32(triggerTime.Substring(0, 2));
            int minutes = Convert.ToInt32(triggerTime.Substring(3, 2));

            DateTime dayDueTime = DateTime.Now;

            switch (timeArray.FirstOrDefault())
            {
                case "TWOWEEKLY": { dayDueTime = DateTime.Now.AddDays(2); } break;
                case "WEEKLY": { dayDueTime = DateTime.Now.AddDays(7); } break;
                case "DAILY": { dayDueTime = DateTime.Now; } break;
            }

            DateTime testDateTime = new DateTime(dayDueTime.Year, dayDueTime.Month, dayDueTime.Day,
                hours, minutes, 0);

            if (DateTime.Now > testDateTime)
            {
                switch (timeArray.FirstOrDefault())
                {
                    case "TWOWEEKLY": { dayDueTime = DateTime.Now.AddDays(2); } break;
                    case "WEEKLY": { dayDueTime = DateTime.Now.AddDays(7); } break;
                    case "DAILY": { dayDueTime = DateTime.Now.AddDays(1); } break;
                }
            }

            DateTime criteriaDateTime = new DateTime(dayDueTime.Year, dayDueTime.Month, dayDueTime.Day,
                hours, minutes, 23);
            //we want the clean up to happen well clear of when the lookups are updating which tends to be 
            //on the minute therefore random value of 23 seconds is used

            long ticks = criteriaDateTime.Ticks - DateTime.Now.Ticks;

            TimeSpan tms = new TimeSpan(ticks);

            return tms.TotalMilliseconds;
        }

        private void Task(object sender, ElapsedEventArgs e)
        {
            actionTimer.Stop();
            actionTimer.Enabled = false;


            MemoryManagement();

            //Tell DBM to start import
            actionTimer.Interval = CalculateInterval();
            actionTimer.Enabled = true;
            actionTimer.Start();
        }

        private void MemoryManagement()
        {
            string[] exchanges = new string[] { "FOREX", "NYMEX", "INDEX" };

            foreach (var exchange in exchanges)
            {
                var symbolList = _realTimeDataHandler.GetSymbolList(exchange);

                foreach (var item in symbolList)
                {
                    foreach (var interval in _intervalCount)
                    {
                        int removedCounter = 0;
                        int countLimit = 1000;

                        var lookUpTradeSummaries = new Dictionary<string, TradeSummaryPairing>();
                        switch (interval)
                        {
                            case 1:
                                {
                                    if (exchange == "NYMEX") lookUpTradeSummaries = oneMinuteTradeSummaries_COMMODITIES_LOOKUP;
                                    else if (exchange == "Forex" || exchange == "FOREX") lookUpTradeSummaries = oneMinuteTradeSummaries_FOREX_LOOKUP_X;
                                    else lookUpTradeSummaries = oneMinuteTradeSummaries_INDICES_LOOKUP;

                                    countLimit = 200000;//SHOULD BE MORE

                                } break;

                            case 5:
                                {
                                    if (exchange == "NYMEX") lookUpTradeSummaries = fiveMinuteTradeSummaries_COMMODITIES_LOOKUP;
                                    else if (exchange == "Forex" || exchange == "FOREX") lookUpTradeSummaries = fiveMinuteTradeSummaries_FOREX_LOOKUP_X;
                                    else lookUpTradeSummaries = fiveMinuteTradeSummaries_INDICES_LOOKUP;

                                    countLimit = 200000;

                                } break;

                            case 10:
                                {
                                    if (exchange == "NYMEX") lookUpTradeSummaries = tenMinuteTradeSummaries_COMMODITIES_LOOKUP;
                                    else if (exchange == "Forex" || exchange == "FOREX") lookUpTradeSummaries = tenMinuteTradeSummaries_FOREX_LOOKUP_X;
                                    else lookUpTradeSummaries = tenMinuteTradeSummaries_INDICES_LOOKUP;

                                    countLimit = 200000;

                                } break;

                            case 15:
                                {
                                    if (exchange == "NYMEX") lookUpTradeSummaries = fifteenMinuteTradeSummaries_INDICES_LOOKUP;
                                    else if (exchange == "Forex" || exchange == "FOREX") lookUpTradeSummaries = fifteenMinuteTradeSummaries_FOREX_LOOKUPX;
                                    else lookUpTradeSummaries = fifteenMinuteTradeSummaries_INDICES_LOOKUP;

                                    countLimit = 200000;

                                } break;

                            case 30:
                                {
                                    if (exchange == "NYMEX") lookUpTradeSummaries = thrityMinuteTradeSummaries_COMMODITIES_LOOKUP;
                                    else if (exchange == "Forex" || exchange == "FOREX") lookUpTradeSummaries = thrityMinuteTradeSummaries_FOREX_LOOKUPX;
                                    else lookUpTradeSummaries = thrityMinuteTradeSummaries_INDICES_LOOKUP;

                                    countLimit = 200000;

                                } break;

                            case 60:
                                {
                                    if (exchange == "NYMEX") lookUpTradeSummaries = oneHourTradeSummaries_COMMODITIES_LOOKUP;
                                    else if (exchange == "Forex" || exchange == "FOREX") lookUpTradeSummaries = oneHourTradeSummaries_FOREX_LOOKUPX;
                                    else lookUpTradeSummaries = oneHourTradeSummaries_INDICES_LOOKUP;

                                    countLimit = 20000;

                                } break;

                            case 120:
                                {
                                    if (exchange == "NYMEX") lookUpTradeSummaries = twoHourTradeSummaries_COMMODITIES_LOOKUP;
                                    else if (exchange == "Forex" || exchange == "FOREX") lookUpTradeSummaries = twoHourTradeSummaries_FOREX_LOOKUPX;
                                    else lookUpTradeSummaries = twoHourTradeSummaries_INDICES_LOOKUP;

                                    countLimit = 20000;

                                } break;

                            case 180:
                                {
                                    if (exchange == "NYMEX") lookUpTradeSummaries = threeHourTradeSummaries_COMMODITIES_LOOKUP;
                                    else if (exchange == "Forex" || exchange == "FOREX") lookUpTradeSummaries = threeHourTradeSummaries_FOREX_LOOKUPX;
                                    else lookUpTradeSummaries = threeHourTradeSummaries_INDICES_LOOKUP;

                                    countLimit = 20000;

                                } break;

                            case 240:
                                {
                                    if (exchange == "NYMEX") lookUpTradeSummaries = fourHourTradeSummaries_COMMODITIES_LOOKUP;
                                    else if (exchange == "Forex" || exchange == "FOREX") lookUpTradeSummaries = fourHourTradeSummaries_FOREX_LOOKUPX;
                                    else lookUpTradeSummaries = fourHourTradeSummaries_INDICES_LOOKUP;

                                    countLimit = 20000;


                                } break;

                            case 1440:
                                {
                                    if (exchange == "NYMEX") lookUpTradeSummaries = dayTradeSummaries_COMMODITIES_LOOKUP;
                                    else if (exchange == "Forex" || exchange == "FOREX") lookUpTradeSummaries = dailyTradeSummaries_FOREX_LOOKUPX;
                                    else lookUpTradeSummaries = dayTradeSummaries_INDICES_LOOKUP;

                                    countLimit = 60;

                                } break;
                        }

                        var tradePairing = new TradeSummaryPairing();

                        if (lookUpTradeSummaries.TryGetValue(item, out tradePairing))
                        {
                            int count = lookUpTradeSummaries[item].TradeList.ToList().Count;

                            foreach (var s in lookUpTradeSummaries[item].TradeList.ToList())
                            {
                                if (count >= countLimit && removedCounter <= countLimit)
                                {
                                    lookUpTradeSummaries[item].TradeList.Remove(s.Key);
                                    removedCounter++;
                                }
                            }
                        }


                    }
                }
            }
        }
    }
}



//int interval = 0;
//               switch (tradeSummary.FirstOrDefault().TimeFrame)
//               {
//                   case "1min":
//                       {
//                           interval = 60;
//                       } break;

//                   case "2min":
//                       {
//                           interval = 120;

//                       } break;

//                   case "3min":
//                       {
//                           interval = 180;
//                       } break;

//                   case "4min":
//                       {
//                           interval = 240;
//                       } break;

//                   case "5min":
//                       {
//                           interval = 300;
//                       } break;

//                   case "6min":
//                       {
//                           interval = 360;
//                       } break;

//                   case "10min":
//                       {
//                           interval = 360;
//                       } break;

//                   case "15min":
//                       {
//                           interval = 900;
//                       } break;

//                   case "30min":
//                       {
//                           interval = 1800;
//                       } break;

//                   case "1hour":
//                       {
//                           interval = 3600;
//                       } break;

//                   case "2hour":
//                       {
//                           interval = 7200;
//                       } break;

//                   case "3hour":
//                       {
//                           interval = 10800;
//                       } break;
//               }