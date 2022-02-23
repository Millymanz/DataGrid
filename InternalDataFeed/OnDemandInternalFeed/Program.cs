using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using log4net;
using System.Net;
using InternalDataFeedService;

namespace OnDemandInternalFeed
{
    class Program
    {
        static void Main(string[] args)
        {
            var modeStr = System.Configuration.ConfigurationManager.AppSettings["MODE"];
            String name = Dns.GetHostName().ToUpper();


            String title = "OnDemand Internal DataFeed Service :: " + modeStr + "- MODE";
            Console.Title = title + " :: " + DateTime.Now.ToString();

            Console.WriteLine(title);
            Console.WriteLine("");

            InternalDataFeedService.Logger.Setup();

            ServiceManager serviceMgr = new ServiceManager(name, modeStr);
            serviceMgr.Start();
        }
    }
}
