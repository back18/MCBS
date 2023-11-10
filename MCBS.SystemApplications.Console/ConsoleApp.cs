using MCBS.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.SystemApplications.Console
{
    public class ConsoleApp : IProgram
    {
        public const string ID = "System.Console";

        public const string Name = "控制台";

        public int Main(string[] args)
        {
            this.RunForm(new ConsoleForm());
            return 0;
        }

        public void Exit()
        {
            throw new NotImplementedException();
        }
    }
}
