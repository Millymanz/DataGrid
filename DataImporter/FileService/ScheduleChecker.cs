using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Data.SqlClient;
using System.Data;
using System.Xml;
using System.Timers;
using System.Text.RegularExpressions;

// added for access to RegistryKey
using Microsoft.Win32;
// added for access to socket classes
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.ComponentModel;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace FileService
{
    public class Scheduling
    {
        public static void StartScheduling()
        {
            var scheduleChecker = new ScheduleChecker();
            ThreadStart threadStart = new ThreadStart(scheduleChecker.RunChecks);
            Thread thread = new Thread(threadStart);
            thread.Start();
        }
    }


    public class ScheduleChecker
    {
        private String _triggerTime;
        System.Timers.Timer actionTimer;

      
        public void SystemEvents_TimeChanged(object sender, EventArgs e)
        {
            actionTimer.Stop();
            actionTimer.Enabled = false;

            actionTimer.Interval = CalculateInterval();

            actionTimer.Enabled = true;
            actionTimer.Start();
        }

        public void RunChecks()
        {
            SystemEvents.TimeChanged += new EventHandler(SystemEvents_TimeChanged);

            _triggerTime = System.Configuration.ConfigurationManager.AppSettings["TIMELIST"].Split('@').LastOrDefault();

            actionTimer = new System.Timers.Timer();

            actionTimer.Elapsed += new ElapsedEventHandler(Task);

            actionTimer.Interval = CalculateInterval();
            actionTimer.AutoReset = true;

            actionTimer.Start();
        }

        private void CriticalImportChecks()
        {
            var previousDay = DateTime.Now.AddDays(-1);

            if (previousDay.DayOfWeek != DayOfWeek.Saturday
                && previousDay.DayOfWeek != DayOfWeek.Sunday)
            {
                var email = new EmailManager(true);

                var dateStr = String.Format("{0}-{1}-{2}", previousDay.Year, previousDay.Month, previousDay.Day);

                var first = DatabaseUtility.GetRowCount("TRADES_TEST_Forex", "SELECT COUNT(*) FROM [dbo].[tblTrades] DD "
+ "WHERE DD.TimeFrame = 'EndOfDay' AND DD.SymbolID = 'EURUSD' AND DD.DateTime ='" + dateStr + "'");

                var second = DatabaseUtility.GetRowCount("TRADES_TEST", "SELECT COUNT(*) FROM [dbo].[tblTrades] DD "
+ "WHERE DD.TimeFrame = 'EndOfDay' AND DD.SymbolID = 'SPX.XO' AND DD.DateTime ='" + dateStr + "'");

                var third = DatabaseUtility.GetRowCount("TRADES_TEST", "SELECT COUNT(*) FROM [dbo].[tblTrades] DD "
+ "WHERE DD.TimeFrame = 'EndOfDay' AND DD.SymbolID = 'QCL#' AND DD.DateTime ='" + dateStr + "'");


                //email
                var message = "";

                if (first != 1)
                {
                    message += "<br/>End of day/Daily Forex data failed to import. <br/> Please check if the data feed has halted or if the importer has failed.";
                    message += "<br/> EURUSD used a test case, previous day data missing.";
                    message += "<br/>";
                }

                if (second != 1)
                {
                    message += "<br/>End of day/Daily Indices data failed to import. <br/> Please check if the data feed has halted or if the importer has failed.";
                    message += "<br/> SP500 used a test case, previous day data missing.";
                    message += "<br/>";
                }

                if (third != 1)
                {
                    message += "<br/>End of day/Daily Commodities data failed to import. <br/> Please check if the data feed has halted or if the importer has failed.";
                    message += "<br/> BRENT CRUDE OIL used a test case, previous day data missing.";
                    message += "<br/>";
                }

                message += "<p> Missing Date Data :: " + dateStr + "</p>";

                email.Send(null, "Critical Failed Consumption and Import", message.ToString(), true, System.Net.Mail.MailPriority.High);
            }
        }

        private void Task(object sender, ElapsedEventArgs e)
        {
            actionTimer.Stop();
            actionTimer.Enabled = false;
            
            CriticalImportChecks();

            //Tell DBM to start import
            actionTimer.Interval = CalculateInterval();
            actionTimer.Enabled = true;
            actionTimer.Start();
        }

        private double CalculateInterval()
        {

            int hours = Convert.ToInt32(_triggerTime.Substring(0, 2));
            int minutes = Convert.ToInt32(_triggerTime.Substring(3, 2));

            DateTime dayDueTime = DateTime.Now;

            DateTime testDateTime = new DateTime(dayDueTime.Year, dayDueTime.Month, dayDueTime.Day,
                hours, minutes, 0);

            if (DateTime.Now > testDateTime)
            {
                dayDueTime = DateTime.Now.AddDays(1);
            }

            DateTime criteriaDateTime = new DateTime(dayDueTime.Year, dayDueTime.Month, dayDueTime.Day,
                hours, minutes, 0);

            long ticks = criteriaDateTime.Ticks - DateTime.Now.Ticks;

            TimeSpan tms = new TimeSpan(ticks);

            return tms.TotalMilliseconds;
        }

        //private double Init()
        //{
        //    int hours = Convert.ToInt32(_triggerTime.Substring(0, 2));
        //    int minutes = Convert.ToInt32(_triggerTime.Substring(3, 2));

        //    DateTime dayDueTime = DateTime.Now;

        //    DateTime testDateTime = new DateTime(dayDueTime.Year, dayDueTime.Month, dayDueTime.Day,
        //        hours, minutes, 0);

        //    DateTime criteriaDateTime = new DateTime(dayDueTime.Year, dayDueTime.Month, dayDueTime.Day,
        //        hours, minutes, 0);

        //    long ticks = criteriaDateTime.Ticks - DateTime.Now.Ticks;

        //    TimeSpan tms = new TimeSpan(ticks);

        //    return tms.TotalMilliseconds;
        //}

    }
}
