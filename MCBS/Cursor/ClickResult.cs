using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Cursor
{
    public readonly struct ClickResult
    {
        public ClickResult(bool isLeftClick, bool isRightClick)
        {
            IsLeftClick = isLeftClick;
            IsRightClick = isRightClick;
        }

        public readonly bool IsLeftClick;

        public readonly bool IsRightClick;
    }
}
