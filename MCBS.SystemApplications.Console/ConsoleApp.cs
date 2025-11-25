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
        public const string Id = "System.Console";

        public const string Name = "控制台";

        public int Main(string[] args)
        {
            string? path = null;
            if (args.Length > 0)
                path = args[0];

            this.RunForm(new ConsoleForm(path));
            return 0;
        }

        public void Exit()
        {
            throw new NotImplementedException();
        }
    }
}
