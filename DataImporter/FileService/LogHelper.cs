using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FileService
{
    public static class LogHelper
    {
        private static ILog logger = log4net.LogManager.GetLogger("log");

        public static void Info(string message, Exception ex = null)
        {
            logger.Info(message, ex);
        }

        public static void Warn(string message, Exception ex = null)
        {
            logger.Warn(message, ex);
        }

        public static void Error(string message, Exception ex = null)
        {
            logger.Error(message, ex);
        }
    }
}
