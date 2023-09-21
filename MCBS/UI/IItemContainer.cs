﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.UI
{
    public interface IItemContainer<T> where T : notnull
    {
        public ItemCollection<T> Items { get; }
    }
}