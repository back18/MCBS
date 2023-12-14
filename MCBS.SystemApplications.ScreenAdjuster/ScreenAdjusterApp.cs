using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MCBS.Application;

namespace MCBS.SystemApplications.ScreenAdjuster
{
    public class ScreenAdjusterApp : IProgram
    {
        public const string ID = "System.ScreenAdjuster";

        public const string Name = "屏幕调节器";

        public int Main(string[] args)
        {
            this.RunForm(new ScreenAdjusterForm());
            return 0;
        }

        public void Exit()
        {
            throw new NotImplementedException();
        }
    }
}
