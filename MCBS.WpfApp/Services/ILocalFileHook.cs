using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.WpfApp.Services
{
    public interface ILocalFileHook<T>
    {
        public bool IsReady { get; }

        public Task<T> LoadContentAsync();
    }
}
