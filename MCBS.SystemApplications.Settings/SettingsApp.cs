using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MCBS.Application;

namespace MCBS.SystemApplications.Settings
{
    public class SettingsApp : IProgram
    {
        public const string ID = "System.Settings";

        public const string Name = "设置";

        public int Main(string[] args)
        {
            this.RunForm(new SettingsForm());
            return 0;
        }

        public void Exit()
        {
            throw new NotImplementedException();
        }
    }
}
