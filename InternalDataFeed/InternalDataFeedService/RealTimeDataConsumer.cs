using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Net;

using System.Data.SqlClient;
using System.Data;
using System.Xml;

using System.Timers;
using System.Text.RegularExpressions;

// added for access to RegistryKey
using Microsoft.Win32;
// added for access to socket classes
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.ServiceModel;


namespace InternalDataFeedService
{
    public class exchangeFileInfo
    {
        public String exchangeSymbol;
        public String filePath;
    }

    public class SymbolInfo
    {
        public String Symbol;
        public String Exchange;
    }

    public enum MODE
    {
        Live = 0,
        Test = 1,
        LiveTest = 2
    }



    public class RealTimeDataConsumer
    {
        //public static Queue<SymbolInfo> _SymbolCollection = new Queue<SymbolInfo>();
        public Queue<SymbolInfo> _SymbolCollection = new Queue<SymbolInfo>();

        private List<exchangeFileInfo> _exchangeInfoList = new List<exchangeFileInfo>();
        private String _fileName = "";        
        public static  MODE Mode = MODE.Test;
        public exchangeFileInfo EFileInfo = null;
        private List<IQFeedHistoricDownloader> _iqFeedHistoricalDownloaderList = new List<IQFeedHistoricDownloader>();

        //Used for real time minute trigger market data updates
        //private List<OnDemandDownloader> _onDemandDownloaderList = new List<OnDemandDownloader>();
        private List<IQFeedHistoricDownloader> _onDemandDownloaderList = new List<IQFeedHistoricDownloader>();


        //Used for ondemand data retrieval especially by real time datagrid for synch
        //Asynchronous on demand data retrieval for specific client
        private List<KeyValuePair<String, OnDemandDownloader>> _onDemandDownloaderDictClients = new List<KeyValuePair<String, OnDemandDownloader>>();


        public static String _selectedExchange = "";
        private String _currentDownloadFile = "";
        private String _currentSelector = "";


        //public RealTimeDataConsumer(FaultyCommmunication comDele)
        public RealTimeDataConsumer()
        {
            GetLatestSymbolListQueueBased();
        }

        public void RemoveClients(string clientName)
        {
            var removalItems = new List<int>();

            for (int i = 0; i < _onDemandDownloaderDictClients.Count; i++)
            {
                if (_onDemandDownloaderDictClients[i].Key == clientName)
                {
                    removalItems.Add(i);
                }
            }

            foreach (var item in removalItems)
            {
                if (item < _onDemandDownloaderDictClients.Count)
                {
                    _onDemandDownloaderDictClients[item].Value.CloseSocket();
                    _onDemandDownloaderDictClients.RemoveAt(item);
                }
            }
        }

        public List<TradeSummary> GetTradeSummaryData(String symbol, int interval, String exchange, DateTime beginDate)
        {
            OnDemandDownloader onDemandDownloader = new OnDemandDownloader(symbol, exchange, false);

            return onDemandDownloader.GetTradeSummaryData(symbol, interval, exchange, beginDate);
        }

        public void GetTradeSummaryDataAsynch(String clientName, String symbol, int interval, String exchange, DateTime beginDate, bool singleDataPoint)
        {
            if (_onDemandDownloaderDictClients.Any())
            {
                var ondemand = _onDemandDownloaderDictClients.Where(m => m.Key == clientName && m.Value.CurrentSymbolID == symbol && m.Value.Exchange == exchange);

                if (ondemand.Any() && ondemand != null)
                {
                    var ondemandKVP = ondemand.FirstOrDefault();
                    ondemandKVP.Value.GetTradeSummaryDataAsynch(symbol, interval, exchange, beginDate, singleDataPoint);
                }
                else
                {
                    OnDemandDownloader onDemandDownloader = new OnDemandDownloader(symbol, exchange, false);
                    onDemandDownloader.GetTradeSummaryDataAsynch(symbol, interval, exchange, beginDate, singleDataPoint);
                    _onDemandDownloaderDictClients.Add(new KeyValuePair<String, OnDemandDownloader>(clientName, onDemandDownloader));
                }
            }
            else
            {
                OnDemandDownloader onDemandDownloader = new OnDemandDownloader(symbol, exchange, false);
                onDemandDownloader.GetTradeSummaryDataAsynch(symbol, interval, exchange, beginDate, singleDataPoint);
                _onDemandDownloaderDictClients.Add(new KeyValuePair<String, OnDemandDownloader>(clientName, onDemandDownloader));
            }
        }

        public void GetTradeSummaryDataSingleDataPointAsynch(String clientName, String symbol, int interval, String exchange, DateTime beginDate, bool singleDataPoint)
        {
            OnDemandDownloader onDemandDownloader = new OnDemandDownloader(symbol, exchange, false);

            onDemandDownloader.GetTradeSummaryDataAsynch(symbol, interval, exchange, beginDate, singleDataPoint);

            _onDemandDownloaderDictClients.Add(new KeyValuePair<String, OnDemandDownloader>(clientName, onDemandDownloader));
        }

        public List<String> GetSymbolList(string exchange)
        {
            return _SymbolCollection.Where(j => j.Exchange == exchange).
                Select(m => IQFeedHistoricDownloader.RemoveExtText(m.Symbol, ".FXCM")).ToList();
        }

        public void RealTimeDataConsumptionStart()
        {
            try
            {
                DataFeedTimer.StartDataFeedTimer();

                foreach (var item in _SymbolCollection)
                {
                    String watchList = String.Format("w{0}\r\n", item.Symbol);

                    var iqFeeddownloader = new IQFeedHistoricDownloader(item.Symbol, item.Exchange);
                    iqFeeddownloader.Watch(item.Symbol);

                    //OnDemandDownloader iqFeeddownloader = new OnDemandDownloader(item.Symbol, item.Exchange, true);
                    _onDemandDownloaderList.Add(iqFeeddownloader);
                }
            }
            catch (InvalidOperationException ex)
            {
                Logger.log.Error("RealTimeDataConsumer -> RealTimeDataConsumptionStart() :: " + ex.ToString());

                throw;
            }
            catch (CommunicationObjectAbortedException ex)
            {
                Logger.log.Error("RealTimeDataConsumer -> RealTimeDataConsumptionStart() :: " + ex.ToString());

                throw;
            }
            catch (CommunicationObjectFaultedException ex)
            {
                Logger.log.Error("RealTimeDataConsumer -> RealTimeDataConsumptionStart() :: " + ex.ToString());

                throw;
            }
            catch (CommunicationException ex)
            {
                Logger.log.Error("RealTimeDataConsumer -> RealTimeDataConsumptionStart() :: " + ex.ToString());

                throw;
            }
            catch (Exception ex)
            {
                Logger.log.Error("RealTimeDataConsumer -> RealTimeDataConsumptionStart() :: " + ex.ToString());

                throw;
            }
        }

        private string GetExchangeLookUp(string symbolID)
        {
            String exchange = "";
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
                    String query = "proc_GetExchangeLookUp";

                    SqlCommand sqlCommand = new SqlCommand(query, c);
                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    sqlCommand.Parameters.AddWithValue("@keywordvar", symbolID.ToLower());

                    SqlDataReader reader = sqlCommand.ExecuteReader();

                    while (reader.Read())
                    {
                        exchange = reader["Exchange"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return exchange;
        }

        private bool GetLatestSymbolListQueueBased()
        {
            bool bSuccess = true;
            _SymbolCollection.Clear();

            if (_selectedExchange != "")
            {
                exchangeFileInfo eFileInfo = new exchangeFileInfo();
                eFileInfo.exchangeSymbol = _selectedExchange;
                //eFileInfo.filePath = _selectedExchange + ".txt";

                eFileInfo.filePath = Directory.GetCurrentDirectory() + "\\" + _selectedExchange + ".txt";
                _exchangeInfoList.Add(eFileInfo);
            }
            else
            {
                String[] tempAPP = null;

                if (System.Configuration.ConfigurationManager.AppSettings["DATAFEED_MODE"] == "FILE")
                    tempAPP = new String[] { "FOREXShort.txt" };
                else
                {
                   tempAPP = System.Configuration.ConfigurationManager.AppSettings["DATALIST"].Split('|');
                }

                foreach (var item in tempAPP)
                {
                    exchangeFileInfo eFileInfo = new exchangeFileInfo();
                    eFileInfo.exchangeSymbol = "";

                    eFileInfo.filePath = item;
                    //eFileInfo.filePath = Directory.GetCurrentDirectory() + "\\" + item;
                    _exchangeInfoList.Add(eFileInfo);
                }
            }

            //if one of the files cant be found the program will not continue, improve code

            //file read
            try
            {
                // Create an instance of StreamReader to read from a file.
                // The using statement also closes the StreamReader.

                foreach (var item in _exchangeInfoList)
                {
                    //temp file location in future will be with exe
                    using (StreamReader sr = new StreamReader(item.filePath))
                    {
                        string line;
                        // Read and display lines from the file until the end of 
                        // the file is reached.

                        //Queue<String> symbols = new Queue<String>();
                        while ((line = sr.ReadLine()) != null)
                        {
                            //Console.WriteLine(line);
                            if (String.IsNullOrEmpty(line) == false)
                            {
                                //symbols.Enqueue(line);
                                SymbolInfo symInfo = new SymbolInfo();
                                symInfo.Symbol = line;
                                symInfo.Exchange = GetExchangeLookUp(line); 

                                _SymbolCollection.Enqueue(symInfo);
                            }
                        }
                    }
                }
            }
            catch (InvalidOperationException ex)
            {
                Logger.log.Error("RealTimeDataConsumer -> GetLatestSymbolListQueueBased() :: " + ex.ToString());

                throw;
            }
            catch (CommunicationObjectAbortedException ex)
            {
                Logger.log.Error("RealTimeDataConsumer -> GetLatestSymbolListQueueBased() :: " + ex.ToString());

                throw;
            }
            catch (CommunicationObjectFaultedException ex)
            {
                Logger.log.Error("RealTimeDataConsumer -> GetLatestSymbolListQueueBased() :: " + ex.ToString());

                throw;
            }
            catch (CommunicationException ex)
            {
                Logger.log.Error("RealTimeDataConsumer -> GetLatestSymbolListQueueBased() :: " + ex.ToString());

                throw;
            }
            catch (Exception e)
            {
                // Let the user know what went wrong.
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
                bSuccess = false;
            }
            return bSuccess;
        }

        public void RegisterClient(TradeSummaryDelegate updateHandler)
        {
            foreach (var iqFeedItem in _onDemandDownloaderList)
            {
                iqFeedItem.RegisterClient(updateHandler);
            }
        }

        public void RegisterOnDemandClient(TradeSummaryOnDemandDelegate updateHandler)
        {
            foreach (var ondemandItem in _onDemandDownloaderDictClients)
            {
                ondemandItem.Value.OnDemandClientName = ondemandItem.Key;
                ondemandItem.Value.RegisterOnDemandClient(updateHandler);
            }
        }

        public void WorkCompleteFreeResources(string clientName)
        {
            RemoveClients(clientName);
        }
    }


    public static class TradeUtility
    {
        public static Dictionary<String, String> _ForexSymbolLookUp = new Dictionary<String, String>();

        static TradeUtility()
        {
            var path = System.Configuration.ConfigurationManager.AppSettings["ForexList"];

            using (StreamReader sr = new StreamReader(path))
            {
                string line;

                while ((line = sr.ReadLine()) != null)
                {
                    if (String.IsNullOrEmpty(line) == false)
                    {
                        var symbolArray = line.Split('/');
                        var key = symbolArray[0] + symbolArray[1];
                        _ForexSymbolLookUp.Add(key, line);
                    }
                }
            }
        }

        public static String ConvertSymbolIntoFriendlyForm(String sym)
        {
            string friendlyVersion = "";

            _ForexSymbolLookUp.TryGetValue(sym, out friendlyVersion);

            return friendlyVersion;
        }
    }






}
