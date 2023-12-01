using QuanLib.Core;
using QuanLib.Minecraft;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Rendering
{
    public class ColorBlockFrame<TPixel> : BlockFrame<TPixel>, IDisposable where TPixel : unmanaged, IPixel<TPixel>
    {
        public ColorBlockFrame(Image<TPixel> image, Facing facing = Facing.Zm)
        {
            ArgumentNullException.ThrowIfNull(image, nameof(image));

            _blockConverter = new(facing);
            _pixelCollection = new(image);
            if (!SR.ColorMappingCaches.ContainsKey(facing))
                _ = BuildCacheAsync();
        }

        public ColorBlockFrame(int width, int height, string pixel = "", Facing facing = Facing.Zm)
        {
            ThrowHelper.ArgumentOutOfMin(0, width, nameof(width));
            ThrowHelper.ArgumentOutOfMin(0, height, nameof(height));
            ArgumentNullException.ThrowIfNull(pixel, nameof(pixel));

            _blockConverter = new(facing);
            _pixelCollection = new(width, height, _blockConverter[pixel]);
        }

        private ColorBlockFrame(ColorPixelCollection<TPixel> pixelCollection, Facing facing)
        {
            ArgumentNullException.ThrowIfNull(pixelCollection, nameof(pixelCollection));

            _blockConverter = new(facing);
            _pixelCollection = pixelCollection;
        }

        public ColorBlockFrame(int width, int height, BlockPixel pixel) : this(width, height, pixel?.ToBlockId() ?? throw new ArgumentNullException(nameof(pixel))) { }

        public ColorBlockFrame(Size size, string pixel = "") : this(size.Width, size.Height, pixel) { }

        public ColorBlockFrame(Size size, BlockPixel pixel) : this(size.Width, size.Height, pixel) { }

        private readonly ColorBlockConverter<TPixel> _blockConverter;

        private readonly ColorPixelCollection<TPixel> _pixelCollection;

        public override IBlockConverter<TPixel> BlockConverter => _blockConverter;

        public override IPixelCollection<TPixel> Pixels => _pixelCollection;

        public Facing Facing
        {
            get => _blockConverter.Facing;
            set
            {
                _blockConverter.Facing = value;
                if (!SR.ColorMappingCaches.ContainsKey(value))
                    _ = BuildCacheAsync();
            }
        }

        public override BlockFrame Clone()
        {
            return new ColorBlockFrame<TPixel>(_pixelCollection.Clone(), Facing);
        }

        public void Dispose()
        {
            _pixelCollection.Dispose();
            GC.SuppressFinalize(this);
        }

        private async Task BuildCacheAsync()
        {
            TPixel[] pixels = await Task.Run(() => _pixelCollection.ToArray());
            await _blockConverter.BuildCacheAsync(pixels);
        }
    }
}
