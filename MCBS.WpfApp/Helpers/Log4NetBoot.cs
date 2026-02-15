using QuanLib.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace MCBS.WpfApp.Helpers
{
    public static class Log4NetBoot
    {
        private const string CONDIG_ASSEMBLY = "MCBS.Common";
        private const string CONDIG_RESOURCE = ".SystemResource.log4net.xml";
        private const string CONFIG_PATH = "MCBS|Config|log4net.xml";
        private const string LOG_PATH = "MCBS|Logs|Latest.log";

        public static ILog4NetProvider Load()
        {
            string logFilePath = Path.Combine(LOG_PATH.Split('|'));
            Stream configStream = LoadConfigStream();
            Log4NetProvider provider = new(logFilePath, configStream, true);
            Log4NetManager.LoadInstance(new(provider));
            return provider;
        }

        private static Stream LoadConfigStream()
        {
            string configPath = Path.Combine(CONFIG_PATH.Split('|'));
            if (File.Exists(configPath) && new FileInfo(configPath).Length > 0)
            {
                try
                {
                    return LoadCinfigFromFile(configPath);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("LOGBOOT LOAD ERROR: ");
                    Console.Error.WriteLine(ex.ToString());

                    Stream stream = LoadCinfigFromResource(CONDIG_ASSEMBLY, CONDIG_RESOURCE);
                    TrySaveConfig(configPath, stream);
                    stream.Seek(0, SeekOrigin.Begin);
                    return stream;
                }
            }
            else
            {
                Stream stream = LoadCinfigFromResource(CONDIG_ASSEMBLY, CONDIG_RESOURCE);
                TrySaveConfig(configPath, stream);
                stream.Seek(0, SeekOrigin.Begin);
                return stream;
            }
        }

        private static Stream LoadCinfigFromResource(string assemblyName, string resourcePath)
        {
            string path = assemblyName + resourcePath;
            Assembly assembly = Assembly.Load(assemblyName);
            Stream stream = assembly.GetManifestResourceStream(path) ?? throw new FileNotFoundException(path);
            return stream;
        }

        private static FileStream LoadCinfigFromFile(string filePath)
        {
            FileStream fileStream = File.OpenRead(filePath);
            return fileStream;
        }

        private static void TrySaveConfig(string filePath, Stream stream)
        {
            try
            {
                using FileStream fileStream = File.Create(filePath);
                stream.CopyTo(fileStream);
                fileStream.Flush();

            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("LOGBOOT SAVE ERROR: ");
                Console.Error.WriteLine(ex.ToString());
            }
        }
    }
}
