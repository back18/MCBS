using MCBS.Application;
using MCBS.SystemApplications.FileExplorer.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.SystemApplications.FileExplorer
{
    public class FileExplorerApp : IProgram
    {
        public const string ID = "FileExplorer";

        public const string Name = "资源管理器";

        public FileExplorerConfig FileExplorerConfig => _FileExplorerConfig ?? throw new InvalidOperationException();
        private FileExplorerConfig? _FileExplorerConfig;

        public int Main(string[] args)
        {
            FileExplorerConfig.CreateIfNotExists();
            _FileExplorerConfig = FileExplorerConfig.Load();

            string? path = null;
            if (args.Length > 0)
                path = args[0];

            this.RunForm(new FileExplorerForm(FileExplorerConfig.RootDirectory, path));
            return 0;
        }

        public void Exit()
        {
            throw new NotImplementedException();
        }
    }
}
