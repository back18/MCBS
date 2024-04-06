﻿using QuanLib.Minecraft.Instance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Config.Constants
{
    public static class CommunicationModes
    {
        public const string MCAPI = IMcapiInstance.INSTANCE_KEY;

        public const string RCON = IRconInstance.INSTANCE_KEY;

        public const string CONSOLE = IConsoleInstance.INSTANCE_KEY;

        public const string HYBRID = IHybridInstance.INSTANCE_KEY;
    }
}
