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
        public CursorEventArgs(Point position, CursorContext context, CursorInputData oldData, CursorInputData newData)
        {
            Position = position;
            CursorContext = context;
            OldData = oldData;
            NewData = newData;
        }

        public Point Position { get; }

        public CursorContext CursorContext { get; }

        public CursorInputData OldData { get; }

        public CursorInputData NewData { get; }

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

        public CursorEventArgs Clone(Func<Point, Point> convert)
        {
            return new(convert.Invoke(Position), CursorContext, OldData, NewData);
        }
    }
}
