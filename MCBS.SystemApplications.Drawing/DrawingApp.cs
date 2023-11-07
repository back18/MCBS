using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MCBS.Application;

namespace MCBS.SystemApplications.Drawing
{
    public class DrawingApp : IProgram
    {
        public const string ID = "Drawing";

        public const string Name = "绘画";

        public int Main(string[] args)
        {
            string? path = null;
            if (args.Length > 0)
                path = args[0];

            this.RunForm(new DrawingForm(path));
            return 0;
        }

        public void Exit()
        {
            throw new NotImplementedException();
        }
    }
}
