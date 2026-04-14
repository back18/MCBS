using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.WpfApp.Services.Implementations
{
    public class GuidScopeProvider : IScopeProvider
    {
        public string ScopeToken { get; } = Guid.NewGuid().ToString();
    }
}
