using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.ServiceModel;
using System.ServiceModel.Description;
using System.Configuration;

using DatabaseManagerService;

namespace DatabaseManager
{

    static public class RunApp
    {
        static public void Run(bool bLive)
        {
            //String modeType = System.Configuration.ConfigurationManager.AppSettings["MODE"];
            Console.Title = "DatabaseManager";

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

            if (modeType == "UI")
            {
                try
                {
                    //FileService.FileManager.StartProcessingCycle(11000);

                    Console.WriteLine("Select Runtime Mode - (1)TEST Or (2)LIVE-TEST");
                    String readAns = Console.ReadLine().ToLower();

                    //FileService.DatabaseImporter.Mode = (FileService.MODE)Convert.ToInt32(readAns);
                    var selection = (FileService.MODE)Convert.ToInt32(readAns);
                    FileService.DatabaseImporter.Mode = (FileService.MODE)selection;
                    if ((FileService.MODE)selection == FileService.MODE.Live) FileService.DatabaseImporter.Mode = FileService.MODE.Test;

                    ModeTitle();

                    Console.WriteLine("On Demand File Imports? Y Or N");
                    String onDemandResponse = Console.ReadLine().ToLower();

                    if (onDemandResponse == "y")
                    {
                        Console.WriteLine("File Import Type? (1)RAW Or (2)PROCESSED");
                        String response = Console.ReadLine().ToLower();

                        DatabaseManagerService.DatabaseManagerService dbImporter = new DatabaseManagerService.DatabaseManagerService();

                        if (Convert.ToInt32(response) == 1)
                        {
                            dbImporter.RetrieveRawDataFiles("", 1);
                        }
                        else
                        {
                            dbImporter.RetrieveFiles("", 1);
                        }
                    }

                   //DatabaseManagerService.DatabaseManagerService dbImporter = new DatabaseManagerService.DatabaseManagerService();
                   // //dbImporter.RetrieveRawDataFiles("\\\\S-index-one\\Users\\Public\\Downloads\\Output\\Test\\RawData\\NYSE\\NYSE_15122013_1920.csv", 1);
                   ////dbImporter.RetrieveRawDataFiles("", 1);
                   //dbImporter.RetrieveFiles("322014", (int)FileService.MODE.Test);
                    //dbImporter.RetrieveFiles("212014", 1);

                   //dbImporter.RunProcessingCycle();

                    StartService();
                }
                catch (Exception e)
                {
                    Library.WriteErrorLog(DateTime.Now + " :: " + e.ToString());

                    Console.WriteLine("ERROR starting WCF service. Exception - " + e.Message);
                    Console.WriteLine("Press Enter To terminate.");
                    Console.ReadLine();
                }
            }
            else if (modeType == "JUSTRUN")
            {
                var dbImporter = new DatabaseManagerService.DatabaseManagerService();

                dbImporter.RetrieveRawDataFiles("", 1);
                StartService();

                Console.ReadLine();
            }
            else
            {
                if (modeType == "LIVE")
                {
                    FileService.DatabaseImporter.Mode = (FileService.MODE)0;
                }
                else if (modeType == "TEST")
                {
                    FileService.DatabaseImporter.Mode = (FileService.MODE)1;
                }
                else if (modeType == "LIVE-TEST")
                {
                    FileService.DatabaseImporter.Mode = (FileService.MODE)2;
                }

                ModeTitle();
                //System.Threading.Thread.Sleep(20000);

                StartService();
                Console.ReadLine();
            }
        }

        static public void ModeTitle()
        {
            switch (FileService.DatabaseImporter.Mode)
            {
                case FileService.MODE.Test:
                    {
                        Console.Title += " :: TEST-MODE";
                    }
                    break;

                case FileService.MODE.Live:
                    {
                        Console.Title += " :: LIVE-MODE";
                    }
                    break;

                case FileService.MODE.LiveTest:
                    {
                        Console.Title += " :: LIVE-TEST-MODE";
                    }
                    break;
            }
        }

        static public void StartService()
        {
            FileService.Scheduling.StartScheduling();            
            
            ServiceHost sv = new ServiceHost(typeof(DatabaseManagerService.DatabaseManagerService));

            sv.Open();
            try
            {
                foreach (ServiceEndpoint se in sv.Description.Endpoints)
                    Console.WriteLine(se.Address.ToString());

                Console.WriteLine("");
                Console.WriteLine("DatabaseManagerService");
                Console.WriteLine("");

                //DatabaseManagerService.DatabaseManagerService dbm = new DatabaseManagerService.DatabaseManagerService();
                //dbm.RetrieveFiles("112014", 1);

                //DatabaseManagerService.DatabaseManagerService dbImporter = new DatabaseManagerService.DatabaseManagerService();
                //dbImporter.RetrieveFiles("122014", (int)FileService.MODE.Test);


            }
            catch (Exception e)
            {
                Library.WriteErrorLog(DateTime.Now + " :: " + e.ToString());

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
            try
            {
                DatabaseManager.RunApp.Run(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.WriteLine("Application Haulted!");
            }
            Console.ReadLine();

        }
    }

}
