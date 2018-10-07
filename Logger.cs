using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace SimpleHTTPServer
{
    static class Logger
    {

        public static int MinimumLogLevel { get; set; } = 0;

        public static void Log(string message, int logLevel = 0)
        {
            if (logLevel >= MinimumLogLevel)
            {
                string time = DateTime.UtcNow.ToString("HH:mm:ss.fff");
                string thread = Thread.CurrentThread.ManagedThreadId.ToString();
                message = time + " - " + thread + " - " + message;
                Console.WriteLine(message);

                LogToFile(message);
            }
        }

        private static void LogToFile(string message)
        {
            // To do: Add File logging
        }
    }
}
