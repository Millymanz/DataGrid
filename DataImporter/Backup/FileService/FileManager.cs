using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;
using System.Data.SqlClient;
using System.Net.Sockets;
using System.Net;

namespace FileService
{
    public struct FileNameExchange
    {
        public String FileName;
        public String Exchange;
    }

    public class FileManager
    {
        static public Boolean bFilesInProcess = false;
        static public Boolean bUpdateDBServicesANDStartProcessingCycle = false;

        private List<String> outputFileList = new List<String>();

        public void ProcessFiles(Object dateID)
        {
            bFilesInProcess = true;

            String dateIDStr = dateID.ToString();

            String selectorStagingDir = "";
            String selectorOutput = "";
            String selector = "JobsManager";
            String selectorResults = "FMDA_Result";



            switch (DatabaseImporter.Mode)
            {
                case MODE.Test:
                    {
                        selector += "_TEST";
                        selectorResults += "_TEST";
                        selectorOutput += "OUTPUTPATH_TEST";
                        selectorStagingDir += "STAGING_DIR_TEST";
                    }
                    break;

                case MODE.Live:
                    {
                        selector += "_LIVE";
                        selectorResults += "_LIVE";
                        selectorOutput += "OUTPUTPATH_LIVE";
                        selectorStagingDir += "STAGING_DIR_LIVE";
                    }
                    break;

                case MODE.LiveTest:
                    {
                        selector += "_LIVE-TEST";
                        selectorResults += "_LIVE-TEST";
                        selectorOutput += "OUTPUTPATH_LIVE-TEST";
                        selectorStagingDir += "STAGING_DIR_LIVETEST";
                    }
                    break;
            }


            String directoryOutput = System.Configuration.ConfigurationManager.AppSettings[selectorOutput];
            String stagingDir = System.Configuration.ConfigurationManager.AppSettings[selectorStagingDir];


            directoryOutput += dateID;

            using (SqlConnection con = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings[selector].ToString()))
            {
                String commandTxt = "SELECT dbo.[FileImports].FileName FROM dbo.[FileImports] WHERE dbo.[FileImports].ImportCompleted ='False' AND dbo.[FileImports].DataType ='Computed'";
                SqlCommand sqlCommand = new SqlCommand(commandTxt, con);
                sqlCommand.CommandTimeout = 0;

                con.Open();

                SqlDataReader rdr = sqlCommand.ExecuteReader();
                while (rdr.Read())
                {
                    if (String.IsNullOrEmpty(rdr["FileName"].ToString()) == false)
                    {
                        String[] fileStrArray = rdr["FileName"].ToString().Split('_'); ;

                        if (fileStrArray.Count() > 1)
                        {
                            //No need to file date id
                           // if (dateIDStr == fileStrArray[1])
                            {
                                outputFileList.Add(rdr["FileName"].ToString());
                            }
                        }
                    }
                }
            }

            if (outputFileList.Count > 0)
            {
                Console.WriteLine("Computed Data Importation Phase DateID :: " + dateIDStr);

                for (int i = 0; i < outputFileList.Count; i++)
                {
                    String[] temp = outputFileList[i].Split('_');

                    String criteria = temp[0];

                    DatabaseImporter databaseImporter = new DatabaseImporter();

                    if (Directory.Exists(stagingDir) == false)
                    {
                        Directory.CreateDirectory(stagingDir);
                    }
                    String fullpath = directoryOutput + "\\" + outputFileList[i] + ".csv";
                    String dst = stagingDir + "\\Temp.csv";

                    if (File.Exists(fullpath))
                    {

                        FileInfo fileInfo = new FileInfo(fullpath);
                        fileInfo.CopyTo(dst, true);

                        int count = 0;
                        using (StreamReader r = new StreamReader(fullpath))
                        {
                            string line;
                            while ((line = r.ReadLine()) != null)
                            {
                                count++;
                            }
                        }

                        int rowsAffected = 0;
                        String errorMsg = "";

                        DateTime importStartTime = new DateTime();
                        importStartTime = DateTime.Now;
                        try
                        {
                            switch (criteria)
                            {
                                case "HC":
                                    {
                                        databaseImporter.ImportHistoricalCorrelations(directoryOutput, outputFileList[i], ref rowsAffected, selectorResults);
                                    }
                                    break;

                                case "RC":
                                    {

                                    }
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            errorMsg = ex.ToString();
                        }

                        Console.WriteLine("File Imported :: " + outputFileList[i]);

                        ImportLogger(rowsAffected, count, errorMsg, outputFileList[i], importStartTime);
                    }
                }
            }
            bFilesInProcess = false;
        }

        public void ProcessRawDataFiles(Object fileNameObj)
        {
            String filename = fileNameObj.ToString();

            //List<String> rawFileList = new List<String>();
            List<FileNameExchange> rawFileList = new List<FileNameExchange>();
 
            DatabaseImporter databaseImporter = new DatabaseImporter();

            String selector = "TRADES";
            String selectorJBM = "JobsManager";

            String selectorStagingDir = "";
            String selectorOutput = "";

            switch (DatabaseImporter.Mode)
            {
                case MODE.Test:
                    {
                        selector += "_TEST";
                        selectorJBM += "_TEST";
                        selectorOutput += "OUTPUTPATH_TEST_RAWDATA";
                        selectorStagingDir += "OUTPUTPATH_TEST_RAWDATA_STAGING";
                    }
                    break;

                case MODE.Live:
                    {
                        selector += "_LIVE";
                        selectorJBM += "_LIVE";
                        selectorOutput += "OUTPUTPATH_LIVE_RAWDATA";
                        selectorStagingDir += "OUTPUTPATH_LIVE_RAWDATA_STAGING";
                    }
                    break;

                case MODE.LiveTest:
                    {
                        selector += "_LIVE-TEST";
                        selectorJBM += "_LIVE-TEST";
                        selectorOutput += "OUTPUTPATH_LIVE-TEST_RAWDATA";
                        selectorStagingDir += "OUTPUTPATH_LIVE-TEST_RAWDATA_STAGING";
                    }
                    break;
            }

            String directoryOutput = System.Configuration.ConfigurationManager.AppSettings[selectorOutput];
            String stagingDir = System.Configuration.ConfigurationManager.AppSettings[selectorStagingDir];


            //directoryOutput += dateID;

            using (SqlConnection con = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings[selectorJBM].ToString()))
            {
                //String commandTxt = "SELECT dbo.[FileImports].FileName FROM dbo.[FileImports] WHERE dbo.[FileImports].ImportCompleted ='False' AND dbo.[FileImports].DataType ='Raw'";

                SqlCommand sqlCommand = new SqlCommand("proc_GetImportReadyFiles", con);
                sqlCommand.CommandTimeout = 0;
                sqlCommand.CommandType = CommandType.StoredProcedure;

                con.Open();

                SqlDataReader rdr = sqlCommand.ExecuteReader();
                while (rdr.Read())
                {
                    String[] fileStrArray = rdr["FileName"].ToString().Split('_');
                    
                    FileNameExchange filenameEx = new FileNameExchange();
                    filenameEx.FileName = rdr["FileName"].ToString();
                    filenameEx.Exchange = fileStrArray.FirstOrDefault();

                    //rawFileList.Add(rdr["FileName"].ToString());
                    rawFileList.Add(filenameEx);
                }
            }

            if (rawFileList.Count > 0)
            {
                //Console.WriteLine("Computed Data Importation Phase DateID :: " + dateIDStr);
                DateTime importStartTime = new DateTime();
                importStartTime = DateTime.Now;

                for (int i = 0; i < rawFileList.Count; i++)
                {
                    System.GC.Collect();

                    String[] temp = rawFileList[i].FileName.Split('_');

                    String criteria = temp[0];

                    if (Directory.Exists(stagingDir) == false)
                    {
                        Directory.CreateDirectory(stagingDir);
                    }
                    var array = temp;

                    //var array = rawFileList[i].Split('_');

                    String fullpath = directoryOutput + "\\" + array.FirstOrDefault() + "\\" + rawFileList[i].FileName + ".csv";
                    String dst = stagingDir + "\\Temp.csv";

                    if (File.Exists(fullpath))
                    {

                        FileInfo fileInfo = new FileInfo(fullpath);
                        fileInfo.CopyTo(dst, true);


                        int rowsAffected = 0;
                        int rowsCommitted = 0;
                        String errorMsg = "";

                        int count = 0;
                        using (StreamReader r = new StreamReader(fullpath))
                        {
                            string line;
                            while ((line = r.ReadLine()) != null)
                            {
                                count++;
                            }
                        }

                        try
                        {
                            String tempSelector = selector;
                            if (rawFileList[i].Exchange == "Forex") tempSelector += "_Forex";

                            databaseImporter.ImportRawData(filename, criteria, ref rowsAffected, tempSelector);
                        }
                        catch (Exception ex)
                        {
                            errorMsg = ex.ToString();
                        }

                        ImportLogger(rowsAffected, count, errorMsg, rawFileList[i].FileName, importStartTime);
                        
                        //Set bUpdateDBServicesANDStartProcessingCycle flag
                        //Hold
                        //if (DatabaseImporter.Mode == MODE.Test)
                        //{
                        //    //Incase running on same machine
                        //    StartProcessingCycle(11000, rawFileList[i].Exchange, selector);
                        //    StartProcessingCycle(11022, rawFileList[i].Exchange, selector);
                        //}
                        //else
                        //{
                        //    //Live will always been on different machines
                        //    StartProcessingCycle(11000, rawFileList[i].Exchange, selector);
                        //}

                    }
                }
            }
        }

        private void ImportLogger(int rowsAffected, int count, String errorMsg, String fileNameNoExt, DateTime importStartTime)
        {
            String selector = "JobsManager";
            switch (DatabaseImporter.Mode)
            {
                case MODE.Test:
                    {
                        selector += "_TEST";
                    }
                    break;

                case MODE.Live:
                    {
                        selector += "_LIVE";
                    }
                    break;

                case MODE.LiveTest:
                    {
                        selector += "_LIVE-TEST";
                    }
                    break;
            }


            DateTime importEndTime = new DateTime();
            importEndTime = DateTime.Now;

            double percentageSuccess = (Convert.ToDouble(rowsAffected) / Convert.ToDouble(count)) * 100.0;

            if (double.IsNaN(percentageSuccess))
            {
                percentageSuccess = 0.0;
            }
            
            double percentageFailure = 100 - percentageSuccess;

            

            double successThreshold = Convert.ToDouble(System.Configuration.ConfigurationManager.AppSettings["IMPORT_SUCCESS_THRESHOLD"]);

            //Import Log
            using (SqlConnection con = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings[selector].ToString()))
            {
                SqlCommand sqlCommand = new SqlCommand("proc_ImportLogging", con);
                con.Open();

                sqlCommand.CommandType = CommandType.StoredProcedure;
                sqlCommand.Parameters.AddWithValue("@FileName", fileNameNoExt);
                sqlCommand.Parameters.AddWithValue("@ErrorMsg", errorMsg);
                sqlCommand.Parameters.AddWithValue("@CSVRecordCount", count);

                sqlCommand.Parameters.AddWithValue("@ImportedRecordCount", rowsAffected);
                sqlCommand.Parameters.AddWithValue("@PercentageSuccess", percentageSuccess);
                sqlCommand.Parameters.AddWithValue("@PercentageFailure", percentageFailure);
                sqlCommand.Parameters.AddWithValue("@ImportStartTime", importStartTime);
                sqlCommand.Parameters.AddWithValue("@ImportEndTime", importEndTime);
                sqlCommand.Parameters.AddWithValue("@SuccessThreshold", successThreshold);

                int affectedRows = sqlCommand.ExecuteNonQuery();
            }
        }

        static public void StartProcessingCycle(int GroupPort, String Exchange, String selector)
        {
            DatabaseImporter databaseImporter = new DatabaseImporter();
            databaseImporter.NotifyJM(selector, Exchange);



            UdpClient udp = new UdpClient();

            IPEndPoint groupEP = new IPEndPoint(IPAddress.Broadcast, GroupPort);
            String dateID = String.Format("{0}{1}{2}", DateTime.Now.Day,  DateTime.Now.Month,  DateTime.Now.Year);
            string str4 = "bUpdateDBServicesANDStartProcessingCycle_" + dateID;

            byte[] sendBytes4 = Encoding.ASCII.GetBytes(str4);
            udp.Send(sendBytes4, sendBytes4.Length, groupEP);
        }
    }
}

        //static public void StartProcessingCycle()
        //{
        //    UdpClient udp = new UdpClient();

        //    int GroupPort = 11000;

        //    IPHostEntry host;
        //    string localIP = "?";
        //    //host = Dns.GetHostEntry(Dns.GetHostName());

        //    // host = Dns.GetHostByName("index-one");

        //    //foreach (IPAddress ip in host.AddressList)
        //    //{
        //    //    if (ip.AddressFamily.ToString() == "InterNetwork")
        //    //    {
        //    //        localIP = ip.ToString();
        //    //    }
        //    //}

        //    // IPEndPoint groupEP = new IPEndPoint(IPAddress.Parse(localIP), GroupPort);
        //    IPEndPoint groupEP = new IPEndPoint(IPAddress.Broadcast, GroupPort);
        //    string str4 = "Is anyone out there? broadcast OUT";

        //    byte[] sendBytes4 = Encoding.ASCII.GetBytes(str4);
        //    udp.Send(sendBytes4, sendBytes4.Length, groupEP);
        //}  

