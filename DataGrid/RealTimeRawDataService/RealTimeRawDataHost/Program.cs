using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace RealTimeRawDataHost
{
    class Program
    {
        static void Main(string[] args)
        {
            Receiver.Logger.Setup("RealTimeRawDataHost.exe.config");

            var modeStr = System.Configuration.ConfigurationManager.AppSettings["MODE"];

            Console.Title = "RealTime Raw Data Service :: " + modeStr + "- MODE";

            ServiceHost sv = new ServiceHost(typeof(RealTimeRawDataService.RealTimeRawDataService));
            sv.Open();
            try
            {

                foreach (ServiceEndpoint se in sv.Description.Endpoints)
                    Console.WriteLine(se.Address.ToString());

                Console.WriteLine("");
                Console.WriteLine("RealTime Raw Data Service");
                Console.WriteLine("");
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR starting WCF service. Exception - " + e.Message);
            }


            Console.WriteLine("Press Enter To terminate.");
            Console.ReadLine();
            Console.ReadLine();

            sv.Close();
        }
    }
}
