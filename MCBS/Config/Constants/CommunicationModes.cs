using QuanLib.Minecraft.Instance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Config.Constants
{
    public static class CommunicationModes
    {
        public const string MCAPI = IMcapiInstance.IDENTIFIER;

        public const string RCON = IRconInstance.IDENTIFIER;

        public const string CONSOLE = IConsoleInstance.IDENTIFIER;

        public const string HYBRID = IHybridInstance.IDENTIFIER;
    }
}
