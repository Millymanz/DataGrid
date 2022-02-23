using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

using System.Data;
using System.Data.SqlClient;
using System.Data.DataSetExtensions;

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


    // NOTE: If you change the class name "Service1" here, you must also update the reference to "Service1" in App.config.

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class DatabaseWcfService : IDatabaseWcfService
    {
        //exchange / data
        public static Dictionary<String, List<DayTradeSummary>> dayTradeSummaries_LSE = new Dictionary<String, List<DayTradeSummary>>();
        public static Dictionary<String, List<DayTradeSummary>> dayTradeSummaries_NYSE = new Dictionary<String, List<DayTradeSummary>>();
        public static Dictionary<String, List<DayTradeSummary>> dayTradeSummaries_AMEX = new Dictionary<String, List<DayTradeSummary>>();
        public static Dictionary<String, List<DayTradeSummary>> dayTradeSummaries_NASDAQ = new Dictionary<String, List<DayTradeSummary>>();


        public static Dictionary<String, DayTradeSummary> dayTradeSummaries_AMEX_LOOKUP = new Dictionary<String, DayTradeSummary>();
        public static Dictionary<String, DayTradeSummary> dayTradeSummaries_NYSE_LOOKUP = new Dictionary<String, DayTradeSummary>();
        public static Dictionary<String, DayTradeSummary> dayTradeSummaries_NASDAQ_LOOKUP = new Dictionary<String, DayTradeSummary>();
        public static Dictionary<String, DayTradeSummary> dayTradeSummaries_LSE_LOOKUP = new Dictionary<String, DayTradeSummary>();

        public static Dictionary<String, DayTradeSummary> dayTradeSummaries_FOREX_LOOKUP = new Dictionary<String, DayTradeSummary>();



        static public MODE Mode;


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


        private void PopulateDataTables()
        {
            Console.WriteLine("\n\n");

            //clear data
            dayTradeSummaries_LSE.Clear();
            dayTradeSummaries_NYSE.Clear();
            dayTradeSummaries_NASDAQ.Clear();
            dayTradeSummaries_AMEX.Clear();

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

            GetStockExchangeDataX();


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
                port = 11022;
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

        private void GetStockExchangeDataXOld()
        {
            String currentTitle = Console.Title;

            foreach (var item in Exchanges.List)
            {
                GC.Collect();

                String stockExchange = item.ToString();

                String selector = stockExchange;
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

                    case MODE.LiveTest:
                        {
                            selector += "_LIVE-TEST";
                        }
                        break;
                }

                var settings = System.Configuration.ConfigurationManager.ConnectionStrings[selector];

                using (SqlConnection c = new SqlConnection(settings.ToString()))
                {
                    c.Open();

                    SqlCommand sqlCommand = new SqlCommand("SELECT * FROM dbo.DayTradeSummaries WHERE dbo.DayTradeSummaries.Date > '2000-01-01' ORDER BY dbo.DayTradeSummaries.SymbolID, dbo.DayTradeSummaries.Date ASC", c);

                    //SqlCommand sqlCommand = new SqlCommand("SELECT * FROM dbo.DayTradeSummaries ORDER BY dbo.DayTradeSummaries.SymbolID, dbo.DayTradeSummaries.Date ASC", c);
                    sqlCommand.CommandTimeout = 0;
                    SqlDataReader reader = sqlCommand.ExecuteReader();

                    String previousSymbolID = "";
                    
                    List<DayTradeSummary> dayTradeList = new List<DayTradeSummary>(); ;
                    
                    int count = 0;

                    while (reader.Read())
                    {
                        DayTradeSummary dayTrade = new DayTradeSummary();
                                               
                        String symbolID = reader["SymbolID"].ToString();

                        dayTrade.SymbolID = symbolID;
                        dayTrade.Date = Convert.ToDateTime(reader["Date"].ToString());
                        dayTrade.Open = Convert.ToDouble(reader["Open"].ToString());
                        dayTrade.High = Convert.ToDouble(reader["High"].ToString());
                        dayTrade.Low = Convert.ToDouble(reader["Low"].ToString());
                        dayTrade.Close = Convert.ToDouble(reader["Close"].ToString());
                        dayTrade.Volume = Convert.ToInt32(reader["Volume"].ToString());
                        dayTrade.AdjustmentClose = Convert.ToDouble(reader["AdjustmentClose"].ToString());

                        dayTradeList.Add(dayTrade);

                        previousSymbolID = symbolID;
                        
                        count++;
                    }

                    switch (stockExchange)
                    {
                        case "LSE":
                            {
                                dayTradeSummaries_LSE.Add(stockExchange, dayTradeList);
                            } break;

                        case "NASDAQ":
                            {
                                dayTradeSummaries_NASDAQ.Add(stockExchange, dayTradeList);
                            } break;

                        case "NYSE":
                            {
                                dayTradeSummaries_NYSE.Add(stockExchange, dayTradeList);
                            } break;

                        case "AMEX":
                            {
                                dayTradeSummaries_AMEX.Add(stockExchange, dayTradeList);
                            } break;
                    }
                    //dayTradeSummaries.Add(stockExchange, dayTradeList);
                }


                 switch (stockExchange)
                    {
                        case "LSE":
                            {
                                Console.WriteLine("StockExchange LSE:: " + dayTradeSummaries_LSE["LSE"].Count());
                            } break;

                        case "NASDAQ":
                            {
                                Console.WriteLine("StockExchange NASDAQ:: " + dayTradeSummaries_NASDAQ["NASDAQ"].Count());
                            } break;

                        case "NYSE":
                            {
                                Console.WriteLine("StockExchange NYSE:: " + dayTradeSummaries_NYSE["NYSE"].Count());
                            } break;

                        case "AMEX":
                            {
                                Console.WriteLine("StockExchange AMEX:: " + dayTradeSummaries_AMEX["AMEX"].Count());
                            } break;
                    }

            }
        }

        private void GetStockExchangeDataX()
        {
            String currentTitle = Console.Title;

            foreach (var item in Exchanges.List)
            {
                GC.Collect();

                //String stockExchange = item.ToString();
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

                    case MODE.LiveTest:
                        {
                            selector += "_LIVE-TEST";
                        }
                        break;
                }

                if (item == "Forex") selector += "_" + item;

                var settings = System.Configuration.ConfigurationManager.ConnectionStrings[selector];

                using (SqlConnection c = new SqlConnection(settings.ToString()))
                {
                    c.Open();
                    String query = "SELECT * FROM dbo.tblTrades WHERE dbo.tblTrades.StockExchange = '"+ item +"' AND dbo.tblTrades.DateTime > '2000-01-01' ORDER BY dbo.tblTrades.SymbolID, dbo.tblTrades.DateTime ASC";
                    if (item == "Forex") query = "SELECT * FROM dbo.tblTrades WHERE dbo.tblTrades.DateTime > '2000-01-01' ORDER BY dbo.tblTrades.SymbolID, dbo.tblTrades.DateTime ASC";
                    
                    SqlCommand sqlCommand = new SqlCommand(query, c);

                    //SqlCommand sqlCommand = new SqlCommand("SELECT * FROM dbo.DayTradeSummaries ORDER BY dbo.DayTradeSummaries.SymbolID, dbo.DayTradeSummaries.Date ASC", c);
                    sqlCommand.CommandTimeout = 0;
                    SqlDataReader reader = sqlCommand.ExecuteReader();

                    String previousSymbolID = "";

                    List<DayTradeSummary> dayTradeList = new List<DayTradeSummary>(); ;

                    int count = 0;

                    while (reader.Read())
                    {
                        DayTradeSummary dayTrade = new DayTradeSummary();

                        String symbolID = reader["SymbolID"].ToString();

                        dayTrade.SymbolID = symbolID;
                        dayTrade.Date = Convert.ToDateTime(reader["DateTime"].ToString());
                        dayTrade.Open = Convert.ToDouble(reader["Open"].ToString());
                        dayTrade.High = Convert.ToDouble(reader["High"].ToString());
                        dayTrade.Low = Convert.ToDouble(reader["Low"].ToString());
                        dayTrade.Close = Convert.ToDouble(reader["Close"].ToString());

                        if (item != "Forex")
                        {
                            dayTrade.Volume = Convert.ToInt32(reader["Volume"].ToString());
                        }

                        dayTrade.AdjustmentClose = Convert.ToDouble(reader["AdjustmentClose"].ToString());

                        //dayTradeList.Add(dayTrade);
                        previousSymbolID = symbolID;

                        String lookUpID = symbolID + "#" + dayTrade.Date.Ticks.ToString();

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
                                    dayTradeSummaries_FOREX_LOOKUP.Add(lookUpID, dayTrade);
                                } break;
                        }
                        count++;
                    }                   
                }


                switch (stockExchange)
                {
                    case "LSE":
                        {
                            Console.WriteLine("StockExchange LSE:: " + dayTradeSummaries_LSE_LOOKUP.Count());
                        } break;

                    case "NASDAQ":
                        {
                            Console.WriteLine("StockExchange NASDAQ:: " + dayTradeSummaries_NASDAQ_LOOKUP.Count());
                        } break;

                    case "NYSE":
                        {
                            Console.WriteLine("StockExchange NYSE:: " + dayTradeSummaries_NYSE_LOOKUP.Count());
                        } break;

                    case "AMEX":
                        {
                            Console.WriteLine("StockExchange AMEX:: " + dayTradeSummaries_AMEX_LOOKUP.Count());
                        } break;
                    
                    case "Forex":
                        {
                            Console.WriteLine("StockExchange Forex:: " + dayTradeSummaries_FOREX_LOOKUP.Count());
                        } break;
                }

            }
        }

        public List<DayTradeSummary> GetDayTradeSummaries(List<String> symbolList, DateTime startDate, DateTime endDate, String stockExchange)
        {
            List<DayTradeSummary> dtList = new List<DayTradeSummary>();
            long dayTicks = 864000000000;

            switch (stockExchange)
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
                            DayTradeSummary resultDayTradeSum = new DayTradeSummary();

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
                            DayTradeSummary resultDayTradeSum = new DayTradeSummary();

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
                            DayTradeSummary resultDayTradeSum = new DayTradeSummary();

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
                            DayTradeSummary resultDayTradeSum = new DayTradeSummary();

                            bool bFound = dayTradeSummaries_AMEX_LOOKUP.TryGetValue(lookUpIDList[i], out resultDayTradeSum);
                            if (bFound)
                            {
                                dtList.Add(resultDayTradeSum);
                            }
                        }

                    } break;
            }
            return dtList;
        }

        public List<DataTable> GetDatabaseData(String query)
        {
            //List or DataSet
            List<DataTable> dataTableList = new List<DataTable>();
            return dataTableList;
        }
    }



    [DataContract]
    public class DayTradeSummary
    {
        private DateTime date;
        private double open;
        private double high;
        private double low;
        private double close;
        private double adjustmentclose;
        private int volume;

        private String symbolID;

        [DataMember]
        public DateTime Date
        {
            get { return date; }
            set { date = value; }
        }

        [DataMember]
        public double Open
        {
            get { return open; }
            set { open = value; }
        }

        [DataMember]
        public double High
        {
            get { return high; }
            set { high = value; }
        }

        [DataMember]
        public double Low
        {
            get { return low; }
            set { low = value; }
        }

        [DataMember]
        public double Close
        {
            get { return close; }
            set { close = value; }
        }

        [DataMember]
        public int Volume
        {
            get { return volume; }
            set { volume = value; }
        }

        [DataMember]
        public double AdjustmentClose
        {
            get { return adjustmentclose; }
            set { adjustmentclose = value; }
        }
 
        [DataMember]
        public String SymbolID
        {
            get { return symbolID; }
            set { symbolID = value; }
        }


    }

}
