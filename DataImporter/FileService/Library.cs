using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Globalization;
using System.Net;
using System.Threading;

namespace FileService
{
    public static class Library
    {
        public static void WriteErrorLog(Exception ex)
        {
            var threadStart = new ParameterizedThreadStart(WriteError);
            Thread initialseDataTablesThread = new Thread(threadStart);
            initialseDataTablesThread.Start(ex);
        }

        private static void WriteError(object current)
        {
            var ex = current as Exception;
            if (ex != null)
            {
                var InvC = new CultureInfo("en-GB");

                StreamWriter sw = null;
                try
                {
                    string logFolder = AppDomain.CurrentDomain.BaseDirectory + "\\" + "LogFolder";
                    string dateTimeStamp = DateTime.Now.ToString("yyyy-MM-dd");

                    if (Directory.Exists(logFolder) == false)
                    {
                        Directory.CreateDirectory(logFolder);
                    }

                    sw = new StreamWriter(logFolder + "\\" + dateTimeStamp + "_LogFile.txt", true);
                    sw.WriteLine(DateTime.Now.ToString() + ": " + ex.Source.ToString().Trim() + "; " + ex.Message.ToString().Trim());
                    sw.Flush();
                    sw.Close();
                }
                catch
                {
                }
            }
        }

        private static void WriteErrorLogStr(object current)
        {
            var Message = current as string;
            if (Message != null)
            {
                var InvC = new CultureInfo("en-GB");

                StreamWriter sw = null;
                try
                {
                    string logFolder = AppDomain.CurrentDomain.BaseDirectory + "\\" + "LogFolder";
                    string dateTimeStamp = DateTime.Now.ToString("yyyy-MM-dd");

                    if (Directory.Exists(logFolder) == false)
                    {
                        Directory.CreateDirectory(logFolder);
                    }

                    sw = new StreamWriter(logFolder + "\\" + dateTimeStamp + "_LogFile.txt", true);
                    sw.WriteLine(DateTime.Now.ToString(InvC) + ": " + Message);
                    sw.Flush();
                    sw.Close();
                }
                catch
                {
                }
            }
        }

        public static void WriteErrorLog(string Message)
        {
            var threadStart = new ParameterizedThreadStart(WriteErrorLogStr);
            Thread initialseDataTablesThread = new Thread(threadStart);
            initialseDataTablesThread.Start(Message);
        }
    }
}
