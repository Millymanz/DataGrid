using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using log4net;
using log4net.Config;
using System.IO;

namespace DatabaseWcfService
{
    public static class Logger
    {
        public static readonly ILog log = LogManager.GetLogger(typeof(Logger));


        public static void Setup()
        {
            log4net.Config.XmlConfigurator.Configure(new FileInfo("DatabaseServiceHost.exe.config"));
            log.Info("Application Starts");
        }

        //Used to demonstrate making log calls from a different namespace
        public static void TestLogger()
        {
            log4net.GlobalContext.Properties["testProperty"] = "This is my test property information";

            log.Debug("Other Class - Debug logging");
            log.Info("Other Class - Info logging");
            log.Warn("Other Class - Warn logging");
            log.Error("Other Class - Error logging");
        }
    }
}
