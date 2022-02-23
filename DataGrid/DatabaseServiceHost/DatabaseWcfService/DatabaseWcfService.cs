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
        private InMemoryDB _leftInMemoryDB = new InMemoryDB();
        private InMemoryDB _rightInMemoryDB = new InMemoryDB();
        private bool _bRightInMemoryInUse = false;

        static public MODE Mode = MODE.Test;
        private const double _lackValueCheck = -999999.9999;


        public DatabaseWcfService()
        {     
            ThreadStart threadStart = new ThreadStart(_rightInMemoryDB.PopulateDataTables);
            Thread initialseDataTablesThread = new Thread(threadStart);
            initialseDataTablesThread.Start();
            _bRightInMemoryInUse = true;
            
            ThreadStart threadStartListen = new ThreadStart(DataUpdateListenner);
            Thread jobCreatorThread = new Thread(threadStartListen);
            jobCreatorThread.Start();
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

                        if (_bRightInMemoryInUse == false)
                        {
                            Library.WriteErrorLog(DateTime.Now + "::" + "Loading Into :: RightInMemoryDB");

                            _rightInMemoryDB.PopulateDataTables();
                            _bRightInMemoryInUse = true;

                            _leftInMemoryDB.ClearAllData();                            
                        }
                        else
                        {
                            Library.WriteErrorLog(DateTime.Now + "::" + "Loading Into :: LeftInMemoryDB");

                            _leftInMemoryDB.PopulateDataTables();
                            _bRightInMemoryInUse = false;

                            _rightInMemoryDB.ClearAllData();                            
                        }
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
        
        public List<TradeSummary> GetTradeSummaries(List<String> symbolList, DateTime startDate, DateTime endDate, String exchange, String timeFrame)
        {
            if (_bRightInMemoryInUse)
            {
                Library.WriteErrorLog(DateTime.Now + "::" + "Using RightInMemoryDB");
                return _rightInMemoryDB.GetTradeSummaries(symbolList, startDate, endDate, exchange, timeFrame);
            }
            Library.WriteErrorLog(DateTime.Now + "::" + "Using LeftInMemoryDB");
            return _leftInMemoryDB.GetTradeSummaries(symbolList, startDate, endDate, exchange, timeFrame);            
        }

        public List<EconomicFundamentals> GetEconomicFundamentals(List<string> symbolList, List<string> eventTypeList, DateTime startDate, DateTime endDate, string category, string country, List<string> economicItemHelpers)
        {
            if (_bRightInMemoryInUse)
            {
                Library.WriteErrorLog(DateTime.Now + "::" + "Using RightInMemoryDB");
                return _rightInMemoryDB.GetEconomicFundamentals(symbolList, eventTypeList, startDate, endDate, category, country, economicItemHelpers);           
            }
            Library.WriteErrorLog(DateTime.Now + "::" + "Using LeftInMemoryDB");
            return _leftInMemoryDB.GetEconomicFundamentals(symbolList, eventTypeList, startDate, endDate, category, country, economicItemHelpers);           
        }
       
        public bool DataExistForTradeSummaries(List<String> symbolList, DateTime startDate, DateTime endDate, String exchange, String timeFrame)
        {
            if (_bRightInMemoryInUse)
            {
                return _rightInMemoryDB.DataExistForTradeSummaries(symbolList, startDate, endDate, exchange, timeFrame);
            }
            return _leftInMemoryDB.DataExistForTradeSummaries(symbolList, startDate, endDate, exchange, timeFrame);     
        }

        public List<DataTable> GetDatabaseData(String query)
        {
            if (_bRightInMemoryInUse)
            {
                return _rightInMemoryDB.GetDatabaseData(query);
            }
            return _leftInMemoryDB.GetDatabaseData(query);            

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
