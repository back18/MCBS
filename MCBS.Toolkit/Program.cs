using QuanLib.CommandLine;
using QuanLib.CommandLine.ConsoleTerminal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Level = QuanLib.CommandLine.Level;

namespace MCBS.Toolkit
{
    public static class Program
    {
        public static readonly CommandSystem CommandSystem = new(new(Level.Root));

        private static void Main(string[] args)
        {
            RegisterCommands();
            CommandSystem.Start();
        }

        private static void RegisterCommands()
        {
            CommandSystem.Pool.AddCommand(new(new("build TextureIndex"), CommandFunc.GetFunc(BuildUtil.BuildTextureIndex)));
        }
    }
}
