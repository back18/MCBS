using MCBS.Application;
using MCBS.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.SystemApplications.Services
{
    public class ServicesApp : IServicesProgram
    {
        public ServicesApp()
        {
            RootForm = new ServicesForm();
        }

        public const string ID = "System.Services";

        public const string Name = "系统服务";

        public IRootForm RootForm { get; }

        public int Main(string[] args)
        {
            this.RunForm(RootForm);
            return 0;
        }

        public void Exit()
        {
            throw new NotImplementedException();
        }
    }
}
