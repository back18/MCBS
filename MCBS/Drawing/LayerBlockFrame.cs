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
        }

        private readonly LayerManager _layerManager;

        public override string this[int index] { get => _layerManager[index]; set => throw new NotSupportedException(); }

        public override string this[int x, int y] { get => _layerManager[x, y]; set => throw new NotSupportedException(); }

        public override int Count => Width * Height;

        public override int Width => _layerManager.Width;

        public override int Height => _layerManager.Height;

        public override SearchMode SearchMode => SearchMode.Coordinate;

        public override BlockFrame Clone()
        {
            return new LayerBlockFrame(_layerManager);
        }

        public override void Fill(string pixel)
        {
            throw new NotSupportedException();
        }

        public override IDictionary<Point, string> GetAllPixel()
        {
            PositionEnumerable positions = new(Width, Height);

            Dictionary<Point, string> result = new();
            Foreach.Start(positions, this, (position, pixel) => result.Add(position, pixel));

            return result;
        }

        public override IEnumerator<string> GetEnumerator()
        {
            return _layerManager.GetEnumerator();
        }

        public override string[] ToArray()
        {
            return _layerManager.ToArray();
        }
    }
}
