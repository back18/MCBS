using MCBS.Config;
using QuanLib.DataAnnotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text;

namespace MCBS.WpfApp.Config.Items
{
    public class ConfigItem
    {
        private static readonly Type STRING_TYPE = typeof(string);
        private static readonly Type IENUMERABLE_TYPE = typeof(IEnumerable);
        private static readonly Type IENUMERABLE_GENERIC_TYPE = typeof(IEnumerable<>);

        public required string Identifier { get; init; }

        public required string Name { get; init; }

        public required string Description { get; init; }

        public required Type Type { get; init; }

        public object? Value { get; init; }

        public string? GroupName { get; init; }

        public int Order { get; init; }

        public static ConfigItem[] LoadAllConfigItem(object model)
        {
            ArgumentNullException.ThrowIfNull(model, nameof(model));

            PropertyInfo[] properties = model.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            List<ConfigItem> result = [];

            foreach (PropertyInfo property in properties)
            {
                if (!property.CanRead || !property.CanWrite || property.GetIndexParameters().Length > 0)
                    continue;

                var attributes = property.GetCustomAttributes();
                DisplayAttribute? display = attributes.OfType<DisplayAttribute>().FirstOrDefault();
                ConfigGroupAttribute? configGroup = attributes.OfType<ConfigGroupAttribute>().FirstOrDefault();
                RangeAttribute? range = attributes.OfType<RangeAttribute>().FirstOrDefault();
                ReadOnlyCollection<object?> allowedValues =
                    attributes.OfType<AllowedValuesAttribute>().FirstOrDefault()?.Values.AsReadOnly() ??
                    attributes.OfType<NewAllowedValuesAttribute>().FirstOrDefault()?.Values ??
                    ReadOnlyCollection<object?>.Empty;

                string identifier = property.Name;
                string name = display?.GetName() ?? identifier;
                string description = display?.GetDescription() ?? string.Empty;
                Type type = property.PropertyType;
                object? value = property.GetValue(model);
                string? groupName = display?.GetGroupName();
                int order = display?.GetOrder() ?? 0;

                ConfigItemBuilder builder = new();
                builder
                    .SetIdentifier(identifier)
                    .SetName(name)
                    .SetDescription(description)
                    .SetType(type)
                    .SetValue(value)
                    .SetGroupName(groupName)
                    .SetOrder(order);

                if (configGroup is not null)
                {
                    if (value is null)
                        builder.SetConfigItems(Array.Empty<ConfigItem>());
                    else
                        builder.SetConfigItems(LoadAllConfigItem(value));
                }
                else if (!STRING_TYPE.Equals(type) && IENUMERABLE_TYPE.IsAssignableFrom(type))
                {
                    List<object?> items = [];
                    if (value is IEnumerable enumerable)
                    {
                        foreach (var item in enumerable)
                            items.Add(item);
                    }

                    Type? genericEnumerableType = type
                        .GetInterfaces()
                        .FirstOrDefault(s => s.IsGenericType &&
                        IENUMERABLE_GENERIC_TYPE.Equals(s.GetGenericTypeDefinition()));

                    builder.SetItems(items);
                    builder.SetItemType(genericEnumerableType?.GetGenericArguments()[0] ?? typeof(object));
                }
                else if (allowedValues.Count > 0)
                {
                    builder.SetAllowedValues(allowedValues);
                }
                else if (range is not null)
                {
                    builder.SetMinimum(range.Minimum);
                    builder.SetMaximum(range.Maximum);
                }

                ConfigItem configItem = builder.Build();
                result.Add(configItem);
            }

            return result.OrderBy(s => s.Order).ToArray();
        }
    }
}
