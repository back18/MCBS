using MCBS.Frame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Event
{
    public class ArrayFrameEventArgs : EventArgs
    {
        public ArrayFrameEventArgs(ArrayFrame arrayFrame)
        {
            ArrayFrame = arrayFrame;
        }

        public ArrayFrame ArrayFrame { get; }
    }
}
