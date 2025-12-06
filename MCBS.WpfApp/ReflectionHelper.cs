using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace MCBS.WpfApp
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

                if (property.GetCustomAttribute<ObservablePropertyAttribute>() is not null)
                    items.Add(property.Name, property);
            }

            return items;
        }
    }
}
