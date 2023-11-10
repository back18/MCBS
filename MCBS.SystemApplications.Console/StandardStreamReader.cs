using log4net.Core;
using MCBS.Logging;
using QuanLib.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.SystemApplications.Console
{
    public class StandardStreamReader : UnmanagedRunnable
    {
        public StandardStreamReader(StreamReader streamReader) : base(LogUtil.GetLogger)
        {
            _streamReader = streamReader ?? throw new ArgumentNullException(nameof(streamReader));
            _output = new();
        }

        private readonly StreamReader _streamReader;

        private readonly StringBuilder _output;

        public string GetOutput()
        {
            lock (_output)
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
