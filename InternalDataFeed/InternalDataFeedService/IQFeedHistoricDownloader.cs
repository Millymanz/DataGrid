using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// added for access to RegistryKey
using Microsoft.Win32;
// added for access to socket classes
using System.Net;
using System.Net.Sockets;
using System.Timers;
using System.Text.RegularExpressions;
using System.IO;
using CsvHelper;
using System.ServiceModel;

namespace InternalDataFeedService
{
    public class TradeSummaryTemporary
    {
        public DateTime DateTime;
        public double Bid;
        public double Ask;
        public double Open;
        public double High;
        public double Low;
        public double Close;
        public double AdjustmentClose;
        public int Volume;
        public String SymbolID;
        public String Exchange;
        public String TimeFrame;
    }


    public struct AdditionalData
    {
        public String Symbol;
        public String Exchange; /*= "NONE";*/
        public String TimeFrame;
        public String Path;
    }

    public enum DTNUpdateFields : int
    {
        Type, // 0
        Symbol, // 1
        Change, // 2
        TotalVolume, // 3 
        High, // 4
        Low, // 5
        Bid, // 6
        Ask, // 7
        BidSize, // 8
        AskSize, // 9
        OpenInterest, // 10
        Open, // 11
        Close, // 12
        Delay, // 13
        MarketIsOpen, // 14
        TickID, // 15
        Last, // "Most Recent Trade" // 16
        TradeSize, // "Most Recent Trade Size" // 17
        TradeTime, // "Most Recent Trade TimeMS" // 18
        TradeConditions, // "Most Recent Trade Conditions" // 19
        MessageContents, // 20
        TradeDate, // "Most Recent Trade Date" // 21
        Count
    }

    public enum DTNFields : int
    {
        Type, // 0 ..?
        Symbol, // 1* 
        Change, // 2 ..?
        Rubbish, // 3 ..?
        TradeDate, // 4
        LowRubbish, // 5..?
        TotalVolume, // 6*
        Ask, // 7
        BidSize, // 8
        AskSize, // 9
        OpenInterest, // 10
        Open, // 11
        High, // 12
        Low, // 13
        Close, // 14
        TickID, // 15
        Last, // "Most Recent Trade" // 16
        TradeSize, // "Most Recent Trade Size" // 17
        TradeTime, // "Most Recent Trade TimeMS" // 18
        TradeConditions, // "Most Recent Trade Conditions" // 19
        MessageContents, // 20
        Count
    }


    public enum DTNFieldsTRVersion : int
    {
        Type, // 0 ..?
        Symbol, // 1* 
        DateTime, // 2 ..?
        Open, // 3 ..?
        High, // 4
        Low, // 5..?
        Close, // 6*
        Volume, // 7
        Ask, // 8
        Bid
    }


    public class CsvRow : List<string>
    {
        public string LineText { get; set; }
    }

    /// <summary>
    /// Class to write data to a CSV file
    /// </summary>
    public class CsvFileWriter : StreamWriter
    {
        public CsvFileWriter(Stream stream)
            : base(stream)
        {
        }

        public CsvFileWriter(string filename)
            : base(filename, true)
        {
        }

        /// <summary>
        /// Writes a single row to a CSV file.
        /// </summary>
        /// <param name="row">The row to be written</param>
        public void WriteRow(CsvRow row)
        {
            String rowStr = "";
            try
            {
                StringBuilder builder = new StringBuilder();
                bool firstColumn = true;
                foreach (string value in row)
                {
                    // Add separator if this isn't the first value
                    if (!firstColumn)
                        builder.Append(',');
                    // Implement special handling for values that contain comma or quote
                    // Enclose in quotes and double up any double quotes
                    if (value.IndexOfAny(new char[] { '"', ',' }) != -1)
                        builder.AppendFormat("\"{0}\"", value.Replace("\"", "\"\""));
                    else
                        builder.Append(value);
                    firstColumn = false;
                }

                row.LineText = builder.ToString();

                rowStr = row.LineText;

                WriteLine(row.LineText);
            }
            catch (InvalidOperationException ex)
            {
                Logger.log.Error("InternalDataFeedService -> WriteRow() :: " + ex.ToString());
                Library.WriteErrorLog(DateTime.Now + " :: " + ex.ToString());

                ServiceManager.GlobalhandlerComFailure.Invoke();
            }
            catch (CommunicationObjectAbortedException ex)
            {
                Logger.log.Error("InternalDataFeedService -> WriteRow() :: " + ex.ToString());
                Library.WriteErrorLog(DateTime.Now + " :: " + ex.ToString());

                ServiceManager.GlobalhandlerComFailure.Invoke();
            }
            catch (CommunicationObjectFaultedException ex)
            {
                Logger.log.Error("InternalDataFeedService -> WriteRow() :: " + ex.ToString());
                Library.WriteErrorLog(DateTime.Now + " :: " + ex.ToString());

                ServiceManager.GlobalhandlerComFailure.Invoke();
            }
            catch (CommunicationException ex)
            {
                Logger.log.Error("InternalDataFeedService -> WriteRow() :: " + ex.ToString());
                Library.WriteErrorLog(DateTime.Now + " :: " + ex.ToString());

                ServiceManager.GlobalhandlerComFailure.Invoke();
            }
            catch (Exception ex)
            {
                Logger.log.Error("InternalDataFeedService -> WriteRow() :: " + ex.ToString());
                Library.WriteErrorLog(DateTime.Now + " :: " + ex.ToString());

                Console.WriteLine("WriteRow Execption - {0} - {1}" + rowStr + ex);
            }
        }
    }

    public delegate void TradeSummaryDelegate(List<TradeSummary> tradeSummary);


    public static class DataFeedTimer
    {
        public static Timer dataFeedTimerChecker = null;
        
        public static void StartDataFeedTimer()
        {
            dataFeedTimerChecker = new Timer();

            //dataFeedTimerChecker.Interval = 180000; //every 3 mins

            dataFeedTimerChecker.Interval = 300000; //every 5 mins

            dataFeedTimerChecker.Elapsed += new System.Timers.ElapsedEventHandler(timerFeed_Tick);
            dataFeedTimerChecker.Enabled = true;
        }

        public static void timerFeed_Tick(object sender, ElapsedEventArgs e)
        {
            //Write code here to do some job depends on your requirement
            Console.WriteLine("[TIME UP RESTART DATA FEED INCASE OF PROBLEM]");
            ServiceManager.Ignore = true;
            Logger.log.Info("[TIME UP RESTART DATA FEED INCASE OF PROBLEM]");


            Library.WriteErrorLog(DateTime.Now + ":: [TIME UP RESTART DATA FEED INCASE OF PROBLEM]");


            //ServiceManager.GlobalhandlerComFailure.Invoke();

            //SendMessage("IDF_Alive_Confirmed_DoNotWorry", 11875);
        }

        public static void ResetAliveTimer()
        {
            if (dataFeedTimerChecker != null)
            {
                try
                {
                    dataFeedTimerChecker.Stop();
                    //dataFeedTimerChecker.Dispose();

                    dataFeedTimerChecker.Start();
                }
                catch (Exception ex)
                {
                    //Console.WriteLine(ex.ToString());
                }
            }
        }
    }

       
    public class IQFeedHistoricDownloader
    {

        // socket communication global variables
        AsyncCallback m_pfnLookupCallback;
        Socket m_sockLookup;
        // we create the socket buffer global for performance
        byte[] m_szLookupSocketBuffer = new byte[262144];
        // stores unprocessed data between reads from the socket
        string m_sLookupIncompleteRecord = "";
        // flag for tracking when a call to BeginReceive needs called
        bool m_bLookupNeedBeginReceive = true;

        //------------------------------------------------------------//
        // global variables for socket communications to the level1 socket
        AsyncCallback m_pfnLevel1Callback;
        Socket m_sockLevel1;
        // we create the socket buffer global for performance
        byte[] m_szLevel1SocketBuffer = new byte[8096];
        // stores unprocessed data between reads from the socket
        string m_sLevel1IncompleteRecord = "";
        // flag for tracking when a call to BeginReceive needs called
        bool m_bLevel1NeedBeginReceive = true;


        //------------------------------------------------------------//

        private List<TradeSummaryTemporary> _dataCollectionCheck = new List<TradeSummaryTemporary>();

        // delegate for updating the data display.
        public delegate void UpdateDataHandler(string sMessage);

        private String _fileName;
        private Queue<AdditionalData> requestedSymbolQueue = new Queue<AdditionalData>();

        //private List<RealTimeRawDataService.TradeSummary> _realTimeTradeSummary = new List<RealTimeRawDataService.TradeSummary>();
        private Queue<TradeSummary> _realTimeTradeSummaryQueue = new Queue<TradeSummary>();
        private static TradeSummaryDelegate _updateTradeSummaryHandler = null;
        private List<TradeSummary> _tradeSummaryListing = new List<TradeSummary>();

        //Used with real time data feed
        private TradeSummary _currentRealTimeTradeSummary = null;


        private Object thisLock = new Object();
        public bool IsLocked = false;

        public static bool bResumeSendRequest = false;

        private String _currentSymbolID = "";
        private String _exchange = "";
        private String _timeFrame = "";
        private String _path = "";


        private static TradeSummaryOnDemandDelegate _updateTradeSummaryHandlerOnDemand = null;
        public String OnDemandClientName { get; set; }

        private bool _bFirstTime = true;

        public String OutputFileName
        {
            get { return _fileName; }
            set { _fileName = value; }
        }

        System.Timers.Timer actionTimer;

        int TimeInterval = 0;

        //public static void timerFeed_Tick(object sender, ElapsedEventArgs e)
        //{
        //    //Write code here to do some job depends on your requirement
        //    Console.WriteLine("time up restart Data feed incase of problem");

        //}


        private void Task(object sender, ElapsedEventArgs e)
        {
            actionTimer.Stop();
            actionTimer.Enabled = false;

            var dataFeedMode = System.Configuration.ConfigurationManager.AppSettings["DATAFEED_MODE"];
            if (dataFeedMode == "FILE")
            {
                UpdateRealTimeDataGridFileVersion();
            }
            else
            {
                MinuteTrigger();
            }

            var controlTime = System.Configuration.ConfigurationManager.AppSettings["CONTROL_TIME_INTERVAL"];

            if (controlTime == "FALSE")
                TimeInterval = 10000 * 6;

            System.Diagnostics.Debug.WriteLine(DateTime.Now.ToString("dd/MM/yyyy H:mm:ss"));

            //Tell DBM to start import
            //actionTimer.Interval = 10000 * 6; 
            actionTimer.Interval = TimeInterval;
            actionTimer.Enabled = true;
            actionTimer.Start();

            _bFirstTime = false;
        }

        public void SystemEvents_TimeChanged(object sender, EventArgs e)
        {
            actionTimer.Stop();
            actionTimer.Enabled = false;

            // Create a timer with a ten second interval.
            //aTimer = new System.Timers.Timer(10000);

            actionTimer.Interval = TimeInterval;

            actionTimer.Enabled = true;
            actionTimer.Start();
        }

        public IQFeedHistoricDownloader(String currentSymbol, String exchange)
        {
            if (exchange == "Forex" || exchange == "FOREX")
            {
                _currentSymbolID = RemoveExtText(currentSymbol, ".FXCM"); ;
            }
            else
            {
                _currentSymbolID = currentSymbol;
            }
            _exchange = exchange;

            TimeInterval = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["TIME_INTERVAL_MILLISECONDS"]);


            var dataFeedMode = System.Configuration.ConfigurationManager.AppSettings["DATAFEED_MODE"];

            if (dataFeedMode == "FILE")
            {
                var splitArray = Regex.Split(currentSymbol, ".FXCM");
                String symbolID = splitArray.FirstOrDefault();


                String path = "Data\\" + symbolID + "_Data_Short.csv";

                if (System.IO.File.Exists(path))
                {
                    using (StreamReader reader = new StreamReader(path))
                    {
                        //line = reader.ReadLine();

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
                            tradeSummary.Exchange = "FOREX";

                            _realTimeTradeSummaryQueue.Enqueue(tradeSummary);
                        }
                    }
                }
            }
            else
            {
                // create the socket and tell it to connect
                m_sockLevel1 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPAddress ipLocalhost = IPAddress.Parse("127.0.0.1");

                // pull the level 1 port out of the registry.  we use the Level 1 port because we want streaming updates
                int iPort = GetIQFeedPort("Level1");

                IPEndPoint ipendLocalhost = new IPEndPoint(ipLocalhost, iPort);

                try
                {
                    // tell the socket to connect to IQFeed
                    m_sockLevel1.Connect(ipendLocalhost);

                    // Set the protocol for the level1 socket to 5.1 so we have access to the trades only watch request
                    SendRequestToIQFeed("S,SET PROTOCOL,5.1\r\n");

                    // this example is using asynchronous sockets to communicate with the feed.  As a result, we are using .NET's BeginReceive and EndReceive calls with a callback.
                    // we call our WaitForData function (see below) to notify the socket that we are ready to receive callbacks when new data arrives
                    WaitForData("Level1");

                    if (_exchange == "Forex" || _exchange == "FOREX")
                    {
                        SendRequestToIQFeed(String.Format("S,SELECT UPDATE FIELDS,{0}\r\n", "Symbol,Last TimeMS,Open,High,Low,Close,Total Volume,Ask,Bid"));
                    }
                    else
                    {
                        SendRequestToIQFeed(String.Format("S,SELECT UPDATE FIELDS,{0}\r\n", "Symbol,Last TimeMS,Open,High,Low,Close,Total Volume"));
                    }


                }
                catch (InvalidOperationException ex)
                {
                    Logger.log.Error("InternalDataFeedService -> IQFeedHistoricDownloader() :: " + ex.ToString());

                    ServiceManager.GlobalhandlerComFailure.Invoke();
                }
                catch (CommunicationObjectAbortedException ex)
                {
                    Logger.log.Error("InternalDataFeedService -> IQFeedHistoricDownloader() :: " + ex.ToString());

                    ServiceManager.GlobalhandlerComFailure.Invoke();
                }
                catch (CommunicationObjectFaultedException ex)
                {
                    Logger.log.Error("InternalDataFeedService -> IQFeedHistoricDownloader() :: " + ex.ToString());

                    ServiceManager.GlobalhandlerComFailure.Invoke();
                }
                catch (CommunicationException ex)
                {
                    Logger.log.Error("InternalDataFeedService -> IQFeedHistoricDownloader() :: " + ex.ToString());

                    ServiceManager.GlobalhandlerComFailure.Invoke();
                }
                catch (SocketException ex)
                {
                    Logger.log.Error("InternalDataFeedService -> IQFeedHistoricDownloader() :: " + ex.ToString());

                    Console.WriteLine(ex.ToString());
                }
            }

            var controlTime = System.Configuration.ConfigurationManager.AppSettings["CONTROL_TIME_INTERVAL"];

            if (_bFirstTime && controlTime == "FALSE")
            {
                int min = (DateTime.Now.Minute + 1) == 60 ? 0 : (DateTime.Now.Minute + 1);
                int hour = DateTime.Now.Hour;

                if (min == 0)
                {
                    hour = (DateTime.Now.Hour + 1) == 24 ? 0 : (DateTime.Now.Hour + 1);
                }

                var dateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, hour, min, 0);

                TimeSpan timeSpan = dateTime - DateTime.Now;
                TimeInterval = Convert.ToInt32(timeSpan.TotalMilliseconds);
            }


            SystemEvents.TimeChanged += new EventHandler(SystemEvents_TimeChanged);
            actionTimer = new System.Timers.Timer();

            actionTimer.Elapsed += new ElapsedEventHandler(Task);
            actionTimer.Interval = TimeInterval;
            actionTimer.AutoReset = true;

            actionTimer.Start();
        }

        public void Watch(String symbolCriteria)
        {
            SendRequestToIQFeed(String.Format("w{0}\r\n", symbolCriteria));
        }

        public static String RemoveExtText(String symbolCriteria, String extension)
        {
            var splitArray = Regex.Split(symbolCriteria, extension);
            return splitArray.FirstOrDefault();
        }

        public List<TradeSummary> GetTradeSummaryData(String symbol, int interval, String exchange, DateTime beginDate)
        {
            string sRequest = "";

            var dateStr = beginDate.ToString("yyyyMMdd");

            // request in the format:
            // HIT,SYMBOL,INTERVAL,BEGINDATE BEGINTIME,ENDDATE ENDTIME,MAXDATAPOINTS,BEGINFILTERTIME,ENDFILTERTIME,DIRECTION,REQUESTID,DATAPOINTSPERSEND,INTERVALTYPE<CR><LF>
            sRequest = String.Format("HIT,{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10}\r\n", symbol, interval, dateStr, "", "", "", "", "", "", "", "s");

            SendRequestToIQFeed(sRequest);

            return null;
        }

        public void SendRequestToIQFeed(string sCommand)
        {
            var dataFeedMode = System.Configuration.ConfigurationManager.AppSettings["DATAFEED_MODE"];

            if (dataFeedMode != "FILE")
            {
                // and we send it to the feed via the socket
                byte[] szCommand = new byte[sCommand.Length];
                szCommand = Encoding.ASCII.GetBytes(sCommand);
                int iBytesToSend = szCommand.Length;
                try
                {
                    int iBytesSent = m_sockLevel1.Send(szCommand, iBytesToSend, SocketFlags.None);
                    if (iBytesSent != iBytesToSend)
                    {
                        Console.WriteLine(String.Format("Error Sending Request:\r\n{0}", sCommand.TrimEnd("\r\n".ToCharArray())));
                    }
                    else
                    {
                        Console.WriteLine(String.Format("Request Sent Successfully:\r\n{0}", sCommand.TrimEnd("\r\n".ToCharArray())));
                    }
                }
                catch (InvalidOperationException ex)
                {
                    Logger.log.Error("InternalDataFeedService -> SendRequestToIQFeed() :: " + ex.ToString());

                    ServiceManager.GlobalhandlerComFailure.Invoke();
                }
                catch (CommunicationObjectAbortedException ex)
                {
                    Logger.log.Error("InternalDataFeedService -> SendRequestToIQFeed() :: " + ex.ToString());

                    ServiceManager.GlobalhandlerComFailure.Invoke();
                }
                catch (CommunicationObjectFaultedException ex)
                {
                    Logger.log.Error("InternalDataFeedService -> SendRequestToIQFeed() :: " + ex.ToString());

                    ServiceManager.GlobalhandlerComFailure.Invoke();
                }
                catch (CommunicationException ex)
                {
                    Logger.log.Error("InternalDataFeedService -> SendRequestToIQFeed() :: " + ex.ToString());

                    ServiceManager.GlobalhandlerComFailure.Invoke();
                }
                catch (SocketException ex)
                {
                    Logger.log.Error("InternalDataFeedService -> SendRequestToIQFeed() :: " + ex.ToString());

                    // handle socket errors
                    Console.WriteLine(String.Format("Socket Error Sending Request:\r\n{0}\r\n{1}", sCommand.TrimEnd("\r\n".ToCharArray()), ex.Message));
                }
            }
        }


        private void OnReceive(IAsyncResult asyn)
        {
            try
            {
                // first verify we received data from the correct socket.  This check isn't really necessary in this example since we 
                // only have a single socket but if we had multiple sockets, we could use this check to use the same callback to recieve data from
                // multiple sockets
                if (asyn.AsyncState.ToString().Equals("Level1"))
                {
                    // read data from the socket.
                    int iReceivedBytes = 0;
                    iReceivedBytes = m_sockLevel1.EndReceive(asyn);
                    // set our flag back to true so we can call begin receive again
                    m_bLevel1NeedBeginReceive = true;
                    // in this example, we will convert to a string for ease of use.
                    string sData = Encoding.ASCII.GetString(m_szLevel1SocketBuffer, 0, iReceivedBytes);

                    // When data is read from the socket, you can get multiple messages at a time and there is no guarantee
                    // that the last message you receive will be complete.  It is possible that only half a message will be read
                    // this time and you will receive the 2nd half of the message at the next call to OnReceive.
                    // As a result, we need to save off any incomplete messages while processing the data and add them to the beginning
                    // of the data next time.
                    sData = m_sLevel1IncompleteRecord + sData;
                    // clear our incomplete record string so it doesn't get processed next time too.
                    m_sLevel1IncompleteRecord = "";

                    // now we loop through the data breaking it appart into messages.  Each message on this port is terminated
                    // with a newline character ("\n")
                    string sLine = "";
                    int iNewLinePos = -1;
                    while (sData.Length > 0)
                    {
                        iNewLinePos = sData.IndexOf("\n");
                        if (iNewLinePos > 0)
                        {
                            sLine = sData.Substring(0, iNewLinePos);
                            // we know what type of message was recieved by the first character in the message.
                            switch (sLine[0])
                            {
                                case 'Q':
                                    ProcessUpdateMsg(sLine);
                                    break;
                                case 'F':
                                    ProcessFundamentalMsg(sLine);
                                    break;
                                case 'P':
                                    ProcessSummaryMsg(sLine);
                                    break;
                                case 'N':
                                    ProcessNewsHeadlineMsg(sLine);
                                    break;
                                case 'S':
                                    ProcessSystemMsg(sLine);
                                    break;
                                case 'R':
                                    ProcessRegionalMsg(sLine);
                                    break;
                                case 'T':
                                    ProcessTimestamp(sLine);
                                    break;
                                case 'E':
                                    ProcessErrorMsg(sLine);
                                    break;
                                default:
                                    // we processed something else we weren't expecting.  Ignore it
                                    break;
                            }
                            // move on to the next message.  This isn't very efficient but it is simple (which is the focus of this example).
                            sData = sData.Substring(sLine.Length + 1);
                        }
                        else
                        {
                            // we get here when there are no more newline characters in the data.  
                            // save off the rest of message for processing the next batch of data.
                            m_sLevel1IncompleteRecord = sData;
                            sData = "";
                        }
                    }

                    // call wait for data to notify the socket that we are ready to receive another callback
                    WaitForData("Level1");
                    //LimitListItems();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Logger.log.Error(ex.ToString());

                ServiceManager.GlobalhandlerComFailure.Invoke();
            }
        }


        private void WaitForData(string sSocketName)
        {
            try
            {
                if (sSocketName.Equals("Level1"))
                {
                    // make sure we have a callback created
                    if (m_pfnLevel1Callback == null)
                    {
                        m_pfnLevel1Callback = new AsyncCallback(OnReceive);
                    }

                    // send the notification to the socket.  It is very important that we don't call Begin Reveive more than once per call
                    // to EndReceive.  As a result, we set a flag to ignore multiple calls.
                    if (m_bLevel1NeedBeginReceive)
                    {
                        m_bLevel1NeedBeginReceive = false;
                        // we pass in the sSocketName in the state parameter so that we can verify the socket data we receive is the data we are looking for
                        m_sockLevel1.BeginReceive(m_szLevel1SocketBuffer, 0, m_szLevel1SocketBuffer.Length, SocketFlags.None, m_pfnLevel1Callback, sSocketName);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Logger.log.Error(ex.ToString());

                ServiceManager.GlobalhandlerComFailure.Invoke();
            }
        }

        private int GetIQFeedPort(string sPort)
        {
            int iReturn = 0;
            RegistryKey key = Registry.CurrentUser.OpenSubKey("Software\\DTN\\IQFeed\\Startup");
            if (key != null)
            {
                string sData = "";
                switch (sPort)
                {
                    case "Level1":
                        // the default port for Level 1 data is 5009.
                        sData = key.GetValue("Level1Port", "5009").ToString();
                        break;
                    case "Lookup":
                        // the default port for Lookup data is 9100.
                        sData = key.GetValue("LookupPort", "9100").ToString();
                        break;
                    case "Level2":
                        // the default port for Level 2 data is 9200.
                        sData = key.GetValue("Level2Port", "9200").ToString();
                        break;
                    case "Admin":
                        // the default port for Admin data is 9300.
                        sData = key.GetValue("AdminPort", "9300").ToString();
                        break;
                    case "Derivative":
                        // the default port for derivative data is 9400
                        sData = key.GetValue("DerivativePort", "9400").ToString();
                        break;
                }
                iReturn = Convert.ToInt32(sData);
            }
            return iReturn;
        }

        /// <summary>
        /// Process an update message from the feed.
        /// </summary>
        /// <param name="sLine"></param>
        private void ProcessUpdateMsg(string sLine)
        {
            // Update messages are sent to the client anytime one of the fields in the current fieldset are updated.
            // In this example, we just display the data to the user.
            // For a list of fields that are contained in the update message, please check the documentation page UpdateSummary Message Format.
            UpdateListbox(sLine);
        }

        /// <summary>
        /// Process a fundamental message from the feed
        /// </summary>
        /// <param name="sLine"></param>
        private void ProcessFundamentalMsg(string sLine)
        {
            // fundamental data will contain data about the stock symbol that does not frequently change (at most once a day).
            // In this example, we just display the data to the user.
            // For a list of fields that are contained in the fundamental message, please check the documentation page Fundamental Message Format.
            UpdateListbox(sLine);
        }

        /// <summary>
        /// Process a summary message from the feed.
        /// </summary>
        /// <param name="sLine"></param>
        private void ProcessSummaryMsg(string sLine)
        {
            // summary data will be in the same format as the Update messages and will contain the most recent data for each field at the 
            //      time you watch the symbol.
            // In this example, we just display the data to the user.
            // For a list of fields that are contained in the Summary message, please check the documentation page UpdateSummaryMessage Format.
            UpdateListbox(sLine);
        }

        /// <summary>
        /// Process a news headline message from the feed.
        /// </summary>
        /// <param name="sLine"></param>
        private void ProcessNewsHeadlineMsg(string sLine)
        {
            // News messages are received anytime a new news story is received for a news type you are authorized to receive AND only when you have streaming news turned on
            // In this example, we just display the data to the user.
            // For a list of fields that are contained in the news message, please check the documentation page Streaming News Data Message Format.
            UpdateListbox(sLine);
        }

        /// <summary>
        /// Process a system message from the feed
        /// </summary>
        /// <param name="sLine"></param>
        private void ProcessSystemMsg(string sLine)
        {
            // system messages are sent to inform the client about current system information.
            // In this example, we just display the data to the user.
            // For a list of system messages that can be sent and the fields each contains, please check the documentation page System Messages.
            UpdateListbox(sLine);
        }

        /// <summary>
        /// Process a timestamp message from the feed.
        /// </summary>
        /// <param name="sLine"></param>
        private void ProcessTimestamp(string sLine)
        {
            // Timestamp messages are sent to the client once a second.  These timestamps are generated by our servers and can be used as a "server time"
            // In this example, we just display the data to the user.
            UpdateListbox(sLine);
        }

        /// <summary>
        /// Process an error message from the feed
        /// </summary>
        /// <param name="sLine"></param>
        private void ProcessErrorMsg(string sLine)
        {
            // Error messages are sent to the client to inform the client of problems.
            // In this example, we just display the data to the user.
            UpdateListbox(sLine);
        }

        /// <summary>
        /// Process a regional message from the feed.
        /// </summary>
        /// <param name="sLine"></param>
        private void ProcessRegionalMsg(string sLine)
        {
            // Regional messages are sent to the client anytime one of the fields for a region updates AND only when the client has requested 
            //      to watch regionals for a specific symbol.
            // In this example, we just display the data to the user.
            // For a list of fields that are contained in the regional message, please check the documentation page Regional Message Format.
            UpdateListbox(sLine);
        }


        public void UpdateListview(string sData)
        {
            try
            {
                // check if we need to invoke the delegate

                // delegate not required, just update the list box.
                List<String> lstMessages = new List<string>(sData.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries));

                lstMessages.ForEach(delegate(String sLine)
                {
                    //String[] results = sLine.Split('\n');
                    String[] results = sLine.Split(',');

                    var item = results[0].Split(',');

                    DateTime testDate = new DateTime();

                    String tempExchange = "";

                    if (results.Length >= 5 && results.Length < 9)
                    {
                        var splitArray = System.Text.RegularExpressions.Regex.Split("", ".FXCM");
                        String selectedSymbol = splitArray.Length > 0 ? splitArray[0] : "";


                    }

                });
            }
            catch (InvalidOperationException ex)
            {
                Logger.log.Error("InternalDataFeedService -> UpdateListview() :: " + ex.ToString());

                ServiceManager.GlobalhandlerComFailure.Invoke();
            }
            catch (CommunicationObjectAbortedException ex)
            {
                Logger.log.Error("InternalDataFeedService -> UpdateListview() :: " + ex.ToString());

                ServiceManager.GlobalhandlerComFailure.Invoke();
            }
            catch (CommunicationObjectFaultedException ex)
            {
                Logger.log.Error("InternalDataFeedService -> UpdateListview() :: " + ex.ToString());

                ServiceManager.GlobalhandlerComFailure.Invoke();
            }
            catch (CommunicationException ex)
            {
                Logger.log.Error("InternalDataFeedService -> UpdateListview() :: " + ex.ToString());

                ServiceManager.GlobalhandlerComFailure.Invoke();
            }
            //catch (ObjectDisposedException ex)
            //{
            //    throw;
            //    // The list view object went away, ignore it since we're probably exiting.
            //}
        }


        /// <summary>
        /// we have to use a delegate to update controls in the dialog to resolve cross-threading issues built into the .NET framework
        /// </summary>
        /// <param name="sData"></param>
        public void UpdateListbox(string sData)
        {
            var Str = sData.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            
            //do not show tick data too processor intensive on the screen.
            //Console.WriteLine(sData);

            //if (_currentSymbolID == "EURUSD.FXCM")
            {
                DataFeedTimer.ResetAliveTimer();
            }

            String[] StrArray = sData.Split(',');

            DateTime testDate = new DateTime();

            if (StrArray.Count() > 6 && StrArray.Count() <= 20)
            {
                var tempSym = _currentSymbolID + ".FXCM";
                if (StrArray[(int)DTNUpdateFields.Symbol] == _currentSymbolID || StrArray[(int)DTNUpdateFields.Symbol] == tempSym)
                {
                    if (DateTime.TryParse(StrArray[(int)DTNFields.TradeDate], out testDate) && StrArray[(int)DTNFields.Type] == "P" || StrArray[(int)DTNFields.Type] == "Q")
                    {
                        var tradeSummary = new TradeSummaryTemporary();

                        tradeSummary.High = Convert.ToDouble(StrArray[(int)DTNFieldsTRVersion.High]);
                        tradeSummary.Low = Convert.ToDouble(StrArray[(int)DTNFieldsTRVersion.Low]);
                        tradeSummary.Open = Convert.ToDouble(StrArray[(int)DTNFieldsTRVersion.Open]);
                        tradeSummary.Close = Convert.ToDouble(StrArray[(int)DTNFieldsTRVersion.Close]);
                        tradeSummary.DateTime = DateTime.Parse(StrArray[(int)DTNFieldsTRVersion.DateTime]);
                        tradeSummary.Volume = Convert.ToInt32(StrArray[(int)DTNFieldsTRVersion.Volume]);

                        if (_exchange == "Forex" || _exchange == "FOREX")
                        {
                            tradeSummary.Bid = Convert.ToDouble(StrArray[(int)DTNFieldsTRVersion.Bid]);
                            tradeSummary.Ask = Convert.ToDouble(StrArray[(int)DTNFieldsTRVersion.Ask]);
                        }
                        tradeSummary.TimeFrame = "1min";

                        tradeSummary.AdjustmentClose = tradeSummary.Close;

                        String symbolIDTemp = StrArray[(int)DTNFieldsTRVersion.Symbol];

                        if (_exchange == "Forex" || _exchange == "FOREX")
                        {
                            symbolIDTemp = RemoveExtText(symbolIDTemp, ".FXCM");
                        }
                        tradeSummary.SymbolID = symbolIDTemp;

                        MinuteGenerationCheck(tradeSummary, sData);

                        //_currentRealTimeTradeSummary = tradeSummary;
                    }
                }
            }
        }

        private void MinuteGenerationCheck(TradeSummaryTemporary tradeSumTemp, string sData)
        {
            if (_dataCollectionCheck.Any())
            {
                if (_dataCollectionCheck.LastOrDefault().DateTime.Year == tradeSumTemp.DateTime.Year &&
                    _dataCollectionCheck.LastOrDefault().DateTime.Month == tradeSumTemp.DateTime.Month &&
                    _dataCollectionCheck.LastOrDefault().DateTime.Day == tradeSumTemp.DateTime.Day &&
                    _dataCollectionCheck.LastOrDefault().DateTime.Hour == tradeSumTemp.DateTime.Hour &&
                    _dataCollectionCheck.LastOrDefault().DateTime.Minute != tradeSumTemp.DateTime.Minute)
                {
                    Console.WriteLine(sData);

                    var tradeSummaryListing = new List<TradeSummary>();

                    //difference in minutes
                    //send of to subscribing apps
                    if (_exchange == "Forex" || _exchange == "FOREX")
                    {
                        //use bid price
                        TradeSummary tradeSumRes = new TradeSummary();
                        tradeSumRes.Open = _dataCollectionCheck.FirstOrDefault().Bid;
                        tradeSumRes.High = _dataCollectionCheck.OrderByDescending(p => p.Bid).FirstOrDefault().Bid;
                        tradeSumRes.Low = _dataCollectionCheck.OrderBy(p => p.Bid).FirstOrDefault().Bid;
                        tradeSumRes.Close = _dataCollectionCheck.LastOrDefault().Bid;

                        tradeSumRes.SymbolID = _dataCollectionCheck.LastOrDefault().SymbolID;
                        
                        tradeSumRes.Exchange = _exchange;
                        tradeSumRes.TimeFrame = _dataCollectionCheck.LastOrDefault().TimeFrame;

                        tradeSumRes.DateTime = new DateTime(tradeSumTemp.DateTime.Year,
                            tradeSumTemp.DateTime.Month,
                            tradeSumTemp.DateTime.Day,
                            tradeSumTemp.DateTime.Hour,
                            tradeSumTemp.DateTime.Minute,
                            0,
                            0);
                       

                        string msg = tradeSumRes.SymbolID + ", " + tradeSumRes.TimeFrame + ", " + string.Format("{0}-{1}-{2} {3}:{4}",
                            tradeSumRes.DateTime.Year,
                            tradeSumRes.DateTime.Month,
                            tradeSumRes.DateTime.Day,
                            tradeSumRes.DateTime.Hour,
                            tradeSumRes.DateTime.Minute);

                        Library.WriteErrorLog(msg);



                        tradeSummaryListing.Add(tradeSumRes);

                        if (tradeSummaryListing.Count > 0)
                            _tradeSummaryListing = tradeSummaryListing;

                        if (tradeSummaryListing.Count > 0 && tradeSummaryListing != null && _updateTradeSummaryHandler != null)
                            _updateTradeSummaryHandler.Invoke(tradeSummaryListing);
                    }
                    else
                    {
                        //other asset classes

                        TradeSummary tradeSumRes = new TradeSummary();
                        tradeSumRes.Open = _dataCollectionCheck.FirstOrDefault().Open;
                        tradeSumRes.High = _dataCollectionCheck.OrderByDescending(p => p.Bid).FirstOrDefault().High;
                        tradeSumRes.Low = _dataCollectionCheck.OrderBy(p => p.Bid).FirstOrDefault().Low;
                        tradeSumRes.Close = _dataCollectionCheck.LastOrDefault().Close;

                        tradeSumRes.SymbolID = _dataCollectionCheck.LastOrDefault().SymbolID;

                        tradeSumRes.Exchange = _exchange;
                        tradeSumRes.TimeFrame = _dataCollectionCheck.LastOrDefault().TimeFrame;

                        tradeSumRes.DateTime = new DateTime(tradeSumTemp.DateTime.Year,
                            tradeSumTemp.DateTime.Month,
                            tradeSumTemp.DateTime.Day,
                            tradeSumTemp.DateTime.Hour,
                            tradeSumTemp.DateTime.Minute,
                            0,
                            0);

                        tradeSumRes.DateTime = RoundToNearestTimeFrame(tradeSumRes.DateTime, tradeSumRes.TimeFrame);

                        tradeSummaryListing.Add(tradeSumRes);

                        if (tradeSummaryListing.Count > 0)
                            _tradeSummaryListing = tradeSummaryListing;

                        if (tradeSummaryListing.Count > 0 && tradeSummaryListing != null && _updateTradeSummaryHandler != null)
                            _updateTradeSummaryHandler.Invoke(tradeSummaryListing);
                    }
                    _dataCollectionCheck.Clear();
                }
            }
            _dataCollectionCheck.Add(tradeSumTemp);

        }

        private DateTime RoundToNearestTimeFrame(DateTime matchDate, string timeframe)
        {
            var dateMatcher = new DateTime();

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

        public void RegisterOnDemandClient(TradeSummaryOnDemandDelegate updateHandler)
        {
            _updateTradeSummaryHandlerOnDemand = updateHandler;
        }

        public void RegisterClient(TradeSummaryDelegate updateHandler)
        {
            _updateTradeSummaryHandler = updateHandler;
        }

        private void MinuteTrigger()
        {
            if (_updateTradeSummaryHandler != null && _currentRealTimeTradeSummary != null)
            {
                //  _updateTradeSummaryHandler.Invoke(_currentRealTimeTradeSummary);
                //No longer used
            }

        }

        private void UpdateRealTimeDataGrid()
        {
            try
            {
                //if (_currentTradeSummary.SymbolID != null)
                //{
                //    using (RealTimeRawDataService.RealTimeRawDataServiceClient dataGridService = new RealTimeRawDataService.RealTimeRawDataServiceClient())
                //    {
                //        dataGridService.Open();

                //        dataGridService.SetTradeSummary(_currentTradeSummary);

                //        _realTimeTradeSummary.Add(_currentTradeSummary);



                //    }
                //}
            }
            catch (InvalidOperationException ex)
            {
                Logger.log.Error("InternalDataFeedService -> UpdateRealTimeDataGrid() :: " + ex.ToString());

                ServiceManager.GlobalhandlerComFailure.Invoke();
            }
            catch (CommunicationObjectAbortedException ex)
            {
                Logger.log.Error("InternalDataFeedService -> UpdateRealTimeDataGrid() :: " + ex.ToString());

                ServiceManager.GlobalhandlerComFailure.Invoke();
            }
            catch (CommunicationObjectFaultedException ex)
            {
                Logger.log.Error("InternalDataFeedService -> UpdateRealTimeDataGrid() :: " + ex.ToString());

                ServiceManager.GlobalhandlerComFailure.Invoke();
            }
            catch (CommunicationException ex)
            {
                Logger.log.Error("InternalDataFeedService -> UpdateRealTimeDataGrid() :: " + ex.ToString());

                ServiceManager.GlobalhandlerComFailure.Invoke();
            }
            catch (Exception ex)
            {
                Logger.log.Error("InternalDataFeedService -> UpdateRealTimeDataGrid() :: " + ex.ToString());

                Console.WriteLine("Error :: " + ex.ToString());
            }
            //finally
            //{
            //    String filename = "Data\\" + _currentTradeSummary.SymbolID + "_test.csv";


            //    using (CsvFileWriter writer = new CsvFileWriter(filename))
            //    {
            //        CsvRow csvRow = new CsvRow();

            //        //String reStr = _currentTradeSummary.DateTime.ToString() + "," + _currentTradeSummary.Open.ToString() + "," + _currentTradeSummary.High + "," + _currentTradeSummary.Low + "," + _currentTradeSummary.Close + "," + _currentTradeSummary.Volume + "," + _currentTradeSummary.Close + "," +_currentTradeSummary.SymbolID + "," + "Ticks";
            //        csvRow.Add(_currentTradeSummary.DateTime.ToString());
            //        csvRow.Add(_currentTradeSummary.Open.ToString());
            //        csvRow.Add(_currentTradeSummary.High.ToString());
            //        csvRow.Add(_currentTradeSummary.Low.ToString());
            //        csvRow.Add(_currentTradeSummary.Close.ToString());

            //        csvRow.Add(_currentTradeSummary.Volume.ToString());
            //        csvRow.Add(_currentTradeSummary.AdjustmentClose.ToString());
            //        csvRow.Add(_currentTradeSummary.SymbolID.ToString());

            //        writer.WriteRow(csvRow);
            //    }
            //}
        }


        private void UpdateRealTimeDataGridFileVersion()
        {
            try
            {
                if (_realTimeTradeSummaryQueue.Count > 0)
                {
                    //var currentTradeSummary = _realTimeTradeSummaryQueue.Dequeue();
                    var currentTradeSummary = _realTimeTradeSummaryQueue.ToList();

                    if (currentTradeSummary != null)
                    {
                        if (_updateTradeSummaryHandler != null)
                        {
                            _updateTradeSummaryHandler.Invoke(currentTradeSummary);
                        }
                    }
                }
            }
            catch (InvalidOperationException ex)
            {
                Logger.log.Error("InternalDataFeedService -> UpdateRealTimeDataGridFileVersion() :: " + ex.ToString());

                ServiceManager.GlobalhandlerComFailure.Invoke();
            }
            catch (CommunicationObjectAbortedException ex)
            {
                Logger.log.Error("InternalDataFeedService -> UpdateRealTimeDataGridFileVersion() :: " + ex.ToString());

                ServiceManager.GlobalhandlerComFailure.Invoke();
            }
            catch (CommunicationObjectFaultedException ex)
            {
                Logger.log.Error("InternalDataFeedService -> UpdateRealTimeDataGridFileVersion() :: " + ex.ToString());

                ServiceManager.GlobalhandlerComFailure.Invoke();
            }
            catch (CommunicationException ex)
            {
                Logger.log.Error("InternalDataFeedService -> UpdateRealTimeDataGridFileVersion() :: " + ex.ToString());

                ServiceManager.GlobalhandlerComFailure.Invoke();
            }
            catch (Exception ex)
            {
                Logger.log.Error("InternalDataFeedService -> UpdateRealTimeDataGridFileVersion() :: " + ex.ToString());

                Console.WriteLine("Error :: " + ex.ToString());
            }
        }
    }
}
