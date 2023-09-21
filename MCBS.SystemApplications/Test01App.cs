using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.SystemApplications
{
    public class Test01App : Application
    {
        public const string ID = "Test01";

        public const string Name = "测试01";

        public override object? Main(string[] args)
        {
            RunForm(new Test01Form());
            return null;
        }
    }
}
