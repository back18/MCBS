using MCBS.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.SystemApplications.Album
{
    public class AlbumApp : IProgram
    {
        public const string Id = "System.Album";

        public const string Name = "相册";

        public int Main(string[] args)
        {
            string? path = null;
            if (args.Length > 0)
                path = args[0];

            this.RunForm(new AlbumForm(path));
            return 0;
        }

        public void Exit()
        {
            throw new NotImplementedException();
        }
    }
}
