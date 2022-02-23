using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.ServiceModel;
using System.Net;
using System.Configuration;
using System.ServiceModel.Configuration;

namespace Receiver
{
    public delegate void TradeSummaryDelegate(List<TradeSummary> tradeSummary);

    public class RealTimeDataHandler
    {
        private static TradeSummaryDelegate _updateTradeSummaryHandler = null;

        private static DataFeedService.InternalDataFeedServiceClient _client;

        public static String RecipientAppID = "RealTimeRawDataService" + 
            String.Format("{0}{1}{2}", DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

        public RealTimeDataHandler(TradeSummaryDelegate updateHandler, String clientName)
        {
            String name = Dns.GetHostName().ToUpper();
            var modeStr = System.Configuration.ConfigurationManager.AppSettings["MODE"];

            try
            {
                _updateTradeSummaryHandler = updateHandler;

                if ((_client != null))
                {
                    _client.Abort();
                    _client = null;
                }

                BroadcastorCallback cb = new BroadcastorCallback();
                cb.SetHandler(HandleBroadcast);

                System.ServiceModel.InstanceContext context =
                    new System.ServiceModel.InstanceContext(cb);
                _client =
                    new DataFeedService.InternalDataFeedServiceClient(context);

                _client.RegisterClient(clientName);

                LogNotifyer.SendMessage(RecipientAppID + " :: " + name + " " + modeStr + " :: Connection to Internal Feed" + "...Started OK");
            }
            catch (Exception ex)
            {
                LogNotifyer.SendMessage(RecipientAppID + " :: " + name + " " + modeStr + " :: Connection to Internal Feed...FAILED!");

                Console.WriteLine(ex.ToString());
                Logger.log.Error("Failure RegisterClient - InternalDataFeedServiceClient :: " + ex.ToString());
            }
        }

        public List<String> GetSymbolList(String exchange)
        {
            try
            {
                BroadcastorCallback cb = new BroadcastorCallback();
                cb.SetHandler(RealTimeDataHandler.HandleBroadcast);

                List<TradeSummary> tradeSummaryListing = new List<TradeSummary>();

                System.ServiceModel.InstanceContext context =
                   new System.ServiceModel.InstanceContext(cb);

                using (DataFeedService.InternalDataFeedServiceClient internalFeedServiceProxy = new DataFeedService.InternalDataFeedServiceClient(context))
                {
                    return internalFeedServiceProxy.GetSymbolList(exchange).ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Logger.log.Error("GetSymbolList- InternalDataFeedServiceClient :: " + ex.ToString());
            }
            return null;
        }


        public List<TradeSummary> GetIntradayData(String symbol, int interval, String exchange, DateTime beginDate)
        {
            BroadcastorCallback cb = new BroadcastorCallback();
            cb.SetHandler(RealTimeDataHandler.HandleBroadcast);

            List<TradeSummary> tradeSummaryListing = new List<TradeSummary>();

            Console.WriteLine("Retrieving Data :: {0} {1} {2} {3}", symbol, interval, exchange, beginDate.ToString());

            try
            {
                System.ServiceModel.InstanceContext context =
                   new System.ServiceModel.InstanceContext(cb);

                using (DataFeedService.InternalDataFeedServiceClient internalFeedServiceProxy = new DataFeedService.InternalDataFeedServiceClient(context))
                {
                    List<DataFeedService.TradeSummary> list = internalFeedServiceProxy.GetTradeSummaryData(symbol, interval, exchange, beginDate).ToList();

                    AutoMapper.Mapper.CreateMap<DataFeedService.TradeSummary, TradeSummary>();

                    foreach (var item in list)
                    {
                        var tradeSummary = AutoMapper.Mapper.Map<TradeSummary>(item);
                        tradeSummaryListing.Add(tradeSummary);
                    }
                }
                Console.WriteLine("Data Retrieval Successful :: {0}", symbol);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.WriteLine("Retrieval Failure :: {0}", symbol);
                Logger.log.Error("Retrieval Failure  :: " + ex.ToString());
            }          
            return tradeSummaryListing;
        }
        
        public void GetIntradayDataAsynch(String symbol, int interval, String exchange, DateTime beginDate)
        {
            BroadcastorCallback cb = new BroadcastorCallback();
            cb.SetHandler(RealTimeDataHandler.HandleBroadcast);

            List<TradeSummary> tradeSummaryListing = new List<TradeSummary>();

            Console.WriteLine("Requesting Data :: {0} {1} {2} {3}", symbol, interval, exchange, beginDate.ToString());

            try
            {
                System.ServiceModel.InstanceContext context =
                   new System.ServiceModel.InstanceContext(cb);

                using (DataFeedService.InternalDataFeedServiceClient internalFeedServiceProxy = new DataFeedService.InternalDataFeedServiceClient(context))
                {
                    internalFeedServiceProxy.GetTradeSummaryDataAsynch(RecipientAppID, symbol, interval, exchange, beginDate); 
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.WriteLine("Request Failure :: {0}", symbol);
                Logger.log.Error("Retrieval Failure  :: " + ex.ToString());
            }            
        }


        #region "callback services"
        private delegate void HandleBroadcastCallback(object sender, EventArgs e);

        //public static void HandleBroadcast(object sender, EventArgs e)
        public static void HandleBroadcast(object sender, EventArgs e)
        {
            try
            {
                //see if object can be set or created as AnswerPackage
                var eventData = (DataFeedService.EventDataTypeCollection)sender;
                
                AutoMapper.Mapper.CreateMap<DataFeedService.TradeSummary, TradeSummary>();
                
                List<TradeSummary> tradeSumCollection = new List<TradeSummary>();

                foreach  (var item in eventData.EventMessage)
                {
                    var tradeSummary = AutoMapper.Mapper.Map<TradeSummary>(item);
                    tradeSumCollection.Add(tradeSummary);                    
                }
                _updateTradeSummaryHandler.Invoke(tradeSumCollection);
            }
            catch (Exception ex)
            {
                Console.WriteLine( ex.ToString());
                Logger.log.Error("HandleBroadcast Failure  :: " + ex.ToString());
            }
        }
        #endregion

    }

    public class BroadcastorCallback : DataFeedService.IInternalDataFeedServiceCallback
    {
        private System.Threading.SynchronizationContext _syncContext = AsyncOperationManager.SynchronizationContext;

        private EventHandler _broadcastorCallBackHandler;
        public void SetHandler(EventHandler handler)
        {
            this._broadcastorCallBackHandler = handler;
        }

        public void BroadcastToClient(DataFeedService.EventDataTypeCollection eventData)
        {
            _syncContext.Post(new System.Threading.SendOrPostCallback(OnBroadcast), eventData);
        }

        private void OnBroadcast(object eventData)
        {
            this._broadcastorCallBackHandler.Invoke(eventData, null);
        }
    }

}
