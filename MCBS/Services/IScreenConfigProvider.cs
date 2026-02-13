using MCBS.Config;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.Services
{
    public interface IScreenConfigProvider
    {
        public ScreenConfig Config { get; }
    }
}
