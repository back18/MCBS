using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.Common.Services
{
    public interface IFFmpegLoader
    {
        public Task LoadAsync();
    }
}
