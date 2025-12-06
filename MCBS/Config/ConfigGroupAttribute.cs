using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.Config
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class ConfigGroupAttribute : Attribute
    {

    }
}
