using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace RTChartPatternDataHost
{
    class Program
    {
        static void Main(string[] args)
        {
            ServiceHost sv = new ServiceHost(typeof(RTChartPatternDataService.RTChartPatternDataService));
            sv.Open();
            try
            {

                foreach (ServiceEndpoint se in sv.Description.Endpoints)
                    Console.WriteLine(se.Address.ToString());

                String title = "RTChartPatternData Service";
                Console.Title = title;

                Console.WriteLine("");
                Console.WriteLine(title);
                Console.WriteLine("");
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR starting WCF service. Exception - " + e.Message);
            }


            Console.WriteLine("Press Enter To terminate.");
            Console.ReadLine();

            sv.Close();

        }
    }
}
