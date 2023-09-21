﻿using MCBS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Event
{
    public class FormContextEventArgs : EventArgs
    {
        public FormContextEventArgs(FormContext formContext)
        {
            FormContext = formContext;
        }

        public FormContext FormContext { get; }
    }
}