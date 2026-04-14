using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.WpfApp.Services
{
    public interface IScopeProvider
    {
        public string ScopeToken { get; }
    }
}
