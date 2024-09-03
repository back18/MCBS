using MCBS.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.SystemApplications.FileExplorer
{
    public class FileExplorerApp : IProgram
    {
        public const string ID = "System.FileExplorer";

        public const string Name = "资源管理器";

        public int Main(string[] args)
        {
            string? path = null;
            if (args.Length > 0)
                path = args[0];

            this.RunForm(new FileExplorerForm(path));
            return 0;
        }

        public void Exit()
        {
            throw new NotImplementedException();
        }
    }
}
