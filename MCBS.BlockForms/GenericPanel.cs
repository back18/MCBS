﻿using MCBS.UI;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.BlockForms
{
    public class GenericPanel<T> : GenericContainerControl<T> where T : class, IControl
    {

    }
}
