using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Drawing
{
    public interface IBlockMapping<TPixel>
    {
        public string this[TPixel color] { get; }

        public TPixel this[string blockId] { get; }

        public int ColorCount { get; }

        public int BlockCount { get; }

        public IEnumerable<TPixel> Colors { get; }

        public IEnumerable<string> Blocks { get; }

        public bool ContainsColor(TPixel color);

        public bool ContainsBlock(string blockId);

        public bool TryGetColor(string blockId, [MaybeNullWhen(false)] out TPixel color);

        public bool TryGetBlock(TPixel color, [MaybeNullWhen(false)] out string blockId);
    }
}
