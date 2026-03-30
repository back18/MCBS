using CommunityToolkit.Mvvm.ComponentModel;
using MCBS.WpfApp.Attributes;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace MCBS.WpfApp.Helpers
{
    public static class ReflectionHelper
    {
        public static Dictionary<string, PropertyInfo> GetObservableProperties(Type type)
        {
            ArgumentNullException.ThrowIfNull(type, nameof(type));

            Dictionary<string, PropertyInfo> items = [];
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo property in properties)
            {
                if (!property.CanRead || !property.CanWrite || property.GetIndexParameters().Length > 0)
                    continue;

                if (property.IsDefined(typeof(ObservablePropertyAttribute), false) ||
                    property.IsDefined(typeof(ManualObservablePropertyAttribute), false))
                    items.Add(property.Name, property);
            }

            return items;
        }
    }
}
