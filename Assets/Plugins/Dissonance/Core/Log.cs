using System;
using System.Diagnostics;
using System.Threading;
using Dissonance.Config;
using Dissonance.Datastructures;

namespace Dissonance
{
    public enum LogLevel
    {
        /// <summary>
        /// Per-frame diagnostic events.
        /// </summary>
        Trace = 0,

        /// <summary>
        /// Significant diagnostic events.
        /// </summary>
        Debug = 1,

        /// <summary>
        /// Significant events which occur under normal operation.
        /// </summary>
        Info = 2,

        /// <summary>
        /// Non-critical errors, which deserve investigation.
        /// </summary>
        Warn = 3,

        /// <summary>
        /// Critical errors caused by external factors, outside of missuse or bugs.
        /// </summary>
        Error = 4
    }

    public enum LogCategory
    {
        Core,
        Recording,
        Network,
        Playback
    }

    public static class Logs
    {
        public static Log Create(LogCategory category, string name)
        {
            return Create((int)category, name);
        }

        public static Log Create(int category, string name)
        {
            return new Log(category, name);
        }

        public static void SetLogLevel(LogCategory category, LogLevel level)
        {
            SetLogLevel((int)category, level);
        }

        public static void SetLogLevel(int category, LogLevel level)
        {
            DebugSettings.Instance.SetLevel(category, level);
        }

        public static LogLevel GetLogLevel(LogCategory category)
        {
            return GetLogLevel((int)category);
        }

        public static LogLevel GetLogLevel(int category)
        {
            return DebugSettings.Instance.GetLevel(category);
        }

        #region multithreading
        private struct LogMessage
        {
            private readonly Action<string> _log;
            private readonly string _message;

            public LogMessage(string message, Action<string> log)
            {
                _message = message;
                _log = log;
            }

            public void Log()
            {
                _log(_message);
            }
        }

        private static readonly TransferBuffer<LogMessage> _logs = new TransferBuffer<LogMessage>(512);
        private static Thread _main;

        internal static void WriteMultithreadedLogs()
        {
            if (_main == null)
                _main = Thread.CurrentThread;

            LogMessage msg;
            while (_logs.Read(out msg))
                msg.Log();
        }

        internal static void SendLogMessage(string message, Action<string> log)
        {
#if NCRUNCH
            Console.WriteLine(message);
#else
            if (_main == Thread.CurrentThread)
                log(message);
            else
                _logs.Write(new LogMessage(message, log));
#endif
        }
        #endregion
    }

    public class Log
    {
        private readonly string _format;
        private readonly int _category;

        internal Log(int category, string name)
        {
            _category = category;
            _format = "[Dissonance:" + (LogCategory) category + "] " + name + ": {0}";
        }
        
        [DebuggerHidden]
        private bool ShouldLog(LogLevel level)
        {
            return level >= Logs.GetLogLevel(_category);
        }

        #region Logging implementation
        [DebuggerHidden]
        private void WriteLog(LogLevel level, string message)
        {
            if (!ShouldLog(level))
                return;

            var msg = string.Format(_format, message);

            switch (level)
            {
                case LogLevel.Trace:
                    Logs.SendLogMessage(string.Format("TRACE {0}", msg), UnityEngine.Debug.Log);
                    break;

                case LogLevel.Debug:
                    Logs.SendLogMessage(string.Format("DEBUG {0}", msg), UnityEngine.Debug.Log);
                    break;

                case LogLevel.Info:
                    Logs.SendLogMessage(msg, UnityEngine.Debug.Log);
                    break;

                case LogLevel.Warn:
                    Logs.SendLogMessage(msg, UnityEngine.Debug.LogWarning);
                    break;

                case LogLevel.Error:
                    Logs.SendLogMessage(msg, UnityEngine.Debug.LogError);
                    break;

                default:
                    throw new ArgumentOutOfRangeException("level", level, null);
            }
        }

        [DebuggerHidden]
        private void WriteLogFormat(LogLevel level, string format, params object[] parameters)
        {
            if (!ShouldLog(level))
                return;

            WriteLog(level, string.Format(format, parameters));
        }

        [DebuggerHidden]
        private void WriteLogFormat<TA>(LogLevel level, string format, TA p0)
        {
            if (!ShouldLog(level))
                return;

            WriteLog(level, string.Format(format, p0));
        }

        [DebuggerHidden]
        private void WriteLogFormat<TA, TB>(LogLevel level, string format, TA p0, TB p1)
        {
            if (!ShouldLog(level))
                return;

            WriteLog(level, string.Format(format, p0, p1));
        }

        [DebuggerHidden]
        private void WriteLogFormat<TA, TB, TC>(LogLevel level, string format, TA p0, TB p1, TC p2)
        {
            if (!ShouldLog(level))
                return;

            WriteLog(level, string.Format(format, p0, p1, p2));
        }
#endregion

        #region Trace
        [DebuggerHidden]
        [Conditional("DEBUG")]
        public void Trace(string message)
        {
            WriteLog(LogLevel.Trace, message);
        }

        [DebuggerHidden]
        [Conditional("DEBUG")]
        public void Trace(string format, params object[] parameters)
        {
            WriteLogFormat(LogLevel.Trace, format, parameters);
        }

        [DebuggerHidden]
        [Conditional("DEBUG")]
        public void Trace<TA>(string format, TA p0)
        {
            WriteLogFormat(LogLevel.Trace, format, p0);
        }

        [DebuggerHidden]
        [Conditional("DEBUG")]
        public void Trace<TA, TB>(string format, TA p0, TB p1)
        {
            WriteLogFormat(LogLevel.Trace, format, p0, p1);
        }
#endregion

        #region Debug
        [DebuggerHidden]
        [Conditional("DEBUG")]
        public void Debug(string message)
        {
            WriteLog(LogLevel.Debug, message);
        }

        [DebuggerHidden]
        [Conditional("DEBUG")]
        public void Debug(string format, params object[] parameters)
        {
            WriteLogFormat(LogLevel.Debug, format, parameters);
        }

        [DebuggerHidden]
        [Conditional("DEBUG")]
        public void Debug<TA>(string format, TA p0)
        {
            WriteLogFormat(LogLevel.Debug, format, p0);
        }

        [DebuggerHidden]
        [Conditional("DEBUG")]
        public void Debug<TA, TB>(string format, TA p0, TB p1)
        {
            WriteLogFormat(LogLevel.Debug, format, p0, p1);
        }
#endregion

        #region info
        [DebuggerHidden]
        public void Info(string message)
        {
            WriteLog(LogLevel.Info, message);
        }

        [DebuggerHidden]
        public void Info(string format, params object[] parameters)
        {
            WriteLogFormat(LogLevel.Info, format, parameters);
        }

        [DebuggerHidden]
        public void Info<TA>(string format, TA p0)
        {
            WriteLogFormat(LogLevel.Info, format, p0);
        }

        [DebuggerHidden]
        public void Info<TA, TB>(string format, TA p0, TB p1)
        {
            WriteLogFormat(LogLevel.Info, format, p0, p1);
        }
#endregion

        #region warn
        [DebuggerHidden]
        public void Warn(string message)
        {
            WriteLog(LogLevel.Warn, message);
        }

        [DebuggerHidden]
        public void Warn(string format, params object[] parameters)
        {
            WriteLogFormat(LogLevel.Warn, format, parameters);
        }

        [DebuggerHidden]
        public void Warn<TA>(string format, TA p0)
        {
            WriteLogFormat(LogLevel.Warn, format, p0);
        }

        [DebuggerHidden]
        public void Warn<TA, TB>(string format, TA p0, TB p1)
        {
            WriteLogFormat(LogLevel.Warn, format, p0, p1);
        }
#endregion

        #region error
        [DebuggerHidden]
        public void Error(string message)
        {
            WriteLog(LogLevel.Error, message);
        }

        [DebuggerHidden]
        public void Error(string format, params object[] parameters)
        {
            WriteLogFormat(LogLevel.Error, format, parameters);
        }

        [DebuggerHidden]
        public void Error<TA>(string format, TA p0)
        {
            WriteLogFormat(LogLevel.Error, format, p0);
        }

        [DebuggerHidden]
        public void Error<TA, TB>(string format, TA p0, TB p1)
        {
            WriteLogFormat(LogLevel.Error, format, p0, p1);
        }

        [DebuggerHidden]
        public void Error<TA, TB, TC>(string format, TA p0, TB p1, TC p2)
        {
            WriteLogFormat(LogLevel.Error, format, p0, p1, p2);
        }
#endregion

#region throw
        [DebuggerHidden]
        public DissonanceException UserError(string problem, string likelyCause, string documentationLink, string guid)
        {
            var message = string.Format(
                "Error: {0}! This is likely caused by \"{1}\", see the documentation at \"{2}\" or visit the community at \"http://placeholder-software.co.uk/dissonance/community\" to get help. Error ID: {3}",
                problem,
                likelyCause,
                documentationLink,
                guid
            );

            return new DissonanceException(
                string.Format(_format, message)
            );
        }

        public string PossibleBugMessage(string problem, string guid)
        {
            return string.Format(
                "Error: {0}! This is probably a bug in Dissonance, we're sorry! Please report the bug on the issue tracker \"https://github.com/Placeholder-Software/Dissonance/issues\". You could also seek help on the community at \"http://placeholder-software.co.uk/dissonance/community\" to get help for a temporary workaround. Error ID: {1}",
                problem,
                guid
            );
        }

        [DebuggerHidden]
        public DissonanceException PossibleBug(string problem, string guid)
        {
            return new DissonanceException(PossibleBugMessage(problem, guid));
        }

        [DebuggerHidden]
        public Exception PossibleBug<T>(Func<string, T> factory, string problem, string guid) where T : Exception
        {
            return factory(PossibleBugMessage(problem, guid));
        }
#endregion
    }
}
