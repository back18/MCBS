using QuanLib.Core;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Rendering
{
    public abstract class BlockFrame<TPixel> : BlockFrame
    {
        public override string this[int index] { get => BlockConverter[Pixels[index]]; set => Pixels[index] = BlockConverter[value]; }

        public override string this[int x, int y] { get => BlockConverter[Pixels[x, y]]; set => Pixels[x, y] = BlockConverter[value]; }

        public override int Count => Pixels.Count;

        public override int Width => Pixels.Width;

        public override int Height => Pixels.Height;

        public override SearchMode SearchMode => Pixels.SearchMode;

        public override bool SupportTransparent => Pixels.SupportTransparent;

        public override string TransparentPixel => BlockConverter[Pixels.TransparentPixel];

        public abstract IBlockConverter<TPixel> BlockConverter { get; }

        public abstract IPixelCollection<TPixel> Pixels { get; }

        public override OverwriteContext Overwrite(BlockFrame blockFrame, Size size, Point location, Point offset)
        {
            if (blockFrame is  BlockFrame<TPixel> pixelBlockFrame)
                return Overwrite(pixelBlockFrame, size, location, offset);

            return base.Overwrite(blockFrame, size, location, offset);
        }

        public virtual OverwriteContext Overwrite(BlockFrame<TPixel> blockFrame, Size size, Point location, Point offset)
        {
            ArgumentNullException.ThrowIfNull(blockFrame, nameof(blockFrame));

            return Pixels.Overwrite(blockFrame.Pixels, size, location, offset);
        }

        public override void Fill(string pixel)
        {
            Pixels.Fill(BlockConverter[pixel]);
        }

        public override IDictionary<Point, string> GetAllPixel()
        {
            PositionEnumerable positions = new(Width, Height);

            Dictionary<Point, string> result = new();
            Foreach.Start(positions, Pixels, (position, pixel) => result.Add(position, BlockConverter[pixel]));

            return result;
        }

        public override string[] ToArray()
        {
            int index = 0;
            string[] array = new string[Pixels.Count];
            foreach (var pixel in Pixels)
            {
                array[index++] = BlockConverter[pixel];
            }

            return array;
        }

        public override IEnumerator<string> GetEnumerator()
        {
            foreach (var pixel in Pixels)
            {
                yield return BlockConverter[pixel];
            }
        }
    }
}
