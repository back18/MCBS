﻿using MCBS.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Application
{
    public abstract class ServicesApplicationBase : ApplicationBase
    {
        public abstract IRootForm RootForm { get; }
    }
}