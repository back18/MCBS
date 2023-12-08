using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Screens
{
    public readonly struct ScreenBlock(Point position, string blockID)
    {
        public Point Position { get; } = position;

        public string BlockID { get; } = blockID;
    }
}
