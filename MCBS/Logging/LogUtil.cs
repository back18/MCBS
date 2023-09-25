using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Logging
{
    public static class LogUtil
    {
        static LogUtil()
        {
            SaveOldLogFile();

            XmlConfigurator.Configure(new FileInfo(SR.McbsDirectory.Configs.Log4Net));
            _repository = (Hierarchy)LogManager.GetRepository();

            PatternLayout layout = new("[%date{HH:mm:ss}] [%t/%p] [%c]: %m%n");
            layout.ActivateOptions();

            _console = new();
            _console.Threshold = Level.All;
            _console.Layout = layout;
            _console.ActivateOptions();

            _file = new();
            _file.Threshold = Level.All;
            _file.Layout = layout;
            _file.Encoding = Encoding.UTF8;
            _file.File = SR.McbsDirectory.Logs.Latest;
            _file.LockingModel = new FileAppender.MinimalLock();
            _file.ActivateOptions();

            MainLogger = GetLogger("McbsMain");
        }

        private static readonly Hierarchy _repository;

        private static readonly ConsoleAppender _console;

        private static readonly RollingFileAppender _file;

        public static LogImpl MainLogger { get; }

        public static LogImpl GetLogger(string name)
        {
            Logger logger = _repository.LoggerFactory.CreateLogger(_repository, name);
            logger.Hierarchy = _repository;
            logger.Parent = _repository.Root;
            logger.Level = Level.All;
            logger.Additivity = false;
            logger.AddAppender(_console);
            logger.AddAppender(_file);

            return new(logger);
        }

        public static void EnableConsoleOutput()
        {
            _console.Threshold = Level.All;
        }

        public static void DisableConsoleOutput()
        {
            _console.Threshold = Level.Off;
        }

        private static void SaveOldLogFile()
        {
            FileInfo logFileInfo = new(SR.McbsDirectory.Logs.Latest);
            if (!logFileInfo.Exists)
                return;

            byte[] logBytes = File.ReadAllBytes(SR.McbsDirectory.Logs.Latest);
            string format = SR.McbsDirectory.Logs.Combine(logFileInfo.LastWriteTime.ToString("yyyy-MM-dd") + "-{0}.log.gz");
            string path = string.Format(format, 1);
            for (int i = 2; i < int.MaxValue; i++)
            {
                if (!File.Exists(path))
                    break;
                else
                    path = string.Format(format, i);
            }

            using FileStream fileStream = new(path, FileMode.Create);
            using GZipStream gZipStream = new(fileStream, CompressionMode.Compress);
            gZipStream.Write(logBytes, 0, logBytes.Length);
            logFileInfo.Delete();
        }
    }
}
