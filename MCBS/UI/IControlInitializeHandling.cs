﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.UI
{
    public interface IControlInitializeHandling
    {
        public bool IsInitializeCompleted { get; }

        public void HandleInitialize();

        public void HandleInitCompleted1();

        public void HandleInitCompleted2();

        public void HandleInitCompleted3();

        public void HandleAllInitialize()
        {
            HandleInitialize();
            HandleInitCompleted1();
            HandleInitCompleted2();
            HandleInitCompleted3();
        }
    }
}