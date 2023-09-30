using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MCBS.Application;

namespace MCBS.SystemApplications.TaskManager
{
    public class TaskManagerApp : ApplicationBase
    {
        public const string ID = "TaskManager";

        public const string Name = "任务管理器";

        public override object? Main(string[] args)
        {
            RunForm(new TaskManagerForm());
            return null;
        }
    }
}
