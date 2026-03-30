using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.WpfApp.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ExcludeFromDIAttribute : Attribute
    {
    }
}
