using MCBS.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.SystemApplications.QRCodeGenerator
{
    public class QRCodeGeneratorApp : IProgram
    {
        public const string ID = "System.QRCodeGenerator";

        public const string Name = "二维码生成器";

        public int Main(string[] args)
        {
            this.RunForm(new QRCodeGeneratorForm());
            return 0;
        }

        public void Exit()
        {
            throw new NotImplementedException();
        }

    }
}
