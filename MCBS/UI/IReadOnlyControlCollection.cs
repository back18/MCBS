﻿using MCBS.Cursor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.UI
{
    public interface IReadOnlyControlCollection<out T> : IReadOnlyList<T> where T : class, IControl
    {
        public T? FirstHover { get; }

        public T? FirstSelected { get; }

        public T? RecentlyAddedControl { get; }

        public T? RecentlyRemovedControl { get; }

        public bool HaveHover { get; }

        public bool HaveSelected { get; }

        public T[] GetHovers();

        public T[] GetSelecteds();

        public T[] ToArray();

        public T? HoverControlOf(CursorContext cursorContext);

        public void Sort();
    }
}
