using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using DynamicPropertyAccessor;
using MCBS.WpfApp.Config;
using MCBS.WpfApp.Messages;
using MCBS.WpfApp.Services;
using Microsoft.Extensions.Logging;
using QuanLib.Core;
using QuanLib.Core.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using System.Windows;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace MCBS.WpfApp.ViewModels
{
    public abstract partial class ConfigServiceViewModel : ObservableValidator, IRecipient<PageNavigatingFromMessage>, IRecipient<MainWindowClosingMessage>
    {
        private static readonly Type COLLECTION_TYPE = typeof(ObservableCollection<string>);

        protected ConfigServiceViewModel(ILoggerFactory loggerFactory, IMessageBoxService messageBoxService)
        {
            ArgumentNullException.ThrowIfNull(loggerFactory, nameof(loggerFactory));
            ArgumentNullException.ThrowIfNull(messageBoxService, nameof(messageBoxService));

            _logger = loggerFactory.CreateLogger(GetType());
            _messageBoxService = messageBoxService;
            _lazyProperties = new Lazy<ReadOnlyDictionary<string, PropertyInfo>>(
                () => ReflectionHelper.GetObservableProperties(GetType()).AsReadOnly(),
                LazyThreadSafetyMode.ExecutionAndPublication
            );

            TempStorage = [];
        }

        protected readonly ILogger _logger;

        protected readonly IMessageBoxService _messageBoxService;

        private readonly Lazy<ReadOnlyDictionary<string, PropertyInfo>> _lazyProperties;

        protected ReadOnlyDictionary<string, PropertyInfo> Properties => _lazyProperties.Value;

        protected Dictionary<string, object?> TempStorage { get; }

        protected abstract IConfigService? ConfigService { get; set; }

        public abstract bool IsLoaded { get; protected set; }

        public abstract event EventHandler<EventArgs<object>>? Loaded;

        public virtual void Receive(PageNavigatingFromMessage message)
        {
            HandleErrors(message.EventArgs);
        }

        public virtual void Receive(MainWindowClosingMessage message)
        {
            HandleErrors(message.EventArgs);
            if (!message.EventArgs.Cancel)
                HandleSave();
        }

        protected virtual void HandleErrors(CancelEventArgs e)
        {
            if (TempStorage.Count == 0 || !HasErrors)
                return;

            var errors = GetErrors();
            string errorMessage = string.Join(Environment.NewLine, errors.Select(s => s.ErrorMessage));
            var result = _messageBoxService.Show(
                $"以下属性未正确设置：{Environment.NewLine}{errorMessage}",
                "是否离开当前页面",
                MessageBoxButton.OKCancel,
                MessageBoxImage.Warning);

            switch (result)
            {
                case MessageBoxResult.OK:
                    DumpTempStorage();
                    break;
                case MessageBoxResult.Cancel:
                    e.Cancel = true;
                    break;
            }
        }

        protected virtual void HandleSave()
        {
            if (TempStorage.Count > 0)
                DumpTempStorage();

            if (ConfigService?.IsModified == true)
            {
                ConfigService.GetConfigStorage().SaveConfig();
                LogSaved();
            }
        }

        protected virtual async Task HandleSaveAsync()
        {
            if (TempStorage.Count > 0)
                DumpTempStorage();

            if (ConfigService?.IsModified == true)
            {
                await ConfigService.GetConfigStorage().SaveConfigAsync();
                LogSaved();
            }
        }

        protected virtual void ObservablePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (ConfigService is null || !IsLoaded)
                return;

            string? propertyName = e.PropertyName;
            if (!string.IsNullOrEmpty(propertyName) &&
                Properties.TryGetValue(propertyName, out var propertyInfo) &&
                !COLLECTION_TYPE.Equals(propertyInfo.PropertyType))
            {
                object? value = this.GetProperty(propertyName);
                ValidateAllProperties();
                HandlePropertyChanged(propertyName, value);
            }
        }

        protected virtual void HandleCollectionChanged(string propertyName, ObservableCollection<string> collection)
        {
            if (ConfigService is null || !IsLoaded)
                return;

            ArgumentException.ThrowIfNullOrEmpty(propertyName, nameof(propertyName));
            ArgumentNullException.ThrowIfNull(collection, nameof(collection));

            string[] value = collection.ToArray();
            ValidateProperty(collection, propertyName);
            HandlePropertyChanged(propertyName, value);

        }

        private void HandlePropertyChanged(string propertyName, object? value)
        {
            TempStorage[propertyName] = value;
            if (!HasErrors)
                DumpTempStorage();
        }

        private void DumpTempStorage()
        {
            if (ConfigService is null || !IsLoaded)
                return;

            foreach (var item in TempStorage)
            {
                ConfigService.SetPropertyValue(item.Key, item.Value);
                if (_logger.IsEnabled(LogLevel.Debug))
                    _logger.LogDebug("配置属性 {PropertyName} 已更新为 {PropertyValue}", item.Key, ObjectFormatter.Format(item.Value));
            }
            TempStorage.Clear();
        }

        [LoggerMessage(Level = LogLevel.Information, Message = "配置已保存")]
        private partial void LogSaved();
    }
}
