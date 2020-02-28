using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NeutrinoStudio.Shell.Helpers
{
    /// <summary>
    /// LogHelper for NEUShell.
    /// </summary>
    public sealed class LogHelper : INotifyPropertyChanged
    {
        /// <summary>
        /// Internal initialize.
        /// </summary>
        private LogHelper()
        {
            _logger = log4net.LogManager.GetLogger("logger");
            _logList = new List<LogMessage>();
        }

        /// <summary>
        /// Get the current LogHelper.
        /// </summary>
        public static LogHelper Current => _current ?? (_current = new LogHelper());

        /// <summary>
        /// The current LogHelper.
        /// </summary>
        private static LogHelper _current;

        /// <summary>
        /// The core logger.
        /// </summary>
        private readonly log4net.ILog _logger;

        /// <summary>
        /// Get the log list.
        /// </summary>
        public List<LogMessage> LogList => _logList ?? (_logList = new List<LogMessage>());

        /// <summary>
        /// The log list.
        /// </summary>
        private List<LogMessage> _logList;

        public void Log(LogType type, string message) => Log(new LogMessage(type, message));

        public void Log(LogMessage logMessage)
        {
            OnPropertyChanged(logMessage);
            switch (logMessage.Type)
            {
                case LogType.Warn:
                    _logger.Warn(logMessage.Message);
                    break;
                case LogType.Error:
                    _logger.Error(logMessage.Message);
                    break;
                case LogType.Fatal:
                    _logger.Fatal(logMessage.Message);
                    break;
                case LogType.Debug:
                    _logger.Debug(logMessage.Message);
                    break;
                default:
                    _logger.Info(logMessage.Message);
                    break;
            }
        }

        public event LogEventHandler LogEvent;
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(LogMessage logMessage, [CallerMemberName] string propertyName = null)
        {
            LogEvent?.Invoke(logMessage);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private static Dictionary<LogType, string> _logTypeDictionary = new Dictionary<LogType, string>
        {
            { LogType.Debug, "调试" },
            { LogType.Info, "信息" },
            { LogType.Warn, "警告" },
            { LogType.Error, "错误" },
            { LogType.Fatal, "灾难性错误" }
        };

        public static string GetLogType(LogType logType)
        {
            string value;
            if (_logTypeDictionary.TryGetValue(logType, out value)) return value;
            return "未知";
        }
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
        public LogType Type { get; }
        public string Message { get; }

        public string DisplayMessage => $"[{LogHelper.GetLogType(Type)}]{Message}";

        public LogMessage(LogType type, string message)
        {
            Type = type;
            Message = message;
        }
    }

    public delegate void LogEventHandler(LogMessage logMessage);
}
