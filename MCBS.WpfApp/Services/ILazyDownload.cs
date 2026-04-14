using Downloader;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.WpfApp.Services
{
    public interface ILazyDownload : IDownload
    {
        public bool HasOwner { get; }

        public void SetOwner(IDownload owner);
    }
}
