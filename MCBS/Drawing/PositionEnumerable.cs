using QuanLib.Core;
using SixLabors.ImageSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Drawing
{
    public class PositionEnumerable : IEnumerable<Point>
    {
        public PositionEnumerable(int width, int height)
        {
            ThrowHelper.ArgumentOutOfMin(1, width, nameof(width));
            ThrowHelper.ArgumentOutOfMin(1, height, nameof(height));

            Width = width;
            Height = height;
        }

        public int Width { get; }

        public int Height { get; }

        public IEnumerator<Point> GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }

        private class Enumerator : IEnumerator<Point>
        {
            public Enumerator(PositionEnumerable owner)
            {
                ArgumentNullException.ThrowIfNull(owner, nameof(owner));

                _owner = owner;
                _position = new(-1, 0);
            }

            private readonly PositionEnumerable _owner;

            private Point _position;

            public Point Current => _position;

            object IEnumerator.Current => Current;

            public bool MoveNext()
            {
                _position.X++;
                if (_position.X >= _owner.Width)
                {
                    _position.X = 0;
                    _position.Y++;
                }

                return _position.Y < _owner.Height && _position.X < _owner.Width;
            }

            public void Reset()
            {
                _position = new(-1, 0);
            }

            void IDisposable.Dispose()
            {

            }
        }
    }
}
