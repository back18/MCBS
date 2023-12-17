﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MCBS.Application;

namespace MCBS.SystemApplications.ScreenController
{
    public class ScreenControllerApp : IProgram
    {
        public const string ID = "System.ScreenController";

        public const string Name = "屏幕控制器";

        public int Main(string[] args)
        {
            this.RunForm(new ScreenControllerForm());
            return 0;
        }

        public void Exit()
        {
            throw new NotImplementedException();
        }
    }
}
