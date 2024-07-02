using QuanLib.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Drawing
{
    public class LayerManager : IEnumerable<string>
    {
        public LayerManager(int width, int height, string backgroundPixel = "")
        {
            ThrowHelper.ArgumentOutOfMin(1, width, nameof(width));
            ThrowHelper.ArgumentOutOfMin(1, height, nameof(height));
            ArgumentNullException.ThrowIfNull(backgroundPixel, nameof(backgroundPixel));

            Width = width;
            Height = height;
            _layers = [];
            BackgroundPixel = backgroundPixel;
        }

        private readonly List<BlockFrame> _layers;

        public string this[int index] { get => this[index / Width, index % Width]; }

        public string this[int x, int y]
        {
            get
            {
                for (int i = _layers.Count - 1; i >= 0; i--)
                {
                    BlockFrame layer = _layers[i];
                    string pixel = layer[x, y];

                    if (layer.SupportTransparent && layer.TransparentPixel == pixel)
                        continue;

                    return pixel;
                }

                return string.Empty;
            }
        }

        public int Width { get; }

        public int Height { get; }

        public string BackgroundPixel { get; }

        public int LayerCount => _layers.Count;

        public BlockFrame GetLayer(int index)
        {
            return _layers[index];
        }

        public BlockFrame CreateTransparentLayer()
        {
            HashBlockFrame hashBlockFrame = new(Width, Height);
            _layers.Add(hashBlockFrame);
            return hashBlockFrame;
        }

        public void AddLayer(BlockFrame layer)
        {
            ArgumentNullException.ThrowIfNull(layer, nameof(layer));
            if (layer.Width != Width || layer.Height != Height)
                throw new ArgumentException("图层尺寸不一致");

            _layers.Add(layer);
        }

        public void InsertLayer(int index, BlockFrame layer)
        {
            ThrowHelper.ArgumentOutOfRange(0, _layers.Count - 1, nameof(index));
            ArgumentNullException.ThrowIfNull(layer, nameof(layer));
            if (layer.Width != Width || layer.Height != Height)
                throw new ArgumentException("图层尺寸不一致");

            _layers.Insert(index, layer);
        }

        public BlockFrame AsBlockFrame()
        {
            return new LayerBlockFrame(this);
        }

        public IEnumerator<string> GetEnumerator()
        {
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    yield return this[x, y];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
