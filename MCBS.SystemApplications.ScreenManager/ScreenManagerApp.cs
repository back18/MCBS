using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MCBS.Application;

namespace MCBS.SystemApplications.ScreenManager
{
    public class ScreenManagerApp : IProgram
    {
        public const string Id = "System.ScreenManager";

        public const string Name = "屏幕管理器";

        public int Main(string[] args)
        {
            this.RunForm(new ScreenManagerForm());
            return 0;
        }

        public void Exit()
        {
            throw new NotImplementedException();
        }
    }
}
