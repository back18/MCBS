using MCBS.Cursor;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Events
{
    public class CursorEventArgs : EventArgs
    {
        public CursorEventArgs(Point position, CursorContext context)
        {
            Position = position;
            CursorContext = context;
        }

        public Point Position { get; }

        public CursorContext CursorContext { get; }

        public CursorInputData OldData => CursorContext.OldInputData;

        public CursorInputData NewData => CursorContext.NewInputData;

        public int InventorySlotDelta
        {
            get
            {
                int delta = NewData.InventorySlot - OldData.InventorySlot;
                if (delta >= 6)
                    delta -= 9;
                else if (delta <= -6)
                    delta += 9;
                return delta;
            }
        }

        public Point CursorPositionOffset => new(NewData.CursorPosition.X - OldData.CursorPosition.X, NewData.CursorPosition.Y - OldData.CursorPosition.Y);

        public Point CursorOriginalPosition => new(Position.X - CursorPositionOffset.X, Position.Y - CursorPositionOffset.Y);

        public CursorEventArgs Clone(Func<Point, Point> convert)
        {
            return new(convert.Invoke(Position), CursorContext);
        }
    }
}
