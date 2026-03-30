using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.WpfApp.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ManualObservablePropertyAttribute : Attribute
    {
    }
}
