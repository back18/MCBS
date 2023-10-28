using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Rendering
{
    public readonly struct ScreenPixel
    {
        public ScreenPixel(Point position, string blockId)
        {
            if (blockId is null)
                throw new ArgumentNullException(nameof(blockId));

            Position = position;
            BlockId = blockId;
        }

        public readonly Point Position;

        public readonly string BlockId;
    }
}
