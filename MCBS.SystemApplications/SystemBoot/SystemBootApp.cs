using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.SystemApplications.SystemBoot
{
    public class SystemBootApp : Application
    {
        public const string ID = "SystemBoot";

        public const string Name = "系统引导";

        public override object? Main(string[] args)
        {
            RunForm(new SystemBootForm());
            return null;
        }
    }
}
