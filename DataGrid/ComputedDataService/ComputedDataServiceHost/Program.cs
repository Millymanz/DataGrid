using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Net;

namespace ComputedDataServiceHost
{
    class Program
    {
        static void Main(string[] args)
        {
            var modeStr = System.Configuration.ConfigurationManager.AppSettings["MODE"];
            String name = Dns.GetHostName().ToUpper();



            ServiceHost sv = new ServiceHost(typeof(ComputedDataService.ComputedDataService));
            sv.Open();
            try
            {

                foreach (ServiceEndpoint se in sv.Description.Endpoints)
                    Console.WriteLine(se.Address.ToString());

                LogNotifyer.SendMessage("Computed Data Service :: " + name + " " + modeStr + "...Started OK");


                Console.WriteLine("");
                Console.WriteLine("Computed Data Service");
                Console.WriteLine("");
            }
            catch (Exception e)
            {
                LogNotifyer.SendMessage("Computed Data Service :: " + name + " " + modeStr + " :: ...FAILED!");

                Console.WriteLine("ERROR starting WCF service. Exception - " + e.Message);
            }


            Console.WriteLine("Press Enter To terminate.");
            Console.ReadLine();

            sv.Close();
        }
    }
}
