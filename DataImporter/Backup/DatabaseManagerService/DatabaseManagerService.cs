using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

using System.Data;
using System.Data.SqlClient;
using System.Threading;
using FileService;

namespace DatabaseManagerService
{
    // NOTE: If you change the class name "Service1" here, you must also update the reference to "Service1" in App.config.
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Single, InstanceContextMode = InstanceContextMode.Single)]
    public class DatabaseManagerService : IDatabaseManagerService
    {
       
        public DatabaseManagerService()
        {

        }

        public bool ModeCheck(int mode)
        {
            MODE modeItem = (MODE)mode;

            if (DatabaseImporter.Mode == modeItem)
            {
                return true;
            }
            return false;
        }


        public bool UpdateANDStartProcessingCycle()
        {
            return FileManager.bUpdateDBServicesANDStartProcessingCycle; 
        }

        public int RetrieveFiles(String dateID, int mode)
        {
            MODE modeItem = (MODE)mode;

            if (FileService.DatabaseImporter.Mode == modeItem)
            {
                if (FileManager.bFilesInProcess == false)
                {
                    FileManager fileManager = new FileManager();

                    Thread thread = new Thread(new ParameterizedThreadStart(fileManager.ProcessFiles));

                    thread.Start(dateID);
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
            return -1;
        }

        public bool RetrieveRawDataFiles(String exchangeFilename, int mode)
        {
            //Temp
            DatabaseImporter.Mode = (MODE)mode;

            MODE modeItem = (MODE)mode;

            //if (FileService.DatabaseImporter.Mode == modeItem)
            {
                FileManager fileManager = new FileManager();

                Thread thread = new Thread(new ParameterizedThreadStart(fileManager.ProcessRawDataFiles));

                thread.Start(exchangeFilename);

                return true;
            }
            return false;
        }

        public void RunProcessingCycle()
        {
            FileManager.StartProcessingCycle(11000, "AMEX", "JobsManager_TEST");
        }
    }
}
