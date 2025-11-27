using log4net.Core;
using QuanLib.Core;
using QuanLib.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.SystemApplications.Console
{
    public class StandardStreamReader : UnmanagedRunnable
    {
        public StandardStreamReader(StreamReader streamReader) : base(Log4NetManager.Instance.GetProvider())
        {
            ArgumentNullException.ThrowIfNull(streamReader, nameof(streamReader));

            _streamReader = streamReader;
            _output = new();
        }

        private readonly Lock _lock = new();

        private readonly StreamReader _streamReader;

        private readonly StringBuilder _output;

        public string GetOutput()
        {
            lock (_lock)
            {
                if (_output.Length == 0)
                    return string.Empty;

                string output = _output.ToString();
                _output.Clear();
                return output;
            }
        }

        protected override void Run()
        {
            while (IsRunning)
            {
                int c = _streamReader.Read();
                if (c == -1)
                {
                    IsRunning = false;
                    return;
                }

                _output.Append((char)c);
            }
        }

        protected override void DisposeUnmanaged()
        {
            _streamReader.Dispose();
        }
    }
}
