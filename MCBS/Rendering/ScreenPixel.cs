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
            ArgumentNullException.ThrowIfNull(blockId, nameof(blockId));

            Position = position;
            BlockId = blockId;
        }

        public readonly Point Position;

        public readonly string BlockId;
    }
}
