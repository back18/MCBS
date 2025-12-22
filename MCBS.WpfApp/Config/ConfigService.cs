using DynamicPropertyAccessor;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace MCBS.WpfApp.Config
{
    public class ConfigService : IConfigService
    {
        public ConfigService(object config, IConfigModel configModel, IConfigStorage configStorage)
        {
            ArgumentNullException.ThrowIfNull(config, nameof(config));
            ArgumentNullException.ThrowIfNull(configModel, nameof(configModel));
            ArgumentNullException.ThrowIfNull(configStorage, nameof(configStorage));

            _config = config;
            _configModel = configModel;
            _configStorage = configStorage;
            _configStorage.Saved += (sender, e) => IsModified = false;

            _lazyProperties = new Lazy<ReadOnlyDictionary<string, PropertyInfo>>(
                () => GetProperties(_configModel.Type).AsReadOnly(),
                LazyThreadSafetyMode.ExecutionAndPublication
            );
        }

        private object? _default;

        private readonly object _config;

        private readonly IConfigModel _configModel;

        private readonly IConfigStorage _configStorage;

        private readonly Lazy<ReadOnlyDictionary<string, PropertyInfo>> _lazyProperties;

        private ReadOnlyDictionary<string, PropertyInfo> Properties => _lazyProperties.Value;

        public bool IsModified { get; private set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public IConfigStorage GetConfigStorage()
        {
            return _configStorage;
        }

        public object GetCurrentConfig()
        {
            return _config;
        }

        public object GetDefaultConfig()
        {
            _default ??= _configModel.CreateDefault();
            return _default;
        }

        public void NotifyModified(string? propertyName)
        {
            IsModified = true;
            PropertyChanged?.Invoke(this, new(propertyName));
        }

        public object? GetPropertyValue(string propertyName)
        {
            ArgumentException.ThrowIfNullOrEmpty(propertyName, nameof(propertyName));

            if (Properties.ContainsKey(propertyName))
                return _config.GetProperty(propertyName);
            else
                return null;
        }

        public void SetPropertyValue(string propertyName, object? value)
        {
            ArgumentException.ThrowIfNullOrEmpty(propertyName, nameof(propertyName));

            if (Properties.ContainsKey(propertyName))
            {
                _config.SetProperty(propertyName, value);
                IsModified = true;
                PropertyChanged?.Invoke(this, new(propertyName));
            }
        }

        private static Dictionary<string, PropertyInfo> GetProperties(Type type)
        {
            ArgumentNullException.ThrowIfNull(type, nameof(type));

            Dictionary<string, PropertyInfo> items = [];
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo property in properties)
            {
                if (!property.CanRead || !property.CanWrite || property.GetIndexParameters().Length > 0)
                    continue;

                items.Add(property.Name, property);
            }

            return items;
        }
    }
}
