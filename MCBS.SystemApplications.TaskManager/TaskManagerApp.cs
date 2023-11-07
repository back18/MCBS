using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MCBS.Application;

namespace MCBS.SystemApplications.TaskManager
{
    public class TaskManagerApp : IProgram
    {
        public const string ID = "System.TaskManager";

        public const string Name = "任务管理器";

        public int Main(string[] args)
        {
            this.RunForm(new TaskManagerForm());
            return 0;
        }

        public void Exit()
        {
            throw new NotImplementedException();
        }
    }
}
