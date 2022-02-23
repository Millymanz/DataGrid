using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Globalization;

namespace RealTimeRawDataService
{
    public static class Library
    {
        public static void WriteErrorLog(Exception ex)
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

                sw = new StreamWriter(logFolder +"\\" + dateTimeStamp + "_LogFile.txt", true);
                sw.WriteLine(DateTime.Now.ToString() + ": " + ex.Source.ToString().Trim() + "; " + ex.Message.ToString().Trim());
                sw.Flush();
                sw.Close();
            }
            catch
            {
            }
        }

        public static void WriteInfoLog(string filename, string Message)
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

                sw = new StreamWriter(logFolder + "\\" + dateTimeStamp + filename + "_LogFile.txt", true);
                sw.WriteLine(DateTime.Now.ToString(InvC) + ": " + Message);
                sw.Flush();
                sw.Close();
            }
            catch
            {
            }
        }


        public static void WriteErrorLog(string Message)
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
}
