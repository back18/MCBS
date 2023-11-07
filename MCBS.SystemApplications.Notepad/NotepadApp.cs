using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MCBS.Application;

namespace MCBS.SystemApplications.Notepad
{
    public class NotepadApp : IProgram
    {
        public const string ID = "Notepad";

        public const string Name = "记事本";

        public int Main(string[] args)
        {
            string? path = null;
            if (args.Length > 0)
                path = args[0];

            this.RunForm(new NotepadForm(path));
            return 0;
        }

        public void Exit()
        {
            throw new NotImplementedException();
        }
    }
}
