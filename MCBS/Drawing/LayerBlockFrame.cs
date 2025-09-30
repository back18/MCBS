using QuanLib.Core;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Drawing
{
    public class LayerBlockFrame : BlockFrame
    {
        public LayerBlockFrame(LayerManager layerManager)
        {
            ArgumentNullException.ThrowIfNull(layerManager, nameof(layerManager));

            _layerManager = layerManager;
            _frameCache = new string[layerManager.Width, layerManager.Height];
            _cachedState = new bool[layerManager.Width, layerManager.Height];

            ContainsTransparent = string.IsNullOrEmpty(layerManager.BackgroundPixel);
        }

        private LayerBlockFrame(LayerManager layerManager, string?[,] frameCache, bool[,] cachedState)
        {
            ArgumentNullException.ThrowIfNull(layerManager, nameof(layerManager));
            ArgumentNullException.ThrowIfNull(frameCache, nameof(frameCache));
            ArgumentNullException.ThrowIfNull(cachedState, nameof(cachedState));

            _layerManager = layerManager;
            _frameCache = frameCache;
            _cachedState = cachedState;

            ContainsTransparent = string.IsNullOrEmpty(layerManager.BackgroundPixel);
        }

        protected readonly LayerManager _layerManager;

        private readonly string?[,] _frameCache;

        private readonly bool[,] _cachedState;

        public override string this[int index]
        {
            get
            {
                string result = _layerManager[index];
                int x = index % _layerManager.Width;
                int y = index / _layerManager.Height;
                _frameCache[x, y] = result;
                _cachedState[x, y] = true;
                return result;
            }
            set => throw new NotSupportedException();
        }

        public override string this[int x, int y]
        {
            get
            {
                if (_cachedState[x, y])
                    return _frameCache[x, y]!;

                string result = _layerManager[x, y];
                _frameCache[x, y] = result;
                _cachedState[x, y] = true;
                return result;
            }
            set => throw new NotSupportedException();
        }

        public override int Count => Width * Height;

        public override int Width => _layerManager.Width;

        public override int Height => _layerManager.Height;

        public override SearchMode SearchMode => _layerManager.SearchMode;

        public override bool IsTransparentPixel(int index)
        {
            return _layerManager.IsTransparentPixel(index);
        }

        public override bool IsTransparentPixel(int x, int y)
        {
            return _layerManager.IsTransparentPixel(x, y);
        }

        public override bool CheckTransparentPixel()
        {
            return _layerManager.CheckTransparentPixel();
        }

        public override BlockFrame Crop(Rectangle rectangle)
        {
            if (_layerManager.LayerCount == 0)
                return new HashBlockFrame(Width, Height, _layerManager.BackgroundPixel);
            else if (_layerManager.LayerCount == 1)
                return _layerManager.GetLayer(0).Crop(rectangle);
            else
                return base.Crop(rectangle);
        }

        public override BlockFrame Clone()
        {
            return new LayerBlockFrame(_layerManager.Clone(), _frameCache, _cachedState);
        }

        public override void Fill(string pixel)
        {
            throw new NotSupportedException();
        }

        public override OverwriteContext Overwrite(BlockFrame blockFrame, Size size, Point location, Point offset)
        {
            ArgumentNullException.ThrowIfNull(blockFrame, nameof(blockFrame));

            if (size.Width < blockFrame.Width || size.Height < blockFrame.Height)
                blockFrame = blockFrame.Crop(new(offset, size));

            BlockFrame layer;
            int width = Width;
            int height = Height;

            if (blockFrame.Width == width && blockFrame.Height == height)
                layer = blockFrame;
            else
                layer = new NestingBlockFrame(
                    Width,
                    Height,
                    blockFrame,
                    location,
                    offset,
                    0,
                    string.Empty,
                    string.Empty);

            _layerManager.AddLayer(layer);

            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    _cachedState[x, y] = false;
                    _frameCache[x, y] = null;
                }

            return new(new(Width, Height), new(Width, Height), new(size.Width, size.Height), location, offset);
        }

        public static (Point location, BlockFrame differenceFrame1, BlockFrame differenceFrame2) GetDifferenceLayer(LayerBlockFrame blockFrame1, LayerBlockFrame blockFrame2)
        {
            ArgumentNullException.ThrowIfNull(blockFrame1, nameof(blockFrame1));
            ArgumentNullException.ThrowIfNull(blockFrame2, nameof(blockFrame2));
            if (blockFrame1.Width != blockFrame1.Width || blockFrame1.Height != blockFrame2.Height)
                throw new ArgumentException("帧尺寸不一致");

            Point location = Point.Empty;
            BlockFrame differenceFrame1;
            BlockFrame differenceFrame2;

            while (true)
            {
                List<(int layer, BlockFrame? layer1, BlockFrame? layer2)> differenceLayers = GetDifferenceLayers(blockFrame1, blockFrame2);
                if (differenceLayers.Count != 1)
                    break;

                var (layer, layer1, layer2) = differenceLayers[0];
                if (layer != blockFrame2._layerManager.LayerCount - 1 ||
                    layer1 is null ||
                    layer2 is null)
                    break;

                if (layer1 is LayerBlockFrame layerBlockFrame1 &&
                    layer2 is LayerBlockFrame layerBlockFrame2 &&
                    !layer2.ContainsTransparent)
                {
                    blockFrame1 = layerBlockFrame1;
                    blockFrame2 = layerBlockFrame2;
                }
                else if (layer1 is NestingBlockFrame nestingBlockFrame1 &&
                        layer2 is NestingBlockFrame nestingBlockFrame2 &&
                        nestingBlockFrame2.Location.X >= 0 &&
                        nestingBlockFrame2.Location.Y >= 0 &&
                        nestingBlockFrame2.Location.X + nestingBlockFrame2.InnerBlockFrame.Width <= nestingBlockFrame2.Width &&
                        nestingBlockFrame2.Location.Y + nestingBlockFrame2.InnerBlockFrame.Height <= nestingBlockFrame2.Height &&
                        nestingBlockFrame2.Offset == Point.Empty)
                {
                    if (nestingBlockFrame1.InnerBlockFrame is LayerBlockFrame nestingLayerBlockFrame1 &&
                        nestingBlockFrame2.InnerBlockFrame is LayerBlockFrame nestingLayerBlockFrame2 &&
                        nestingBlockFrame1.Location == nestingBlockFrame2.Location &&
                        nestingLayerBlockFrame1.Width == nestingLayerBlockFrame2.Width &&
                        nestingLayerBlockFrame1.Height == nestingLayerBlockFrame2.Height &&
                        !nestingLayerBlockFrame2.ContainsTransparent)
                    {
                        blockFrame1 = nestingLayerBlockFrame1;
                        blockFrame2 = nestingLayerBlockFrame2;
                        location = Point.Add(location, new(nestingBlockFrame2.Location.X, nestingBlockFrame2.Location.Y));
                    }
                    else if (!nestingBlockFrame2.ContainsTransparent)
                    {
                        return (location, nestingBlockFrame1, nestingBlockFrame2);
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }

            differenceFrame1 = blockFrame1;
            differenceFrame2 = blockFrame2;
            return (location, differenceFrame1, differenceFrame2);
        }

        public static List<(int layer, BlockFrame? layer1, BlockFrame? layer2)> GetDifferenceLayers(LayerBlockFrame blockFrame1, LayerBlockFrame blockFrame2)
        {
            ArgumentNullException.ThrowIfNull(blockFrame1, nameof(blockFrame1));
            ArgumentNullException.ThrowIfNull(blockFrame2, nameof(blockFrame2));
            if (blockFrame1.Width != blockFrame1.Width || blockFrame1.Height != blockFrame2.Height)
                throw new ArgumentException("帧尺寸不一致");

            List<(int layer, BlockFrame? layer1, BlockFrame? layer2)> result = [];
            int count = Math.Min(blockFrame1._layerManager.LayerCount, blockFrame2._layerManager.LayerCount);

            for (int i = 0; i < count; i++)
            {
                BlockFrame layer1 = blockFrame1._layerManager.GetLayer(i);
                BlockFrame layer2 = blockFrame2._layerManager.GetLayer(i);

                if (layer1 == layer2)
                    continue;

                if (layer1 is NestingBlockFrame nestingBlockFrame1 &&
                    layer2 is NestingBlockFrame nestingBlockFrame2 &&
                    nestingBlockFrame1.Location == nestingBlockFrame2.Location &&
                    nestingBlockFrame1.InnerBlockFrame == nestingBlockFrame2.InnerBlockFrame)
                    continue;

                result.Add((i, layer1, layer2));
            }

            if (blockFrame1._layerManager.LayerCount > count)
            {
                for (int i = count; i < blockFrame1._layerManager.LayerCount; i++)
                {
                    BlockFrame layer = blockFrame1._layerManager.GetLayer(i);
                    result.Add((i, layer, null));
                }
            }
            else if (blockFrame2._layerManager.LayerCount > count)
            {
                for (int i = count; i < blockFrame2._layerManager.LayerCount; i++)
                {
                    BlockFrame layer = blockFrame2._layerManager.GetLayer(i);
                    result.Add((i, null, layer));
                }
            }

            return result;
        }

        public LayerManager GetLayerManagerCopy()
        {
            return _layerManager.Clone();
        }

        public override ScreenPixel<string>[] GetAllPixel()
        {
            PositionEnumerable positions = new(Width, Height);
            ScreenPixel<string>[] result = new ScreenPixel<string>[Count];
            int index = 0;

            Foreach.Start(positions, this, (position, pixel) => result[index++] = new(position, pixel));

            return result;
        }

        public override string[] ToArray()
        {
            return _layerManager.ToArray();
        }

        public override IEnumerator<string> GetEnumerator()
        {
            return _layerManager.GetEnumerator();
        }
    }
}
