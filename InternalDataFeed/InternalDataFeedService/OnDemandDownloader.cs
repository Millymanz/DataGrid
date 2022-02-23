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
using log4net;
using System.ServiceModel;



namespace InternalDataFeedService
{
    public delegate void TradeSummaryOnDemandDelegate(String clientName, List<TradeSummary> tradeSummary);

    public class OnDemandDownloader
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



        // delegate for updating the data display.
        public delegate void UpdateDataHandler(string sMessage);

        private String _fileName;
        private Queue<AdditionalData> requestedSymbolQueue = new Queue<AdditionalData>();

        //private List<RealTimeRawDataService.TradeSummary> _realTimeTradeSummary = new List<RealTimeRawDataService.TradeSummary>();
        private Queue<TradeSummary> _realTimeTradeSummaryQueue = new Queue<TradeSummary>();
        private static TradeSummaryDelegate _updateTradeSummaryHandler = null;
        private static TradeSummaryOnDemandDelegate _updateTradeSummaryHandlerOnDemand = null;
        public String OnDemandClientName { get; set; }

        //Used with real time data feed
        private TradeSummary _currentRealTimeTradeSummary = null;
        private List<TradeSummary> _tradeSummaryListing = new List<TradeSummary>();


        private Object thisLock = new Object();
        public bool IsLocked = false;

        public static bool bResumeSendRequest = false;

        private String _currentSymbolID = "";
        private String _exchange = "";
        private String _timeFrame = "";
        private String _path = "";

        private bool _bFirstTime = true;

        bool _timerInUse = false;

        public String OutputFileName
        {
            get { return _fileName; }
            set { _fileName = value; }
        }

        public string CurrentSymbolID
        {
            get { return _currentSymbolID; }
            set { _currentSymbolID = value; }
        }

        public string Exchange
        {
            get { return _exchange; }
            set { _exchange = value;  }
        }

        System.Timers.Timer actionTimer;

        int TimeInterval = 0;


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

            actionTimer.AutoReset = true;//Enable resetting run at this interval all the time..newly added

            actionTimer.Enabled = true;
            actionTimer.Start();

            _bFirstTime = false;

            try
            {
                //throw new CommunicationException();
            }
            catch (CommunicationException ex)
            {
                Logger.log.Error("OnDemandDownloader -> Task() :: " + ex.ToString());

                Library.WriteErrorLog(DateTime.Now + " :: "+ ex.ToString());
                ServiceManager.GlobalhandlerComFailure.Invoke();
            }
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

        public OnDemandDownloader(String currentSymbol, String exchange, bool timer)
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
            _timerInUse = timer;

            if (timer)
            {
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
                    InitalizeSocket();
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
                actionTimer.AutoReset = false; //Changed to false

                actionTimer.Enabled = true;
                actionTimer.Start();
            }
            else
            {
                InitalizeSocket();
            }
        }
        
        public void CloseSocket()
        {
            try
            {
                m_sockLookup.Close(5000);
            }
            catch (Exception ex)
            {
                Library.WriteErrorLog(DateTime.Now + " : " + ex.ToString());
            }
        }

        private void InitalizeSocket()
        {
            try
            {
                // create the socket
                m_sockLookup = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPAddress ipLocalhost = IPAddress.Parse("127.0.0.1");

                // Historical data is received from IQFeed on the Lookup port.
                // pull the Lookup port out of the registry
                int iPort = GetIQFeedPort("Lookup");
                IPEndPoint ipendLocalhost = new IPEndPoint(ipLocalhost, iPort);


                // connect the socket
                m_sockLookup.Connect(ipendLocalhost);

                // Set the protocol for the lookup socket to 5.0
                SendRequestToIQFeed("S,SET PROTOCOL,5.0\r\n");
            }
            catch (InvalidOperationException ex)
            {
                Logger.log.Error("OnDemandDownloader -> InitalizeSocket() :: " + ex.ToString());
                Library.WriteErrorLog(DateTime.Now + " :: " + ex.ToString());

                ServiceManager.GlobalhandlerComFailure.Invoke();
            }
            catch (CommunicationObjectAbortedException ex)
            {
                Logger.log.Error("OnDemandDownloader -> InitalizeSocket() :: " + ex.ToString());
                Library.WriteErrorLog(DateTime.Now + " :: " + ex.ToString());


                ServiceManager.GlobalhandlerComFailure.Invoke();
            }
            catch (CommunicationObjectFaultedException ex)
            {
                Logger.log.Error("OnDemandDownloader -> InitalizeSocket() :: " + ex.ToString());
                Library.WriteErrorLog(DateTime.Now + " :: " + ex.ToString());

                ServiceManager.GlobalhandlerComFailure.Invoke();
            }
            catch (CommunicationException ex)
            {
                Logger.log.Error("OnDemandDownloader -> InitalizeSocket() :: " + ex.ToString());
                Library.WriteErrorLog(DateTime.Now + " :: " + ex.ToString());

                ServiceManager.GlobalhandlerComFailure.Invoke();
            }
            catch (SocketException ex)
            {
                Console.WriteLine(String.Format("Oops.  Did you forget to Login first?\nTake a Look at the LaunchingTheFeed example app\n{0}", ex.Message), "Error Connecting to IQFeed");
                Logger.log.Error("IQFeed Not Started :: " + ex.Message);
                Library.WriteErrorLog(DateTime.Now + " :: " + ex.ToString());

            }
        }

        private String RemoveExtText(String symbolCriteria, String extension)
        {
            var splitArray = Regex.Split(symbolCriteria, extension);
            return splitArray.FirstOrDefault();
        }

        public List<TradeSummary> GetTradeSummaryData(String symbol, int interval, String exchange, DateTime beginDate)
        {
            string sRequest = "";

            var dateStr = beginDate.ToString("yyyyMMdd");

            switch (interval)
            {
                case 60:
                    {
                        _timeFrame = "1min";
                    } break;

                case 120:
                    {
                        _timeFrame = "2min";
                    } break;

                case 180:
                    {
                        _timeFrame = "3min";
                    } break;

                case 240:
                    {
                        _timeFrame = "4min";
                    } break;

                case 300:
                    {
                        _timeFrame = "5min";
                    } break;

                case 360:
                    {
                        _timeFrame = "6min";
                    } break;

                case 600:
                    {
                        _timeFrame = "10min";
                    } break;

                case 900:
                    {
                        _timeFrame = "15min";
                    } break;

                case 1800:
                    {
                        _timeFrame = "30min";
                    } break;

                case 3600:
                    {
                        _timeFrame = "1hour";
                    } break;

                case 7200:
                    {
                        _timeFrame = "2hour";
                    } break;

                case 10800:
                    {
                        _timeFrame = "3hour";
                    } break;

                case 14400:
                    {
                        _timeFrame = "4hour";
                    } break;

                case 86400:
                    {
                        _timeFrame = "EndOfDay";
                    } break;
            }

            if (exchange == "Forex" || exchange == "FOREX")
            {
                RemoveExtText(symbol, ".FXCM");
                symbol = symbol + ".FXCM";
            }

            // request in the format:
            // HIT,SYMBOL,INTERVAL,BEGINDATE BEGINTIME,ENDDATE ENDTIME,MAXDATAPOINTS,BEGINFILTERTIME,ENDFILTERTIME,DIRECTION,REQUESTID,DATAPOINTSPERSEND,INTERVALTYPE<CR><LF>
            
            sRequest = String.Format("HIT,{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10}\r\n", symbol, interval, dateStr, "", "", "", "", "1", "", "", "s");

            SendRequestToIQFeed(sRequest);
            WaitForData("History");

            //not ideal change how this works later
            //while (bWaitForDataCompletion) { };

            return _tradeSummaryListing;
        }

        public void GetTradeSummaryDataAsynch(String symbol, int interval, String exchange, DateTime beginDate, bool singleDataPoint)
        {
            string sRequest = "";

            var dateStr = beginDate.ToString("yyyyMMdd");

            switch (interval)
            {
                case 60:
                    {
                        _timeFrame = "1min";
                    } break;

                case 120:
                    {
                        _timeFrame = "2min";
                    } break;

                case 180:
                    {
                        _timeFrame = "3min";
                    } break;

                case 240:
                    {
                        _timeFrame = "4min";
                    } break;

                case 300:
                    {
                        _timeFrame = "5min";
                    } break;

                case 360:
                    {
                        _timeFrame = "6min";
                    } break;

                case 600:
                    {
                        _timeFrame = "10min";
                    } break;

                case 900:
                    {
                        _timeFrame = "15min";
                    } break;

                case 1800:
                    {
                        _timeFrame = "30min";
                    } break;

                case 3600:
                    {
                        _timeFrame = "1hour";
                    } break;

                case 7200:
                    {
                        _timeFrame = "2hour";
                    } break;

                case 10800:
                    {
                        _timeFrame = "3hour";
                    } break;

                case 14400:
                    {
                        _timeFrame = "4hour";
                    } break;

                case 86400:
                    {
                        _timeFrame = "EndOfDay";
                    } break;
            }

            if (exchange == "Forex" || exchange == "FOREX")
            {
                RemoveExtText(symbol, ".FXCM");
                symbol = symbol + ".FXCM";
            }

            if (_timeFrame == "EndOfDay")
            {
                // HDT,SYMBOL,BEGINDATE,ENDDATE,MAXDATAPOINTS,DIRECTION,REQUESTID,DATAPOINTSPERSEND<CR><LF>
                if (singleDataPoint)
                {
                    sRequest = String.Format("HDT,{0},{1},{2},{3},{4},{5},{6}\r\n", symbol, dateStr, "", 1, 1, "", "");
                }
                else
                {
                    sRequest = String.Format("HDT,{0},{1},{2},{3},{4},{5},{6}\r\n", symbol, dateStr, "", "", 1, "", "");
                }
            }
            else
            {
                // request in the format:
                // HIT,SYMBOL,INTERVAL,BEGINDATE BEGINTIME,ENDDATE ENDTIME,MAXDATAPOINTS,BEGINFILTERTIME,ENDFILTERTIME,DIRECTION,REQUESTID,DATAPOINTSPERSEND,INTERVALTYPE<CR><LF>
                if (singleDataPoint)
                {
                    sRequest = String.Format("HIT,{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10}\r\n", symbol, interval, dateStr, "", 1, "", "", "1", "", "", "s");
                }
                else
                {
                    sRequest = String.Format("HIT,{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10}\r\n", symbol, interval, dateStr, "", "", "", "", "1", "", "", "s");
                }
            }
            SendRequestToIQFeed(sRequest);
            WaitForData("History");
        }

        public void GetTradeSummaryData(String symbol, int interval, String exchange, DateTime beginTime, DateTime beginDate)
        {
            //This is used for Real time data retrieval
            var beginTimeStr = beginTime.ToString("HHmm");
            var endTimeStr = beginDate.ToString("HHmm");

            string sRequest = "";

            var dateStr = beginDate.ToString("yyyyMMdd");

            switch (interval)
            {
                case 60:
                    {
                        _timeFrame = "1min";
                    } break;

                case 120:
                    {
                        _timeFrame = "2min";
                    } break;

                case 180:
                    {
                        _timeFrame = "3min";
                    } break;

                case 240:
                    {
                        _timeFrame = "4min";
                    } break;

                case 300:
                    {
                        _timeFrame = "5min";
                    } break;

                case 360:
                    {
                        _timeFrame = "6min";
                    } break;

                case 600:
                    {
                        _timeFrame = "10min";
                    } break;

                case 900:
                    {
                        _timeFrame = "15min";
                    } break;

                case 1800:
                    {
                        _timeFrame = "30min";
                    } break;

                case 3600:
                    {
                        _timeFrame = "1hour";
                    } break;

                case 7200:
                    {
                        _timeFrame = "2hour";
                    } break;

                case 10800:
                    {
                        _timeFrame = "3hour";
                    } break;

                case 14400:
                    {
                        _timeFrame = "4hour";
                    } break;

                case 86400:
                    {
                        _timeFrame = "EndOfDay";
                    } break;
            }

            symbol = RemoveExtText(symbol, ".FXCM") + ".FXCM";

            // request in the format:
            // HIT,SYMBOL,INTERVAL,BEGINDATE BEGINTIME,ENDDATE ENDTIME,MAXDATAPOINTS,BEGINFILTERTIME,ENDFILTERTIME,DIRECTION,REQUESTID,DATAPOINTSPERSEND,INTERVALTYPE<CR><LF>
            sRequest = String.Format("HIT,{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10}\r\n", 
                symbol, interval, dateStr, "", "2", "", "", "1", "", "", "s");


            SendRequestToIQFeed(sRequest);
            WaitForData("History");
        }

        private void SendRequestToIQFeed(string sCommand)
        {
            try
            {
            // and we send it to the feed via the socket
            byte[] szCommand = new byte[sCommand.Length];
            szCommand = Encoding.ASCII.GetBytes(sCommand);
            int iBytesToSend = szCommand.Length;

                int iBytesSent = m_sockLookup.Send(szCommand, iBytesToSend, SocketFlags.None);

                if (iBytesSent != iBytesToSend)
                {
                    UpdateTradeSummaryCollection(String.Format("Error Sending Request:\r\n{0}", sCommand.TrimEnd("\r\n".ToCharArray())));
                }
                else
                {
                    UpdateTradeSummaryCollection(String.Format("Request Sent Successfully:\r\n{0}", sCommand.TrimEnd("\r\n".ToCharArray())));
                }                
            }
            catch (InvalidOperationException ex)
            {
                Logger.log.Error("OnDemandDownloader -> SendRequestToIQFeed() :: " + ex.ToString());
                Library.WriteErrorLog(DateTime.Now + " :: " + ex.ToString());

                ServiceManager.GlobalhandlerComFailure.Invoke();
            }
            catch (CommunicationObjectAbortedException ex)
            {
                Logger.log.Error("OnDemandDownloader -> SendRequestToIQFeed() :: " + ex.ToString());
                Library.WriteErrorLog(DateTime.Now + " :: " + ex.ToString());

                ServiceManager.GlobalhandlerComFailure.Invoke();
            }
            catch (CommunicationObjectFaultedException ex)
            {
                Logger.log.Error("OnDemandDownloader -> SendRequestToIQFeed() :: " + ex.ToString());
                Library.WriteErrorLog(DateTime.Now + " :: " + ex.ToString());

                ServiceManager.GlobalhandlerComFailure.Invoke();
            }
            catch (CommunicationException ex)
            {
                Logger.log.Error("OnDemandDownloader -> SendRequestToIQFeed() :: " + ex.ToString());
                Library.WriteErrorLog(DateTime.Now + " :: " + ex.ToString());

                ServiceManager.GlobalhandlerComFailure.Invoke();
            }
            catch (SocketException ex)
            {
                Logger.log.Error("Socket Connection Problem whilst Sending Request :: " + ex.Message);
                Library.WriteErrorLog(DateTime.Now + " :: " + ex.ToString());

                // handle socket errors
                UpdateTradeSummaryCollection(String.Format("Socket Error Sending Request:\r\n{0}\r\n{1}", sCommand.TrimEnd("\r\n".ToCharArray()), ex.Message));
            }
        }


        private void OnReceive(IAsyncResult asyn)
        {
            try
            {

                // first verify we received data from the correct socket.  This check isn't really necessary in this example since we 
                // only have a single socket but if we had multiple sockets, we could use this check to use the same callback to recieve data from
                // multiple sockets
                if (asyn.AsyncState.ToString().Equals("History"))
                {
                    // read data from the socket.  The call to EndReceive tells the Framework to copy data available on the socket into our socket buffer
                    // that we supplied in the BeginReceive call.
                    int iReceivedBytes = 0;
                    iReceivedBytes = m_sockLookup.EndReceive(asyn);
                    m_bLookupNeedBeginReceive = true;
                    // add the data received from the socket to any data that was left over from the previous read off the socket.
                    string sData = m_sLookupIncompleteRecord + Encoding.ASCII.GetString(m_szLookupSocketBuffer, 0, iReceivedBytes);
                    // clear the incomplete record string so it doesn't get added again next time we read from the socket
                    m_sLookupIncompleteRecord = "";
                    // history data will be read off the socket in groups of messages.  We have no control over how many messages will be
                    // read off the socket at each read.  Likewise we have no guarantee that we won't get an incomplete message at the beginning
                    // or ending of the group of messages.  Our processing needs to handle this.
                    // history data is always terminated with a cariage return and newline characters ("\r\n").  
                    // we verify a record is complete by finding the newline character.
                    int iNewLinePos = sData.IndexOf("\n");
                    int iPos = 0;
                    // loop through the group of messages
                    while (iNewLinePos >= 0)
                    {
                        // at this point, we know we have a complete message between iPos (start of the message) and iNewLinePos (end)
                        // here we could add message processing for this single line of data but in this example, we just display the raw data
                        // so we just keep looping through the messages
                        iPos = iNewLinePos + 1;
                        iNewLinePos = sData.IndexOf("\n", iPos);
                    }

                    UpdateTradeSummaryCollection(sData);

                    // at this point, iPos (start of the current message) will be less than m_strData.Length if we had an incomplete message
                    // at the end of the data.  We detect this and save off the incomplete message
                    if (sData.Length > iPos)
                    {
                        // left an incomplete record in the buffer
                        m_sLookupIncompleteRecord = sData.Substring(iPos);
                        // remove the incomplete message from the message
                        sData = sData.Remove(iPos);
                    }
                    else if (sData.EndsWith("!ENDMSG!,\r\n"))
                    {
                        // end of message.
                        if (String.IsNullOrEmpty(OnDemandClientName) == false && _timerInUse == false)
                        {
                            //temp solution hack
                            var tradeSumCompletedNotice = new List<TradeSummary>() { };
                            TradeSummary tempTram = new TradeSummary();
                            tempTram.SymbolID = _currentSymbolID;
                            tempTram.TimeFrame = _timeFrame;
                            tempTram.Exchange = "SynchComplete";
                            tradeSumCompletedNotice.Add(tempTram);

                            _updateTradeSummaryHandlerOnDemand.Invoke(OnDemandClientName, tradeSumCompletedNotice);
                        }
                        else
                        {
                            if (_timerInUse)
                            {
                                _updateTradeSummaryHandler.Invoke(_tradeSummaryListing);
                            }
                        }
                    }
                    // clear the m_strData to verify it is empty for the next read off the socket
                    sData = "";

                    // call wait for data to notify the socket that we are ready to receive another callback
                    WaitForData("History");
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.ToString());
                //Logger.log.Error(ex.ToString());

                //ServiceManager.GlobalhandlerComFailure.Invoke();
            }
        }

        private void WaitForData(string sSocketName)
        {
            try
            {
                if (sSocketName.Equals("History"))
                {
                    // make sure we have a callback created
                    if (m_pfnLookupCallback == null)
                    {
                        m_pfnLookupCallback = new AsyncCallback(OnReceive);
                    }

                    // send the notification to the socket.  It is very important that we don't call Begin Reveive more than once per call
                    // to EndReceive.  As a result, we set a flag to ignore multiple calls.
                    if (m_bLookupNeedBeginReceive)
                    {
                        m_bLookupNeedBeginReceive = false;
                        // we pass in the sSocketName in the state parameter so that we can verify the socket data we receive is the data we are looking for
                        m_sockLookup.BeginReceive(m_szLookupSocketBuffer, 0, m_szLookupSocketBuffer.Length, SocketFlags.None, m_pfnLookupCallback, sSocketName);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Logger.log.Error(ex.ToString());
                Library.WriteErrorLog(DateTime.Now + " :: " + ex.ToString());


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
            UpdateTradeSummaryCollection(sLine);
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
            UpdateTradeSummaryCollection(sLine);
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
            UpdateTradeSummaryCollection(sLine);
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
            UpdateTradeSummaryCollection(sLine);
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
            UpdateTradeSummaryCollection(sLine);
        }

        /// <summary>
        /// Process a timestamp message from the feed.
        /// </summary>
        /// <param name="sLine"></param>
        private void ProcessTimestamp(string sLine)
        {
            // Timestamp messages are sent to the client once a second.  These timestamps are generated by our servers and can be used as a "server time"
            // In this example, we just display the data to the user.
            UpdateTradeSummaryCollection(sLine);
        }

        /// <summary>
        /// Process an error message from the feed
        /// </summary>
        /// <param name="sLine"></param>
        private void ProcessErrorMsg(string sLine)
        {
            // Error messages are sent to the client to inform the client of problems.
            // In this example, we just display the data to the user.
            UpdateTradeSummaryCollection(sLine);
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
            UpdateTradeSummaryCollection(sLine);
        }

        /// <summary>
        /// we have to use a delegate to update controls in the dialog to resolve cross-threading issues built into the .NET framework
        /// </summary>
        /// <param name="sData"></param>
        public void UpdateTradeSummaryCollection(string sData)
        {
            var Str = sData.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            Console.WriteLine(sData);

            List<TradeSummary> tradeSummaryListing = new List<TradeSummary>();

            String[] StrArray = sData.Split(',');

            DateTime testDate = new DateTime();

            List<String> lstMessages = new List<string>(sData.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries));
            lstMessages.ForEach(delegate(String sLine)
            {
                try
                {
                    String[] results = sLine.Split(',');

                    var item = results[0].Split(',');

                    if (results.Length >= 5 && results.Length < 9)
                    {
                        var tempSym = _currentSymbolID + ".FXCM";
                        ///DateTime,High,Low,Open,Close,Volume,AdjustmentClose,Symbol,TimeFrame

                        if (String.IsNullOrEmpty(results[0]) == false &&
                            String.IsNullOrEmpty(results[1]) == false &&
                            String.IsNullOrEmpty(results[2]) == false &&
                            String.IsNullOrEmpty(results[3]) == false &&
                            String.IsNullOrEmpty(results[4]) == false)
                        {

                            TradeSummary tradeSummary = new TradeSummary();

                            tradeSummary.DateTime = DateTime.Parse(results[0]);
                            tradeSummary.High = Convert.ToDouble(results[1]);
                            tradeSummary.Low = Convert.ToDouble(results[2]);
                            tradeSummary.Open = Convert.ToDouble(results[3]);
                            tradeSummary.Close = Convert.ToDouble(results[4]);

                            if (results.Count() >= 6)
                            {
                                if (String.IsNullOrEmpty(results[5]) == false)
                                    tradeSummary.Volume = Convert.ToInt32(results[5]);
                            }

                            tradeSummary.TimeFrame = _timeFrame;
                            tradeSummary.Exchange = _exchange;

                            tradeSummary.AdjustmentClose = tradeSummary.Close;

                            String symbolIDTemp = _currentSymbolID;

                            if (_exchange == "Forex" || _exchange == "FOREX")
                            {
                                symbolIDTemp = RemoveExtText(symbolIDTemp, ".FXCM");
                            }
                            tradeSummary.SymbolID = symbolIDTemp;

                            _currentRealTimeTradeSummary = tradeSummary;

                            tradeSummaryListing.Add(tradeSummary);
                        }
                    }
                }
                catch (InvalidOperationException ex)
                {
                    Logger.log.Error("OnDemandDownloader -> UpdateTradeSummaryCollection() :: " + ex.ToString());
                    Library.WriteErrorLog(DateTime.Now + " :: " + ex.ToString());

                    ServiceManager.GlobalhandlerComFailure.Invoke();
                }
                catch (CommunicationObjectAbortedException ex)
                {
                    Logger.log.Error("OnDemandDownloader -> UpdateTradeSummaryCollection() :: " + ex.ToString());
                    Library.WriteErrorLog(DateTime.Now + " :: " + ex.ToString());

                    ServiceManager.GlobalhandlerComFailure.Invoke();
                }
                catch (CommunicationObjectFaultedException ex)
                {
                    Logger.log.Error("OnDemandDownloader -> UpdateTradeSummaryCollection() :: " + ex.ToString());
                    Library.WriteErrorLog(DateTime.Now + " :: " + ex.ToString());

                    ServiceManager.GlobalhandlerComFailure.Invoke();
                }
                catch (CommunicationException ex)
                {
                    Logger.log.Error("OnDemandDownloader -> UpdateTradeSummaryCollection() :: " + ex.ToString());
                    Library.WriteErrorLog(DateTime.Now + " :: " + ex.ToString());

                    ServiceManager.GlobalhandlerComFailure.Invoke();
                }
                catch (Exception ex)
                {
                    Logger.log.Error("Error whilst Populating TradeSummary Object :: " + ex.Message);
                    Library.WriteErrorLog(DateTime.Now + " :: " + ex.ToString());

                    Console.WriteLine(ex.ToString());
                }
            });

            if (tradeSummaryListing.Count > 0)
                _tradeSummaryListing = tradeSummaryListing;

            if (tradeSummaryListing.Count > 0 && tradeSummaryListing != null && _updateTradeSummaryHandlerOnDemand != null)
                _updateTradeSummaryHandlerOnDemand.Invoke(OnDemandClientName, tradeSummaryListing);

        } 

        private void MinuteTrigger()
        {
            if (_updateTradeSummaryHandler != null)
            {
                //use this for minute updates
                var usaTimeWhereFeedOriginates = DateTime.Now.AddHours(-5);//New york
                GetTradeSummaryData(_currentSymbolID, 60, "Forex", usaTimeWhereFeedOriginates.AddMinutes(-3), usaTimeWhereFeedOriginates);
            }
        }

        public void RegisterOnDemandClient(TradeSummaryOnDemandDelegate updateHandler)
        {
            _updateTradeSummaryHandlerOnDemand = updateHandler;
        }

        public void RegisterClient(TradeSummaryDelegate updateHandler)
        {
            _updateTradeSummaryHandler = updateHandler;
        }

        private void UpdateRealTimeDataGridFileVersion()
        {
            try
            {
                if (_realTimeTradeSummaryQueue.Count > 0)
                {
                    var temp = _realTimeTradeSummaryQueue.Dequeue();
                    List<TradeSummary> currentTradeSummary = new List<TradeSummary>() {temp };
                   // var currentTradeSummary = _realTimeTradeSummaryQueue.ToList();

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
                ServiceManager.GlobalhandlerComFailure.Invoke();
            }
            catch (CommunicationObjectAbortedException ex)
            {
                ServiceManager.GlobalhandlerComFailure.Invoke();
            }
            catch (CommunicationObjectFaultedException ex)
            {
                ServiceManager.GlobalhandlerComFailure.Invoke();
            }
            catch (CommunicationException ex)
            {
                ServiceManager.GlobalhandlerComFailure.Invoke();
            }
            catch (Exception ex)
            {
                Logger.log.Error("Error whilst Invoking Delegate Object for handling TradeSummary Object :: " + ex.Message);

                Console.WriteLine("Error :: " + ex.ToString());
            }
        }
    }
}
