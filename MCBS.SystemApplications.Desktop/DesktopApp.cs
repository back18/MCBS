using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MCBS.Application;

namespace MCBS.SystemApplications.Desktop
{
    public class DesktopApp : IProgram
    {
        public const string Id = "System.Desktop";

        public const string Name = "系统桌面";

        public int Main(string[] args)
        {
            this.RunForm(new DesktopForm());
            return 0;
        }

        public void Exit()
        {
            throw new NotImplementedException();
        }
    }
}
