using QuanLib.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Namespaces
{
    public class SystemResourceNamespace : NamespaceBase
    {
        public SystemResourceNamespace(string @namespace) : base(@namespace)
        {
            CursorsNamespace = AddNamespace<CursorsNamespace>("Cursors");
            CursorIndexFile = Combine("CursorIndex.json");
            DefaultFontFile = Combine("DefaultFont.bdf");
            DefaultIconFile = Combine("DefaultIcon.png");
        }

        public CursorsNamespace CursorsNamespace { get; }

        public string CursorIndexFile { get; }

        public string DefaultFontFile { get; }

        public string DefaultIconFile { get; }
    }
}
