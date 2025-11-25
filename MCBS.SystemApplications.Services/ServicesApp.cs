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
            ScreenView = new ScreenView();
        }

        public const string Id = "System.Services";

        public const string Name = "系统服务";

        public IScreenView ScreenView { get; }

        public IRootForm RootForm => ScreenView.RootForm;

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
