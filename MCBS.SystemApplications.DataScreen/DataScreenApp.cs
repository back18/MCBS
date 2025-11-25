using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MCBS.Application;

namespace MCBS.SystemApplications.DataScreen
{
    public class DataScreenApp : IProgram
    {
        public const string Id = "System.DataScreen";

        public const string Name = "数据大屏";

        public int Main(string[] args)
        {
            this.RunForm(new DataScreenForm());
            return 0;
        }

        public void Exit()
        {
            throw new NotImplementedException();
        }
    }
}
