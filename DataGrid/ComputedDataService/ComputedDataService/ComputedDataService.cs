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

namespace ComputedDataService
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


    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class ComputedDataService : IComputedDataService
    {
        //exchange / data
        //public static Dictionary<String, List<TradeSummary>> dayTradeSummaries_LSE = new Dictionary<String, List<TradeSummary>>();
        //public static Dictionary<String, List<TradeSummary>> dayTradeSummaries_NYSE = new Dictionary<String, List<TradeSummary>>();
        //public static Dictionary<String, List<TradeSummary>> dayTradeSummaries_AMEX = new Dictionary<String, List<TradeSummary>>();
        //public static Dictionary<String, List<TradeSummary>> dayTradeSummaries_NASDAQ = new Dictionary<String, List<TradeSummary>>();


        //public static Dictionary<String, TradeSummary> dayTradeSummaries_AMEX_LOOKUP = new Dictionary<String, TradeSummary>();
        //public static Dictionary<String, TradeSummary> dayTradeSummaries_NYSE_LOOKUP = new Dictionary<String, TradeSummary>();
        //public static Dictionary<String, TradeSummary> dayTradeSummaries_NASDAQ_LOOKUP = new Dictionary<String, TradeSummary>();

        public static Dictionary<String, tblCorrelation> StockExchangeCorrelations_LOOKUP = new Dictionary<String, tblCorrelation>();
        public static Dictionary<String, tblCorrelation> ForexCorrelations_LOOKUP = new Dictionary<String, tblCorrelation>();

        public static MODE Mode;

        private static DataModel _dataModel = null;

        public ComputedDataService()
        {
            //Temp
            Mode = MODE.Test;

            ThreadStart threadStart = new ThreadStart(InitialseDataTables);
            Thread initialseDataTablesThread = new Thread(threadStart);
            initialseDataTablesThread.Start();

            _dataModel = new DataModel();


            var temp = GetPerformanceStatistics("Head and Shoulders", "ChartPattern", "EndOfDay", "Forex");

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

            InitializeCorrelationsData();


            //end time
            TimeSpan span = DateTime.Now - startTime;

            Console.WriteLine(String.Format("EndTime {0} {1} Total :: {2}", DateTime.Now.TimeOfDay, DateTime.Now, span));

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(triggerlogFileName, true))
            {
                file.WriteLine(String.Format("EndTime  {0}  {1} Total Duration::{2}", DateTime.Now.TimeOfDay, DateTime.Now, span));
                file.WriteLine("");
            }
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
                //port = 11022;
                port = 11252;
            }
            else
            {
                //Live will always been on different machines
                port = 11000;
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
                }
            }
        }

        private void InitializeCorrelationsData()
        {
            String currentTitle = Console.Title;

            try
            {
                foreach (var item in Exchanges.List)
                {
                    GC.Collect();

                    String exchange = item;

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

                    using (SqlConnection c = new SqlConnection(settings.ToString()))
                    {
                        c.Open();

                        String query = "SELECT * FROM dbo.tblHistoricalCorrelations";
                        if (item == "Forex") query = "SELECT * FROM dbo.tblHistoricalCorrelations";

                        SqlCommand sqlCommand = new SqlCommand(query, c);

                        sqlCommand.CommandTimeout = 0;
                        SqlDataReader reader = sqlCommand.ExecuteReader();

                        String previousSymbolID = "";

                        List<tblCorrelation> dayTradeList = new List<tblCorrelation>(); ;

                        int count = 0;

                        while (reader.Read())
                        {
                            tblCorrelation correlationResult = new tblCorrelation();

                            String symbolID = reader["SymbolID"].ToString().Trim();

                            correlationResult.SymbolID = symbolID;
                            correlationResult.CorrelatingEntityID = reader["CorrelatingEntityID"].ToString().Trim();

                            correlationResult.StartDateTime = Convert.ToDateTime(reader["StartDate"].ToString());
                            correlationResult.EndDateTime = Convert.ToDateTime(reader["EndDate"].ToString());

                            if (item == "Forex")
                            {
                                correlationResult.SourceExchange = "Forex";
                                correlationResult.DestinationExchange = "Forex";
                            }
                            else
                            {
                                correlationResult.SourceExchange = reader["SourceStockExchange"].ToString().Trim();
                                correlationResult.DestinationExchange = reader["DestinationStockExchange"].ToString().Trim();
                            }

                            correlationResult.TimeFrame = reader["TimeFrame"].ToString().Trim();
                            correlationResult.Distance = Convert.ToDouble(reader["Distance"].ToString());
                            correlationResult.CorrelationRatio = Convert.ToDouble(reader["CorrelationRatio"].ToString());
                            correlationResult.Event = reader["Event"].ToString().Trim();

                            correlationResult.HC_ID = reader["HC_ID"].ToString().Trim();
                            correlationResult.ResultantSymbolID = reader["ResultantSymbolID"].ToString().Trim();

                            previousSymbolID = symbolID;

                            // String lookUpID = symbolID + "#" + correlationResult.CorrelatingEntityID + "#" + correlationResult.Event + "#" + correlationResult.TimeFrame;
                            String lookUpID = reader["EntryID"].ToString();

                            switch (exchange)
                            {
                                case "NASDAQ":
                                case "LSE":
                                case "NYSE":
                                case "AMEX":
                                    {
                                        StockExchangeCorrelations_LOOKUP.Add(lookUpID, correlationResult);
                                    } break;

                                case "Forex":
                                    {
                                        ForexCorrelations_LOOKUP.Add(lookUpID, correlationResult);
                                    } break;
                            }
                            count++;
                        }
                    }


                    switch (exchange)
                    {
                        case "NASDAQ":
                        case "LSE":
                        case "NYSE":
                        case "AMEX":
                            {
                                Console.WriteLine("StockExchange Correlations Data:: " + StockExchangeCorrelations_LOOKUP.Count());
                            } break;

                        case "Forex":
                            {
                                Console.WriteLine("Forex Correlations Data:: " + ForexCorrelations_LOOKUP.Count());
                            } break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error :: " + ex.ToString());
            }
        }

        public List<tblCorrelation> GetCorrelations(List<String> symbolList, String entity, String exchange, String timeFrame)
        {
            List<tblCorrelation> dtList = new List<tblCorrelation>();
            long dayTicks = 864000000000;

            switch (exchange)
            {
                case "LSE":
                case "NASDAQ":
                case "NYSE":
                case "AMEX":
                    {
                        //List<String> lookUpIDList = new List<String>();

                        //for (int f = 0; f < symbolList.Count; f++)
                        //{
                        //    var temp = symbolList[f] + "#";

                        //    long tempCurrent = startDate.Ticks;
                        //    while (tempCurrent <= endDate.Ticks)
                        //    {
                        //        String lookUpIDTemp = temp + tempCurrent.ToString();
                        //        lookUpIDList.Add(lookUpIDTemp);

                        //        tempCurrent = tempCurrent + dayTicks;
                        //    }
                        //}

                        //for (int i = 0; i < lookUpIDList.Count; i++)
                        //{
                        //    tblCorrelation resultDayTradeSum = new tblCorrelation();

                        //    bool bFound = StockExchangeCorrelations_LOOKUP.TryGetValue(lookUpIDList[i], out resultDayTradeSum);
                        //    if (bFound)
                        //    {
                        //        dtList.Add(resultDayTradeSum);
                        //    }
                        //}

                        for (int i = 0; i < symbolList.Count; i++)
                        {
                            Parallel.ForEach(StockExchangeCorrelations_LOOKUP, item =>
                            {
                                if (item.Value.SymbolID == symbolList[i] && (item.Value.Event == entity) && (item.Value.TimeFrame == timeFrame))
                                {
                                    dtList.Add(item.Value);
                                }
                            });
                        }




                    } break;

                case "FOREX":
                case "Forex":
                    {
                        for (int i = 0; i < symbolList.Count; i++)
                        {
                            Parallel.ForEach(ForexCorrelations_LOOKUP, item =>
                            {
                                if (item.Value.SymbolID == symbolList[i] && (item.Value.Event == entity) && (item.Value.TimeFrame == timeFrame))
                                {
                                    dtList.Add(item.Value);
                                }
                            });
                        }
                    } break;
            }
            return dtList;
        }

        public List<tblCorrelation> GetCorrelations(List<String> symbolList, DateTime startDate, DateTime endDate, String entity, String exchange, String timeFrame)
        {
            List<tblCorrelation> dtList = new List<tblCorrelation>();
            long dayTicks = 864000000000;

            switch (exchange)
            {
                case "LSE":
                case "NASDAQ":
                case "NYSE":
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
                            tblCorrelation resultDayTradeSum = new tblCorrelation();

                            bool bFound = StockExchangeCorrelations_LOOKUP.TryGetValue(lookUpIDList[i], out resultDayTradeSum);
                            if (bFound)
                            {
                                dtList.Add(resultDayTradeSum);
                            }
                        }

                    } break;
                

                case "FOREX":
                case "Forex":
                    {
                        //List<String> lookUpIDList = new List<String>();

                        //for (int f = 0; f < symbolList.Count; f++)
                        //{
                        //    var temp = symbolList[f] + "#";

                        //    long tempCurrent = startDate.Ticks;
                        //    while (tempCurrent <= endDate.Ticks)
                        //    {
                        //        String lookUpIDTemp = temp + tempCurrent.ToString();
                        //        lookUpIDList.Add(lookUpIDTemp);

                        //        tempCurrent = tempCurrent + dayTicks;
                        //    }
                        //}

                        for (int i = 0; i < symbolList.Count; i++)
                        {
                            Parallel.ForEach(ForexCorrelations_LOOKUP, item =>
                            {
                                if (item.Value.SymbolID == symbolList[i] && (item.Value.StartDateTime == startDate) && (item.Value.EndDateTime == endDate) 
                                    && (item.Value.Event == entity) && (item.Value.TimeFrame == timeFrame))
                                {
                                    dtList.Add(item.Value);
                                }
                            });
                        }

                        //for (int i = 0; i < lookUpIDList.Count; i++)
                        //{
                        //    tblCorrelation resultDayTradeSum = new tblCorrelation();

                        //    bool bFound = ForexCorrelations_LOOKUP.TryGetValue(lookUpIDList[i], out resultDayTradeSum);
                        //    if (bFound)
                        //    {
                        //        dtList.Add(resultDayTradeSum);
                        //    }
                        //}

                    } break;
            }
            return dtList;
        }

        public List<PerformanceStats> GetPerformanceStatistics(string operation, string type, string timeframe, string exchange)
        {
            return _dataModel.GetChartPatternPerformanceStatistics(operation, type, timeframe, exchange);
        }

        public List<PerformanceStats> GetIndicatorPerformanceStatistics(List<String> symbolList, string category, string timeframe, string exchange)
        {
            return _dataModel.GetIndicatorPerformanceStatistics(symbolList, category, timeframe, exchange);
        }

        public string GetIndicatorWithBestPerformanceStatistics(List<String> symbolList, string type, string timeframe, string exchange)
        {
            return _dataModel.GetIndicatorWithBestPerformanceStatistics(symbolList, type, timeframe, exchange);
        }
        public List<DataTable> GetDatabaseData(String query)
        {
            //List or DataSet
            List<DataTable> dataTableList = new List<DataTable>();
            return dataTableList;
        }

        public string GetData(int value)
        {
            return string.Format("You entered: {0}", value);
        }
    }

    [DataContract]
    public class TradeSummary
    {
        private DateTime _date;
        private double _open;
        private double _high;
        private double _low;
        private double _close;
        private double _adjustmentclose;
        private int _volume;
        private String _timeFrame;

        private String _symbolID;

        [DataMember]
        public DateTime Date
        {
            get { return _date; }
            set { _date = value; }
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


    }


}
