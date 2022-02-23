using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace InternalDataFeedService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in both code and config file together.

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class InternalDataFeedService : IInternalDataFeedService
    {
        private RealTimeDataConsumer _realTimeDataConsumer = null;
        FaultyCommmunication _faultyCom = null;

        private IQConnect m_IQConnet = new IQConnect();

        //public InternalDataFeedService(FaultyCommmunication comDele)
        public InternalDataFeedService()
        {
            

            //var modeStr = System.Configuration.ConfigurationManager.AppSettings["MODE"];
            //String name = Dns.GetHostName().ToUpper();
            //ServiceManager serviceMgr = new ServiceManager(name, modeStr);
            //serviceMgr.Start();


            _realTimeDataConsumer = new RealTimeDataConsumer();

            var realtimeRun = System.Configuration.ConfigurationManager.AppSettings["REALTIME_RUN"];

            if (realtimeRun == "TRUE")
            {
                InitializeDataFeed();

                System.Threading.Thread.Sleep(10000);

                var task1 = Task.Factory.StartNew(() =>
                {
                    _realTimeDataConsumer.RealTimeDataConsumptionStart();
                });

                try
                {
                    //this is done so we can capture any exceptions which are in other threads
                    task1.Wait();

                }
                catch (AggregateException ae)
                {
                    Logger.log.Error("InternalDataFeedService() constructor :: " + ae.ToString());
                    throw new CommunicationException(ae.Message);
                }
            }
            else
            {
                Console.Title += " :: ONDEMAND ONLY";
            }
        }

        private void InitializeDataFeed()
        {
            m_IQConnet.InitializeDataFeed();
        }

        public string GetData(int value)
        {
            return string.Format("You entered: {0}", value);
        }

        public CompositeType GetDataUsingDataContract(CompositeType composite)
        {
            if (composite == null)
            {
                throw new ArgumentNullException("composite");
            }
            if (composite.BoolValue)
            {
                composite.StringValue += "Suffix";
            }
            return composite;
        }

        public List<String> GetSymbolList(String exchange)
        {
            return _realTimeDataConsumer.GetSymbolList(exchange);
        }

        public List<TradeSummary> GetTradeSummaryData(String symbol, int interval, String exchange, DateTime beginDate)
        {
            Library.WriteErrorLog(DateTime.Now + " :: "+ symbol + " : " + interval + " : " + exchange + " : " + beginDate.ToString());

            return _realTimeDataConsumer.GetTradeSummaryData(symbol, interval, exchange, beginDate);
        }

        public void GetTradeSummaryDataAsynch(String clientName, String symbol, int interval, String exchange, DateTime beginDate)
        {
            Library.WriteErrorLog(DateTime.Now + " :: " + clientName + " : " + symbol + " : " + interval + " : " + exchange + " : " + beginDate.ToString());

            var ondemandClientUpdate = new TradeSummaryOnDemandDelegate(OnDemandUpdateClients);

            _realTimeDataConsumer.GetTradeSummaryDataAsynch(clientName, symbol, interval, exchange, beginDate, false);

            _realTimeDataConsumer.RegisterOnDemandClient(ondemandClientUpdate);
        }

        public void GetTradeSummaryDataSingleDataPointAsynch(String clientName, String symbol, int interval, String exchange, DateTime beginDate)
        {
            Library.WriteErrorLog(DateTime.Now + " :: " + clientName + " : " + symbol + " : " + interval + " : " + exchange + " : " + beginDate.ToString());

            var ondemandClientUpdate = new TradeSummaryOnDemandDelegate(OnDemandUpdateClients);

            _realTimeDataConsumer.GetTradeSummaryDataAsynch(clientName, symbol, interval, exchange, beginDate, true);
            _realTimeDataConsumer.RegisterOnDemandClient(ondemandClientUpdate);
        }

        public void WorkCompleteFreeResources(String clientName)
        {
            Library.WriteErrorLog(DateTime.Now + " :: " + clientName + " : Free Resources");

            _realTimeDataConsumer.WorkCompleteFreeResources(clientName);
        }

        private static ConcurrentDictionary<string, IBroadcastorCallBack> _clients = new ConcurrentDictionary<string, IBroadcastorCallBack>();

        private static object locker = new object();

        private bool startClock = false;

        public void UpdateClients(List<TradeSummary> tradeSummary)
        {
            lock (locker)
            {
                EventDataTypeCollection eventData = new EventDataTypeCollection();

                var inactiveClients = new List<string>();

                foreach (var client in _clients)
                {
                    try
                    {
                        eventData.EventMessage = tradeSummary;
                        client.Value.BroadcastToClient(eventData);
                    }
                    catch (InvalidOperationException ex)
                    {
                        Logger.log.Error("InternalDataFeedService -> UpdateClients() :: " + ex.ToString());

                        inactiveClients.Add(client.Key);
                    }
                    catch (CommunicationObjectAbortedException ex)
                    {
                        Logger.log.Error("InternalDataFeedService -> UpdateClients() :: " + ex.ToString());

                        inactiveClients.Add(client.Key);
                    }
                    catch (CommunicationObjectFaultedException ex)
                    {
                        Logger.log.Error("InternalDataFeedService -> UpdateClients() :: " + ex.ToString());

                        inactiveClients.Add(client.Key);
                    }
                    catch (CommunicationException ex)
                    {
                        Logger.log.Error("InternalDataFeedService -> UpdateClients() :: " + ex.ToString());

                        inactiveClients.Add(client.Key);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                        Logger.log.Error("Error whilst Updating Clients :: " + client + " : " + ex.ToString());

                        inactiveClients.Add(client.Key);
                    }
                }

                if (inactiveClients.Count > 0)
                {
                    IBroadcastorCallBack callback = null;

                    foreach (var client in inactiveClients)
                    {
                        if (_clients.TryRemove(client, out callback))
                        {
                            _realTimeDataConsumer.RemoveClients(client);
                            System.GC.Collect();

                            var msgStr = "Removal Succeeded :: " + client;
                            Console.WriteLine(msgStr);
                            Logger.log.Error(msgStr);
                        }
                        else
                        {
                            var msgStr = "Removal Failed :: " + client;
                            Console.WriteLine(msgStr);
                            Logger.log.Error(msgStr);
                        }
                    }
                }
            }
        }

        public void OnDemandUpdateClients(String clientName, List<TradeSummary> tradeSummary)
        {
            lock (locker)
            {
                EventDataTypeCollection eventData = new EventDataTypeCollection();

                var inactiveClients = new List<string>();

                foreach (var client in _clients)
                {
                    try
                    {
                        if (client.Key == clientName)
                        {
                            eventData.EventMessage = tradeSummary;
                            client.Value.BroadcastToClient(eventData);
                        }
                    }
                    catch (InvalidOperationException ex)
                    {
                        Logger.log.Error("InternalDataFeedService -> OnDemandUpdateClients() :: " + ex.ToString());

                        inactiveClients.Add(client.Key);
                    }
                    catch (CommunicationObjectAbortedException ex)
                    {
                        Logger.log.Error("InternalDataFeedService -> OnDemandUpdateClients() :: " + ex.ToString());

                        inactiveClients.Add(client.Key);
                    }
                    catch (CommunicationObjectFaultedException ex)
                    {
                        Logger.log.Error("InternalDataFeedService -> OnDemandUpdateClients() :: " + ex.ToString());

                        inactiveClients.Add(client.Key);
                    }
                    catch (CommunicationException ex)
                    {
                        Logger.log.Error("InternalDataFeedService -> OnDemandUpdateClients() :: " + ex.ToString());

                        inactiveClients.Add(client.Key);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                        Logger.log.Error("Error whilst Updating Selected Client :: " + client.Key + ":" + ex.ToString());

                        inactiveClients.Add(client.Key);
                    }
                }

                if (inactiveClients.Count > 0)
                {
                    IBroadcastorCallBack callback = null;

                    foreach (var client in inactiveClients)
                    {
                        if (_clients.TryRemove(client, out callback))
                        {
                            _realTimeDataConsumer.RemoveClients(client);
                            System.GC.Collect();

                            var msgStr = "Removal Succeeded :: " + client;
                            Console.WriteLine(msgStr);
                            Logger.log.Error(msgStr);
                        }
                        else
                        {
                            var msgStr = "Removal Failed :: " + client;
                            Console.WriteLine(msgStr);
                            Logger.log.Error(msgStr);
                        }
                    }
                }
            }
        }

        public void NotifyServer(EventDataTypeCollection eventData)
        {
            lock (locker)
            {
                var inactiveClients = new List<string>();
                foreach (var client in _clients)
                {
                    try
                    {
                        client.Value.BroadcastToClient(eventData);
                    }
                    catch (InvalidOperationException ex)
                    {
                        Logger.log.Error("InternalDataFeedService -> NotifyServer() :: " + ex.ToString());

                        throw;
                    }
                    catch (CommunicationObjectAbortedException ex)
                    {
                        Logger.log.Error("InternalDataFeedService -> NotifyServer() :: " + ex.ToString());

                        throw;
                    }
                    catch (CommunicationObjectFaultedException ex)
                    {
                        Logger.log.Error("InternalDataFeedService -> NotifyServer() :: " + ex.ToString());

                        throw;
                    }
                    catch (CommunicationException ex)
                    {
                        Logger.log.Error("InternalDataFeedService -> NotifyServer() :: " + ex.ToString());

                        throw;
                    }
                    catch (Exception ex)
                    {
                        Logger.log.Error("InternalDataFeedService -> NotifyServer() :: " + ex.ToString());

                        inactiveClients.Add(client.Key);
                    }
                }

                if (inactiveClients.Count > 0)
                {
                    IBroadcastorCallBack callback =
                        OperationContext.Current.GetCallbackChannel<IBroadcastorCallBack>();

                    foreach (var client in inactiveClients)
                    {
                        _clients.TryRemove(client, out callback);
                    }
                }
            }
        }

        public void RegisterClient(string clientName)
        {
            if (clientName != null && clientName != "")
            {
                try
                {
                    //Real time market updates by the minute
                    var updateHandlingTemp = new TradeSummaryDelegate(UpdateClients);
                    _realTimeDataConsumer.RegisterClient(updateHandlingTemp);


                    IBroadcastorCallBack callback =
                        OperationContext.Current.GetCallbackChannel<IBroadcastorCallBack>();


                    lock (locker)
                    {
                        //remove the old client
                        if (_clients.Keys.Contains(clientName))
                            _clients.TryRemove(clientName, out callback);
                        _clients.TryAdd(clientName, callback);

                    }
                }
                catch (InvalidOperationException ex)
                {
                    Logger.log.Error("InternalDataFeedService -> RegisterClient() :: " + ex.ToString());

                    throw;
                }
                catch (CommunicationObjectAbortedException ex)
                {
                    Logger.log.Error("InternalDataFeedService -> RegisterClient() :: " + ex.ToString());

                    throw;
                }
                catch (CommunicationObjectFaultedException ex)
                {
                    Logger.log.Error("InternalDataFeedService -> RegisterClient() :: " + ex.ToString());

                    throw;
                }
                catch (CommunicationException ex)
                {
                    Logger.log.Error("InternalDataFeedService -> RegisterClient() :: " + ex.ToString());

                    throw;
                }
                catch (Exception ex)
                {
                    Logger.log.Error("InternalDataFeedService -> RegisterClient() :: " + ex.ToString());

                    Console.WriteLine(ex.ToString());
                }
            }
        }




    }
}
