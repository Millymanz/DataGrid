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
        public String DataType;
    }

    public class FileManager
    {
        static public Boolean bFilesInProcess = false;
        static public Boolean bUpdateDBServicesANDStartProcessingCycle = false;

        //private List<String> outputFileList = new List<String>();

        private List<FileNameExchange> outputFileList = new List<FileNameExchange>();


        private static Object thisLock = new Object();


        public void ProcessFiles(Object dateID)
        {
            lock (thisLock)
            {
                bFilesInProcess = true;

                String dateIDStr = dateID.ToString();

                String selectorStagingDir = "";
                String selectorOutput = "";
                String selector = "JobsManager";
                String selectorResults = "";



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


               // directoryOutput += dateID;

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

                            if (fileStrArray.Any())
                            {
                                //No need to file date id
                                // if (dateIDStr == fileStrArray[1])
                                //{
                                //    outputFileList.Add(rdr["FileName"].ToString());
                                //}

                                FileNameExchange filenameEx = new FileNameExchange();
                                filenameEx.FileName = rdr["FileName"].ToString();
                                filenameEx.Exchange = fileStrArray[1];

                                outputFileList.Add(filenameEx);

                            }
                        }
                    }
                }

                if (outputFileList.Count > 0)
                {
                    Console.WriteLine("Computed Data Importation Phase DateID :: " + dateIDStr);
                    LogHelper.Info("Computed Data Importation Phase DateID :: " + dateIDStr);

                    ImportPreparation(outputFileList, directoryOutput, stagingDir, false);


                    for (int i = 0; i < outputFileList.Count; i++)
                    {
                        String[] temp = outputFileList[i].FileName.Split('_');

                        String criteria = temp[0];
                        String selTemp = "";

                        if (temp[1] == "Forex")
                        {
                            selTemp = "FMDA_Forex_Result";
                        }
                        else
                        {
                            selTemp = "FMDA_Result";
                        }
                        selTemp += selectorResults;


                        DatabaseImporter databaseImporter = new DatabaseImporter();

                        if (Directory.Exists(stagingDir) == false)
                        {
                            Directory.CreateDirectory(stagingDir);
                        }
                        String fullpath = directoryOutput + "\\" + outputFileList[i].FileName + ".csv";
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
                                    case "RC":
                                        {
                                            databaseImporter.ImportHistoricalCorrelations(directoryOutput, outputFileList[i].FileName, ref rowsAffected, selTemp);
                                        }
                                        break;

                                }
                            }
                            catch (Exception ex)
                            {
                                errorMsg = ex.ToString();
                            }

                            Console.WriteLine("File Imported :: " + outputFileList[i].FileName);
                            LogHelper.Info("File Imported :: " + outputFileList[i].FileName);

                            ImportLogger(rowsAffected, count, errorMsg, outputFileList[i].FileName, importStartTime);
                        }
                    }
                    
                    List<String> list = outputFileList.Select(m => m.Exchange).ToList();

                    var fileManager = new DatabaseImporter();
                    fileManager.EmailNotificationCheck(list, false);

                }
                bFilesInProcess = false;
            }
        }

        private void EmailImportPreparation(Dictionary<String, KeyValuePair<int, long>> fileNamePairing, bool bRaw)
        {
            var message = new StringBuilder();
            string title = "";
            if (bRaw)
            {
                title = "STAGE (2a) Raw Trade Data Importation Preparation";
            }
            else
            {
                title = "STAGE (4a) Computed Data Importation Preparation";
            }

            message.AppendLine("Trade Data import preparation. File Listing Summary\n\n");

            message.AppendLine("Date :" + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
            message.AppendLine("\n\nData Manufacturing Process\n\n <br/><br/>");

            var style = "<style>table, td, th { border: 1px solid blue;} th {background-color: blue;color: white;}table {border-collapse: collapse;}table {width: 100%;}th {height: 50px;}td {padding: 25px;}</style>";

            message.AppendLine("<html><head>" + style + "</head><body><table style='width:100%; border: 1px solid black'>");
            message.AppendLine("<tr>");
            message.AppendLine("<th>Filename</th>");
            message.AppendLine("<th>CSVRecordCount</th>");
            message.AppendLine("<th>FileSize (Megabyte(s))</th>");
            message.AppendLine("</tr>");

            long totalFileSize = 0;

            foreach (var item in fileNamePairing)
            {
                var splitArray = item.Key.Split('\\');
                var filename = splitArray.LastOrDefault();

                message.AppendLine("<tr>");
                message.AppendLine("<td>" + filename + "</td>");
                message.AppendLine("<td>" + item.Value.Key + "</td>");

                long megaBytes = item.Value.Value / 1048576;
                totalFileSize += megaBytes;

                message.AppendLine("<td>" + megaBytes + "</td>");

                message.AppendLine("</tr>");
            }
            //222MB takes about 40mins to import
            //60min instead

            Double time = (totalFileSize * 40) / 222;

            message.AppendLine("</table><br/><br/><br/>");

            message.AppendLine("Estimated Imporatation Time :: " + (time / 60) + "hours <br/>");
            message.AppendLine("Estimated Imporatation Time :: " + time + "mins <br/>");
            message.AppendLine("Estimated Imporatation Time :: " + (time * 60) + "seconds <br/>");

            message.AppendLine("<br/><br/></body></html>");


            var email = new EmailManager();

            email.Send(null, title, message.ToString(), true);
        }

        private void ImportPreparation(List<FileNameExchange> rawFileList, String directoryOutput, String stagingDir, bool bRaw)
        {
            var timeframeList = System.Configuration.ConfigurationManager.AppSettings["MachineSpecificTimeFrameList"].Split(',').ToList();
            
            bool minuteOnly = false;
            bool tenMin15minOnly = false;

            if (timeframeList.Any(d => d == "1min" || d == "5min") && timeframeList.Count == 4)
            {
                minuteOnly = true;
            }

            if (timeframeList.Any(d => d == "10min" || d == "15min") && timeframeList.Count == 4)
            {
                tenMin15minOnly = true;
            }

            LogHelper.Info("Import Preparation : Get Files : " + directoryOutput);
            Library.WriteErrorLog(DateTime.Now + " :: " + "Import Preparation : Get Files : " + directoryOutput);

            Dictionary<String, KeyValuePair<int, long>> fileNamePairing = new Dictionary<String, KeyValuePair<int, long>>();

            DirectoryInfo dirInfo = new DirectoryInfo(directoryOutput);

            var fileListings = dirInfo.GetFiles("*.csv");

            var selectFileListings = fileListings.Where(x => rawFileList.Select(p => p.FileName).Contains(Path.GetFileNameWithoutExtension(x.FullName)));

            var fileInfoListing = new List<FileInfo>();
            if (selectFileListings.Any())
            {
                foreach (var item in selectFileListings)
                {
                    var tempArray = item.FullName.Split('_');

                    if (tempArray.LastOrDefault() == "1min5min" &&  minuteOnly || 
                        tempArray.LastOrDefault() == "10min15min" && tenMin15minOnly)
                    {
                        fileInfoListing.Add(item);
                    }
                }
            }

            if (fileInfoListing.Any()) selectFileListings = fileInfoListing;

            foreach (var item in selectFileListings)
            {
                if (File.Exists(item.FullName))
                {
                    int count = 0;
                    using (StreamReader r = new StreamReader(item.FullName))
                    {
                        string line;
                        while ((line = r.ReadLine()) != null) { count++; }
                    }

                    FileInfo fileInfo = new FileInfo(item.FullName);

                    fileNamePairing.Add(item.FullName, new KeyValuePair<int, long>(count, fileInfo.Length));
                }
            }

            LogHelper.Info("File Prep Stats Completed");

            EmailImportPreparation(fileNamePairing, bRaw);
        }

        public void ProcessRawDataFiles(Object fileNameObj)
        {
            lock (thisLock)
            {
                var bFirstTime = true;
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
                List<FileNameExchange> tempFileList = new List<FileNameExchange>();

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

                        filenameEx.DataType = rdr["DataType"].ToString();

                        tempFileList.Add(filenameEx);
                    }


                    //Order dependant
                    var raw = tempFileList.Where(m => m.DataType == "Raw");
                    if (raw.Count() > 0) rawFileList.AddRange(raw);

                    var eventDesc = tempFileList.Where(m => m.DataType == "EventDesc");
                    if (eventDesc.Count() > 0) rawFileList.AddRange(eventDesc);

                    var keyPers = tempFileList.Where(m => m.DataType == "KeyPers");
                    if (keyPers.Count() > 0) rawFileList.AddRange(keyPers);

                    var eventData = tempFileList.Where(m => m.DataType == "EventData");
                    if (eventData.Count() > 0) rawFileList.AddRange(eventData);

                }

                try
                {

                    if (rawFileList.Count > 0)
                    {
                        Console.WriteLine("Number Of Potential File Imports : " + rawFileList.Count);
                        Library.WriteErrorLog(DateTime.Now + " :: " + "Number Of Potential File Imports : " + rawFileList.Count);

                        ImportPreparation(rawFileList, directoryOutput, stagingDir, true);

                        var timeframeList = System.Configuration.ConfigurationManager.AppSettings["MachineSpecificTimeFrameList"].Split(',').ToList();
                        bool minuteOnly = false;
                        bool tenMin15minOnly = false;
                        if (timeframeList.Any(d => d == "1min" || d == "5min") && timeframeList.Count == 4)
                        {
                            minuteOnly = true;
                        }
                        if (timeframeList.Any(d => d == "10min" || d == "15min") && timeframeList.Count == 4)
                        {
                            tenMin15minOnly = true;
                        }
                        var fileInfoListing = new List<FileNameExchange>();
                        if (rawFileList.Any())
                        {
                            foreach (var item in rawFileList)
                            {
                                var tempArray = item.FileName.Split('_');

                                if (tempArray.LastOrDefault() == "1min5min" && minuteOnly ||
                                    tempArray.LastOrDefault() == "10min15min" && tenMin15minOnly)
                                {
                                    fileInfoListing.Add(item);
                                }
                            }
                        }
                        if (fileInfoListing.Any()) rawFileList = fileInfoListing;


                        DateTime importStartTime = new DateTime();
                        importStartTime = DateTime.Now;

                        for (int i = 0; i < rawFileList.Count; i++) // this to use the returned dictionary from ImportPreparation 
                        {
                            System.GC.Collect();

                            String[] temp = rawFileList[i].FileName.Split('_');

                            String criteria = temp[0];

                            if (Directory.Exists(stagingDir) == false)
                            {
                                Directory.CreateDirectory(stagingDir);
                            }
                            var array = temp;

                            String fullpath = directoryOutput + "\\" + array.FirstOrDefault() + "\\" + rawFileList[i].FileName + ".csv";
                            String dst = stagingDir + "\\Temp.csv";

                            if (File.Exists(fullpath))
                            {
                                FileInfo fileInfo = new FileInfo(fullpath);
                                fileInfo.CopyTo(dst, true);

                                int rowsAffected = 0;

                                String errorMsg = "";
                                Console.WriteLine("");
                                Console.WriteLine("Counting rows for filename :: " + rawFileList[i].FileName);

                                int count = 0;
                                using (StreamReader r = new StreamReader(fullpath))
                                {
                                    string line;
                                    while ((line = r.ReadLine()) != null)
                                    {
                                        count++;
                                    }
                                }


                                if (count > 0)
                                {
                                    Console.WriteLine("Total Rows :: " + count);

                                    try
                                    {
                                        String tempSelector = selector;
                                        bool bforex = false;

                                        if (rawFileList[i].Exchange.ToLower() == "forex") { tempSelector += "_Forex"; bforex = true; }
                                        if (rawFileList[i].Exchange.ToLower() == "fundamentaldata") tempSelector += "_FundamentalData";
                                        if (rawFileList[i].Exchange.ToLower() == "nymex") tempSelector += "_" + rawFileList[i].Exchange;


                                        if (bFirstTime && bforex)
                                        {
                                            bFirstTime = false;
                                            ClearDataFillerLog(tempSelector);
                                        }

                                        int iter = i + 1;
                                        Console.WriteLine(iter + " Of " + rawFileList.Count);

                                        Console.WriteLine("Importing :: " + rawFileList[i].FileName + " Timestamp :: " + DateTime.Now.ToString());

                                        databaseImporter.ImportRawData(dst, criteria, ref rowsAffected, tempSelector, rawFileList[i].DataType);

                                        int left = rawFileList.Count - iter;
                                        Console.WriteLine(left + " Files Left To Import...");
                                    }
                                    catch (Exception ex)
                                    {
                                        errorMsg = ex.ToString();
                                    }
                                    ImportLogger(rowsAffected, count, errorMsg, rawFileList[i].FileName, importStartTime);
                                }
                            }
                        }

                        if (rawFileList.Any())
                        {
                            var exchangeList = rawFileList.Select(m => m.Exchange);

                            int port = 11002;

                            if (DatabaseImporter.Mode == MODE.Test) { port = 11000; }

                            StartProcessingCycle(port, exchangeList.ToList(), selectorJBM);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    Library.WriteErrorLog(DateTime.Now + " :: " + ex.ToString());
                }
            }
            Console.WriteLine("Import Phase Completed!");
            Library.WriteErrorLog(DateTime.Now + " :: " + "Import Phase Completed!");
        }

        private void ClearDataFillerLog(string selector)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings[selector].ConnectionString))
                {
                    conn.Open();
                    SqlCommand sqlCommand = new SqlCommand("proc_RecreateDataFiller", conn);
                    sqlCommand.CommandType = CommandType.StoredProcedure;

                    sqlCommand.CommandTimeout = 0;
                    sqlCommand.ExecuteNonQuery();

                }
            }
            catch (Exception ex)
            {
                Library.WriteErrorLog(DateTime.Now + " :: " + ex.ToString());
                Console.WriteLine(ex.ToString());
            }
        }




        //public void ProcessRawDataFiles(Object fileNameObj)
        //{
        //    lock (thisLock)
        //    {
        //        String filename = fileNameObj.ToString();

        //        //List<String> rawFileList = new List<String>();
        //        List<FileNameExchange> rawFileList = new List<FileNameExchange>();

        //        DatabaseImporter databaseImporter = new DatabaseImporter();

        //        String selector = "TRADES";
        //        String selectorJBM = "JobsManager";

        //        String selectorStagingDir = "";
        //        String selectorOutput = "";

        //        switch (DatabaseImporter.Mode)
        //        {
        //            case MODE.Test:
        //                {
        //                    selector += "_TEST";
        //                    selectorJBM += "_TEST";
        //                    selectorOutput += "OUTPUTPATH_TEST_RAWDATA";
        //                    selectorStagingDir += "OUTPUTPATH_TEST_RAWDATA_STAGING";
        //                }
        //                break;

        //            case MODE.Live:
        //                {
        //                    selector += "_LIVE";
        //                    selectorJBM += "_LIVE";
        //                    selectorOutput += "OUTPUTPATH_LIVE_RAWDATA";
        //                    selectorStagingDir += "OUTPUTPATH_LIVE_RAWDATA_STAGING";
        //                }
        //                break;

        //            case MODE.LiveTest:
        //                {
        //                    selector += "_LIVE-TEST";
        //                    selectorJBM += "_LIVE-TEST";
        //                    selectorOutput += "OUTPUTPATH_LIVE-TEST_RAWDATA";
        //                    selectorStagingDir += "OUTPUTPATH_LIVE-TEST_RAWDATA_STAGING";
        //                }
        //                break;
        //        }

        //        String directoryOutput = System.Configuration.ConfigurationManager.AppSettings[selectorOutput];
        //        String stagingDir = System.Configuration.ConfigurationManager.AppSettings[selectorStagingDir];


        //        //directoryOutput += dateID;

        //        using (SqlConnection con = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings[selectorJBM].ToString()))
        //        {
        //            //String commandTxt = "SELECT dbo.[FileImports].FileName FROM dbo.[FileImports] WHERE dbo.[FileImports].ImportCompleted ='False' AND dbo.[FileImports].DataType ='Raw'";

        //            SqlCommand sqlCommand = new SqlCommand("proc_GetImportReadyFiles", con);
        //            sqlCommand.CommandTimeout = 0;
        //            sqlCommand.CommandType = CommandType.StoredProcedure;

        //            con.Open();

        //            SqlDataReader rdr = sqlCommand.ExecuteReader();
        //            while (rdr.Read())
        //            {
        //                String[] fileStrArray = rdr["FileName"].ToString().Split('_');

        //                FileNameExchange filenameEx = new FileNameExchange();
        //                filenameEx.FileName = rdr["FileName"].ToString();
        //                filenameEx.Exchange = fileStrArray.FirstOrDefault();

        //                //rawFileList.Add(rdr["FileName"].ToString());
        //                rawFileList.Add(filenameEx);
        //            }
        //        }

        //        if (rawFileList.Count > 0)
        //        {
        //            //Console.WriteLine("Computed Data Importation Phase DateID :: " + dateIDStr);
        //            DateTime importStartTime = new DateTime();
        //            importStartTime = DateTime.Now;

        //            for (int i = 0; i < rawFileList.Count; i++)
        //            {
        //                System.GC.Collect();

        //                String[] temp = rawFileList[i].FileName.Split('_');

        //                String criteria = temp[0];

        //                if (Directory.Exists(stagingDir) == false)
        //                {
        //                    Directory.CreateDirectory(stagingDir);
        //                }
        //                var array = temp;

        //                //var array = rawFileList[i].Split('_');

        //                String fullpath = directoryOutput + "\\" + array.FirstOrDefault() + "\\" + rawFileList[i].FileName + ".csv";
        //                String dst = stagingDir + "\\Temp.csv";

        //                if (File.Exists(fullpath))
        //                {

        //                    FileInfo fileInfo = new FileInfo(fullpath);
        //                    fileInfo.CopyTo(dst, true);


        //                    int rowsAffected = 0;
        //                    int rowsCommitted = 0;
        //                    String errorMsg = "";

        //                    Console.WriteLine("Counting rows for filename :: " + rawFileList[i].FileName);


        //                    int count = 0;
        //                    using (StreamReader r = new StreamReader(fullpath))
        //                    {
        //                        string line;
        //                        while ((line = r.ReadLine()) != null)
        //                        {
        //                            count++;
        //                        }
        //                    }

        //                    Console.WriteLine("Total Rows :: " + count);

        //                    try
        //                    {
        //                        String tempSelector = selector;
        //                        if (rawFileList[i].Exchange == "Forex" || rawFileList[i].Exchange == "FOREX") tempSelector += "_Forex";

        //                        Console.WriteLine("Importing :: " + rawFileList[i].FileName + "  Timestamp :: " + DateTime.Now.ToString());

        //                        databaseImporter.ImportRawData(filename, criteria, ref rowsAffected, tempSelector);
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        errorMsg = ex.ToString();
        //                    }

        //                    ImportLogger(rowsAffected, count, errorMsg, rawFileList[i].FileName, importStartTime);

        //                    ///Set bUpdateDBServicesANDStartProcessingCycle flag
        //                    ///Hold
        //                    ///

        //                    //if (DatabaseImporter.Mode == MODE.Test)
        //                    //{
        //                    //    //Incase running on same machine
        //                    //    StartProcessingCycle(11000, rawFileList[i].Exchange, selectorJBM);
        //                    //   // StartProcessingCycle(11022, rawFileList[i].Exchange, selectorJBM);
        //                    //}
        //                    //else
        //                    //{
        //                    //    //Live will always been on different machines
        //                    //    StartProcessingCycle(11000, rawFileList[i].Exchange, selectorJBM);
        //                    //}

        //                }
        //            }

        //            if (rawFileList.Any())
        //            {
        //                var exchangeList = rawFileList.Select(m => m.Exchange);

        //                int port = 11002;
                        
        //                if (DatabaseImporter.Mode == MODE.Test) { port = 11000; }

        //                StartProcessingCycle(port, exchangeList.ToList(), selectorJBM);
        //            }
        //        }
        //    }
        //}

        private void ImportLogger(int rowsAffected, int count, String errorMsg, String fileNameNoExt, DateTime importStartTime)
        {
            if (count == 0) errorMsg = "Empty file discarded";

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

            //if (String.IsNullOrEmpty(errorMsg) == false)
            //{
            //    percentageSuccess = 0.0;
            //    percentageFailure = 100.0;
            //}

            

            double successThreshold = Convert.ToDouble(System.Configuration.ConfigurationManager.AppSettings["IMPORT_SUCCESS_THRESHOLD"]);

            //Import Log
            using (SqlConnection con = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings[selector].ToString()))
            {
                //String proc = bComputed ? : "proc_ImportLogging";

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

        static public void StartProcessingCycle(int GroupPort, List<String> Exchange, String selector)
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

        static public void StartProcessingCycleNOW(int GroupPort, List<String> Exchange, String selector)
        {
            UdpClient udp = new UdpClient();

            IPEndPoint groupEP = new IPEndPoint(IPAddress.Broadcast, GroupPort);
            String dateID = String.Format("{0}{1}{2}", DateTime.Now.Day, DateTime.Now.Month, DateTime.Now.Year);
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

