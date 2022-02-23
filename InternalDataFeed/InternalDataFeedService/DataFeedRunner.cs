using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ServiceModel;
using log4net;
using System.Net;


namespace InternalDataFeedService
{
    public delegate void FaultyCommmunication(); 

    public class DataFeedRunner
    {
        FaultyCommmunication _faultyCom = null;
        String _name;
        String _modeStr;



        public DataFeedRunner(FaultyCommmunication comDele, String name, String modeStr)
        {
            _faultyCom = comDele;
            _name = name;
            _modeStr = modeStr;
            //ServiceManager.DataFeedRestarted = false;
        }

        public void Run()
        {
            try
            {
                using (ServiceHost sv = new ServiceHost(typeof(InternalDataFeedService)))
                {
                    ServiceManager.DataFeedRestarted = false;

                    sv.Open();

                    foreach (var item in sv.Description.Endpoints)
                    {
                        Console.WriteLine(item.Address);
                    }

                    LogNotifyer.SendMessage("Internal DataFeed :: " + _name + " " + _modeStr + "...Started OK");

                    Console.WriteLine("");
                    Console.ReadLine();
                    Console.ReadLine();
                }
            }
            catch (AggregateException ex)
            {
                Logger.log.Error("DataFeedRunner -> Run() :: " + ex.ToString());

                _faultyCom.Invoke();
            }
            catch (InvalidOperationException ex)
            {
                Logger.log.Error("DataFeedRunner -> Run() :: " + ex.ToString());

                _faultyCom.Invoke();
            }
            catch (CommunicationObjectAbortedException ex)
            {
                Logger.log.Error("DataFeedRunner -> Run() :: " + ex.ToString());

                _faultyCom.Invoke();
            }
            catch (CommunicationObjectFaultedException ex)
            {
                Logger.log.Error("DataFeedRunner -> Run() :: " + ex.ToString());

                _faultyCom.Invoke();
            }
            catch (CommunicationException ex)
            {
                Logger.log.Error("DataFeedRunner -> Run() :: " + ex.ToString());

                _faultyCom.Invoke();
            }
            catch (Exception ex)
            {
                _faultyCom.Invoke();

                LogNotifyer.SendMessage("Internal DataFeed :: " + _name + " " + _modeStr + " :: ...FAILED!");

                Console.WriteLine(ex.Message);
                Logger.log.Error("Failed To Start Service :: " + ex.ToString());
            }
        }
    }
}
