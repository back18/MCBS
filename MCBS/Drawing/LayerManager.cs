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
            BackgroundPixel = backgroundPixel;
            _isTransparentBackground = string.IsNullOrEmpty(backgroundPixel);
            _layers = [];
        }

        private readonly bool _isTransparentBackground;

        private readonly List<BlockFrame> _layers;

        public string this[int index]
        {
            get
            {
                for (int i = _layers.Count - 1; i >= 0; i--)
                {
                    BlockFrame layer = _layers[i];
                    if (layer.IsTransparentPixel(index))
                        continue;
                    return layer[index];
                }

                return BackgroundPixel;
            }
        }

        public string this[int x, int y]
        {
            get
            {
                for (int i = _layers.Count - 1; i >= 0; i--)
                {
                    BlockFrame layer = _layers[i];
                    if (layer.IsTransparentPixel(x, y))
                        continue;
                    return layer[x, y];
                }

                return BackgroundPixel;
            }
        }

        public int Width { get; }

        public int Height { get; }

        public string BackgroundPixel { get; }

        public int LayerCount => _layers.Count;

        public SearchMode SearchMode
        {
            get
            {
                int count = _layers.Count(i => i.SearchMode == SearchMode.Coordinate);
                if (count > _layers.Count - count)
                    return SearchMode.Coordinate;
                else
                    return SearchMode.Index;
            }
        }

        public virtual bool IsTransparentPixel(int index)
        {
            if (!_isTransparentBackground)
                return false;

            for (int i = 0; i < _layers.Count; i++)
            {
                if (!_layers[i].IsTransparentPixel(index))
                    return false;
            }

            return true;
        }

        public virtual bool IsTransparentPixel(int x, int y)
        {
            if (!_isTransparentBackground)
                return false;

            for (int i = 0; i < _layers.Count; i++)
            {
                if (!_layers[i].IsTransparentPixel(x, y))
                    return false;
            }

            return true;
        }

        public bool CheckTransparentPixel()
        {
            if (!_isTransparentBackground)
                return false;

            return _layers.Any(i => i.CheckTransparentPixel());
        }

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

        public bool RemoveLayer(int index)
        {
            if (index < 0 || index >= _layers.Count)
                return false;

            _layers.RemoveAt(index);
            return true;
        }

        public bool RemoveLayer(BlockFrame layer)
        {
            ArgumentNullException.ThrowIfNull(layer, nameof(layer));

            return _layers.Remove(layer);
        }

        public void ClearLayers()
        {
            _layers.Clear();
        }

        public LayerManager Clone()
        {
            LayerManager layerManager = new(Width, Height, BackgroundPixel);
            layerManager._layers.AddRange(_layers);
            return layerManager;
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
