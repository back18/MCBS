using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Rendering
{
    public readonly struct OverwriteMapping
    {
        public OverwriteMapping(Point indexPosition, Point basePosition, Point overwritePosition)
        {
            IndexPosition = indexPosition;
            BasePosition = basePosition;
            OverwritePosition = overwritePosition;
        }

        public readonly Point IndexPosition;

        public readonly Point BasePosition;

        public readonly Point OverwritePosition;
    }
}
