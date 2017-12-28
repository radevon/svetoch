using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PumpVisualizer
{
    public static class Loging
    {
        public static void Log(Logger loger, LogMessage message)
        {
            loger.LogToFile(message);
            loger.LogToDatabase(message);
        }
    }
}