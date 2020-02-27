using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeutrinoStudio.Shell.Helpers
{
    public static class LogHelper
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger("logger");

        public static void Log(LogType type, string message) => Log(new LogMessage(type, message));

        public static void Log(LogMessage logMessage)
        {
            LogEvent?.Invoke(logMessage);
            switch (logMessage.Type)
            {
                case LogType.Warn:
                    logger.Warn(logMessage.Message);
                    break;
                case LogType.Error:
                    logger.Error(logMessage.Message);
                    break;
                case LogType.Fatal:
                    logger.Fatal(logMessage.Message);
                    break;
                case LogType.Debug:
                    logger.Debug(logMessage.Message);
                    break;
                default:
                    logger.Info(logMessage.Message);
                    break;
            }
        }

        public static event LogEventHandler LogEvent;
    }

    public enum LogType
    {
        Debug = 0,
        Info = 1,
        Warn = 2,
        Error = 3,
        Fatal = 4
    }

    public class LogMessage
    {
        public LogType Type { get; set; }
        public string Message { get; set; }

        public LogMessage(LogType type, string message)
        {
            Type = type;
            Message = message;
        }
    }

    public delegate void LogEventHandler(LogMessage logMessage);
}
