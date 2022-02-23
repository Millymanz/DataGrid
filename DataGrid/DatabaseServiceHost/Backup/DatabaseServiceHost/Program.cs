using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.ServiceModel;
using System.ServiceModel.Description;
using DatabaseWcfService;


namespace DatabaseServiceHost
{

    static public class RunApp
    {
        static public void Run(bool bLive)
        {
            String modeType = bLive ? "LIVE" : "TEST";

            if (bLive)
            {
                modeType = "LIVE";
            }
            else
            {
                var modeStr = System.Configuration.ConfigurationManager.AppSettings["MODE"];
                modeType = modeStr;

                if (modeStr == "LIVE") modeType = "TEST";
            }


            //String modeType = System.Configuration.ConfigurationManager.AppSettings["MODE"];
            if (modeType == "UI")
            {
                try
                {
                    Console.Title = "DatabaseService";

                    Console.WriteLine("Select Runtime Mode - (1)TEST Or (2)LIVE-TEST");

                    String readAns = Console.ReadLine().ToLower();
                    //DatabaseWcfService.DatabaseWcfService.Mode = (MODE)Convert.ToInt32(readAns);

                    var selection = (MODE)Convert.ToInt32(readAns);
                    DatabaseWcfService.DatabaseWcfService.Mode = (MODE)selection;
                    if ((MODE)selection == MODE.Live) DatabaseWcfService.DatabaseWcfService.Mode = MODE.Test;


                    ModeTitle();

                    StartService();
                }
                catch (Exception e)
                {
                    Console.WriteLine("ERROR starting WCF service. Exception - " + e.Message);
                    Console.WriteLine("Press Enter To terminate.");
                    Console.ReadLine();
                }
            }
            else
            {
                if (modeType == "LIVE")
                {
                    DatabaseWcfService.DatabaseWcfService.Mode = (MODE)0;
                }
                else if (modeType == "TEST")
                {
                    DatabaseWcfService.DatabaseWcfService.Mode = (MODE)1;
                }
                else if (modeType == "LIVE-TEST")
                {
                    DatabaseWcfService.DatabaseWcfService.Mode = (MODE)2;
                }

                ModeTitle();

                StartService();

                Console.ReadLine();
            }
        }

         static public void ModeTitle()
        {
            switch (DatabaseWcfService.DatabaseWcfService.Mode)
            {
                case MODE.Test:
                    {
                        Console.Title += " :: TEST-MODE";
                    }
                    break;

                case MODE.Live:
                    {
                        Console.Title += " :: LIVE-MODE";
                    }
                    break;

                case MODE.LiveTest:
                    {
                        Console.Title += " :: LIVE-TEST-MODE";
                    }
                    break;
            }
        }

        static public void StartService()
        {
            ServiceHost sv = new ServiceHost(typeof(DatabaseWcfService.DatabaseWcfService));
            sv.Open();
            try
            {

                foreach (ServiceEndpoint se in sv.Description.Endpoints)
                    Console.WriteLine(se.Address.ToString());

                Console.WriteLine("");
                Console.WriteLine("DatabaseWcfService");
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

    class Program
    {
        static void Main(string[] args)
        {
            RunApp.Run(false);
        }
    }
       
}
