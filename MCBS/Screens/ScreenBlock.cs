using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Screens
{
    public readonly struct ScreenBlock(Point position, string blockId)
    {
        public readonly Point Position = position;

        public readonly string BlockId = blockId;
    }
}
