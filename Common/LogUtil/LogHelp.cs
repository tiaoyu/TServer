using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using log4net;

/// <summary>
/// 日志相关工具包
/// </summary>
namespace Common.LogUtil
{
    /// <summary>
    /// 
    /// </summary>
    public partial class LogHelp
    {
        private ILog _log;
        private static log4net.Repository.ILoggerRepository repository;

        public static void Init(string configFile)
        {
            repository = LogManager.CreateRepository("NETCoreRepository");
            log4net.Config.XmlConfigurator.Configure(repository, new FileInfo(configFile));
        }

        public static LogHelp GetLogger(Type t)
        {
            return new LogHelp
            {
                _log = LogManager.GetLogger("NETCoreRepository", t)
            };
        }
    }

    public partial class LogHelp
    {
        public void Debug(string msg)
        {
            _log.Debug(msg);
        }

        public void Info(string msg)
        {
            _log.Info(msg);
        }

        public void Warn(string msg)
        {
            _log.Warn(msg);
        }

        public void Error(string msg)
        {
            _log.Error(msg);
        }

        public void Fatal(string msg)
        {
            _log.Fatal(msg);
        }
        public void DebugFormat(string msg, params object[] param)
        {
            _log.DebugFormat(msg, param);
        }

        public void InfoFormat(string msg, params object[] param)
        {
            _log.InfoFormat(msg, param);
        }

        public void WarnFormat(string msg, params object[] param)
        {
            _log.WarnFormat(msg, param);
        }

        public void ErrorFormat(string msg, params object[] param)
        {
            _log.ErrorFormat(msg, param);
        }

        public void FatalFormat(string msg, params object[] param)
        {
            _log.FatalFormat(msg, param);
        }

        public static void TestLog()
        {
            LogHelp log = LogHelp.GetLogger(typeof(LogHelp));
            log.Debug("Hello, Logger!");
        }
    }
}
