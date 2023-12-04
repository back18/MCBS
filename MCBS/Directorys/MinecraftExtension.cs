using QuanLib.Minecraft.Directorys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Directorys
{
    public static class MinecraftExtension
    {
        public static McbsDataDirectory GetMcbsDataDirectory(this WorldDirectory source)
        {
            return new(source.Combine("McbsData"));
        }
    }
}
