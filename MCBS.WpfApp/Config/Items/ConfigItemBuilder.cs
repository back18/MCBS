using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace MCBS.WpfApp.Config.Items
{
    public class ConfigItemBuilder
    {
        private string? _identifier;
        private string? _name;
        private string? _description;
        private Type? _type;
        private object? _value;
        private string? _groupName;
        private int _order = 0;
        private ConfigItem[]? _configItems;
        private List<object?>? _items;
        public Type? _itemType;
        private ReadOnlyCollection<object?>? _allowedValues;
        private object? _minimum;
        private object? _maximum;

        public ConfigItemBuilder SetIdentifier(string identifier)
        {
            _identifier = identifier;
            return this;
        }

        public ConfigItemBuilder SetName(string name)
        {
            _name = name;
            return this;
        }

        public ConfigItemBuilder SetDescription(string description)
        {
            _description = description;
            return this;
        }

        public ConfigItemBuilder SetType(Type type)
        {
            _type = type;
            return this;
        }

        public ConfigItemBuilder SetValue(object? value)
        {
            _value = value;
            return this;
        }

        public ConfigItemBuilder SetGroupName(string? groupName)
        {
            _groupName = groupName;
            return this;
        }

        public ConfigItemBuilder SetOrder(int order)
        {
            _order = order;
            return this;
        }

        public ConfigItemBuilder SetConfigItems(IEnumerable<ConfigItem> configItems)
        {
            if (configItems is ConfigItem[] array)
                _configItems = array;
            else
                _configItems = configItems.ToArray();
            return this;
        }

        public ConfigItemBuilder SetItems(IEnumerable<object?> items)
        {
            if (items is List<object?> collection)
                _items = collection;
            else
                _items = items.ToList();
            return this;
        }

        public ConfigItemBuilder SetItemType(Type itemType)
        {
            _itemType = itemType;
            return this;
        }

        public ConfigItemBuilder SetAllowedValues(IList<object?> allowedValues)
        {
            if (allowedValues is ReadOnlyCollection<object?> collection)
                _allowedValues = collection;
            else
                _allowedValues = allowedValues.AsReadOnly();
            return this;
        }

        public ConfigItemBuilder SetMinimum(object minimum)
        {
            _minimum = minimum;
            return this;
        }

        public ConfigItemBuilder SetMaximum(object maximum)
        {
            _maximum = maximum;
            return this;
        }

        public ConfigItem Build()
        {
            if (_identifier is null || _type is null)
                throw new InvalidOperationException("Missing identifier or type information");

            _name ??= _identifier;
            _description ??= string.Empty;

            if (_configItems is not null)
            {
                return new GroupConfigItem()
                {
                    Identifier = _identifier,
                    Name = _name,
                    Description = _description,
                    Type = _type,
                    Value = _value,
                    GroupName = _groupName,
                    Order = _order,
                    ConfigItems = _configItems
                };
            }
            else if (_items is not null)
            {
                _itemType ??= typeof(object);
                return new ListConfigItem()
                {
                    Identifier = _identifier,
                    Name = _name,
                    Description = _description,
                    Type = _type,
                    Value = _value,
                    GroupName = _groupName,
                    Order = _order,
                    Items = _items,
                    ItemType = _itemType,
                };
            }
            else if (_allowedValues is not null)
            {
                return new SelectorConfigItem()
                {
                    Identifier = _identifier,
                    Name = _name,
                    Description = _description,
                    Type = _type,
                    Value = _value,
                    GroupName = _groupName,
                    Order = _order,
                    AllowedValues = _allowedValues
                };
            }
            if (_minimum is not null && _maximum is not null)
            {
                return new RangeConfigItem()
                {
                    Identifier = _identifier,
                    Name = _name,
                    Description = _description,
                    Type = _type,
                    Value = _value,
                    GroupName = _groupName,
                    Order = _order,
                    Minimum = _minimum,
                    Maximum = _maximum
                };
            }
            else
            {
                return new ConfigItem()
                {
                    Identifier = _identifier,
                    Name = _name,
                    Description = _description,
                    Type = _type,
                    Value = _value,
                    GroupName = _groupName,
                    Order = _order
                };
            }
        }
    }
}
