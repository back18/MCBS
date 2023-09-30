using MCBS.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.SystemApplications.Album
{
    public class AlbumApp : ApplicationBase
    {
        public const string ID = "Album";

        public const string Name = "相册";

        public override object? Main(string[] args)
        {
            string? path = null;
            if (args.Length > 0)
                path = args[0];

            RunForm(new AlbumForm(path));
            return null;
        }
    }
}
