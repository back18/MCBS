using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MCBS.WpfApp.Services
{
    public interface IDownloadCompletedHook<T> : IDisposable
    {
        public MemoryStream? CloneStream { get; }

        public bool IsBinding { get; }

        public bool IsReady { get; }

        public Task<T> LoadContentAsync();

        public void Binding(IDownloadViewModel downloadViewModel);

        public void Unbinding();
    }
}
