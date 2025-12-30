using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.WpfApp.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class RouteAttribute : Attribute
    {
        public Type? Parent { get; set; }

        public string? Uri { get; set; }
    }
}
