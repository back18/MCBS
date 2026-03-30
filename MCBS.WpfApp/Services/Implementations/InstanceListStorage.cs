using MCBS.Common.Services;
using MCBS.WpfApp.Models;
using Microsoft.Extensions.Logging;
using QuanLib.IO.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MCBS.WpfApp.Services.Implementations
{
    public class InstanceListStorage : IInstanceListStorage
    {
        public InstanceListStorage(
            IInstanceListPathProvider pathProvider,
            IJsonConfigLoadService jsonConfigLoadService,
            IJsonConfigSaveService jsonConfigSaveService,
            ITomlConfigSaveService tomlConfigSaveService,
            ILogger<InstanceListStorage> logger)
        {
            ArgumentNullException.ThrowIfNull(pathProvider, nameof(pathProvider));
            ArgumentNullException.ThrowIfNull(jsonConfigLoadService, nameof(jsonConfigLoadService));
            ArgumentNullException.ThrowIfNull(jsonConfigSaveService, nameof(jsonConfigSaveService));
            ArgumentNullException.ThrowIfNull(tomlConfigSaveService, nameof(tomlConfigSaveService));
            ArgumentNullException.ThrowIfNull(logger, nameof(logger));

            _pathProvider = pathProvider;
            _jsonConfigLoadService = jsonConfigLoadService;
            _jsonConfigSaveService = jsonConfigSaveService;
            _tomlConfigSaveService = tomlConfigSaveService;
            _logger = logger;
        }

        private readonly IInstanceListPathProvider _pathProvider;
        private readonly IJsonConfigLoadService _jsonConfigLoadService;
        private readonly IJsonConfigSaveService _jsonConfigSaveService;
        private readonly ITomlConfigSaveService _tomlConfigSaveService;
        private readonly ILogger<InstanceListStorage> _logger;

        private readonly SemaphoreSlim _semaphore = new(1);
        private readonly ConcurrentDictionary<string, TomlConfigFileStorage> _instanceCache = [];
        private MinecraftInstanceIndex? _instanceIndex;

        public event EventHandler? InstanceIndexChanged;

        public IConfigStorage GetInstanceStorage(string instanceName)
        {
            return _instanceCache.GetOrAdd(instanceName, name =>
            {
                ConfigDataModel<MinecraftInstanceConfig.Model> dataModel = new();
                string filePath = _pathProvider.GetInstanceConfig(name).FullName;
                return new TomlConfigFileStorage(dataModel, filePath, Encoding.UTF8);
            });
        }

        public MinecraftInstanceIndex GetInstanceIndex()
        {
            _semaphore.Wait();
            try
            {
                if (_logger.IsEnabled(LogLevel.Debug))
                    _logger.LogDebug("请求查询实例索引");

                if (_pathProvider.InstanceIndex.OpenReadIfExists(out var fileStream))
                {
                    using (fileStream)
                    {
                        var model = _jsonConfigLoadService.Load<MinecraftInstanceIndex.Model>(fileStream);
                        _instanceIndex = MinecraftInstanceIndex.FromDataModel(model);
                    }
                }
                else
                {
                    _instanceIndex = MinecraftInstanceIndex.Empty;
                }
            }
            finally
            {
                _semaphore.Release();
            }

            return _instanceIndex;
        }

        public async Task<MinecraftInstanceIndex> GetInstanceIndexAsync()
        {
            await _semaphore.WaitAsync();
            try
            {
                if (_logger.IsEnabled(LogLevel.Debug))
                    _logger.LogDebug("请求查询实例索引");

                if (_pathProvider.InstanceIndex.OpenReadIfExists(out var fileStream))
                {
                    using (fileStream)
                    {
                        var model = await _jsonConfigLoadService.LoadAsync<MinecraftInstanceIndex.Model>(fileStream);
                        _instanceIndex = MinecraftInstanceIndex.FromDataModel(model);
                    }
                }
                else
                {
                    _instanceIndex = MinecraftInstanceIndex.Empty;
                }
            }
            finally
            {
                _semaphore.Release();
            }

            return _instanceIndex;
        }

        public void SetCurrentInstance(string instanceName)
        {
            var model = GetInstanceIndexModel();
            model.CurrentInstance = instanceName;

            _semaphore.Wait();
            try
            {
                using FileStream fileStream = _pathProvider.InstanceIndex.Create();
                _jsonConfigSaveService.Save(model, fileStream);
            }
            finally
            {
                _semaphore.Release();
            }

            string? oldCurrent = _instanceIndex?.CurrentInstance;
            _instanceIndex = MinecraftInstanceIndex.FromDataModel(model);

            if (_logger.IsEnabled(LogLevel.Debug) && oldCurrent != model.CurrentInstance)
                _logger.LogDebug("当前Minecraft实例已变更: \"{OldInstance}\" -> \"{NewInstance}\"", oldCurrent, model.CurrentInstance);
            InstanceIndexChanged?.Invoke(this, EventArgs.Empty);
        }

        public async Task SetCurrentInstanceAsync(string instanceName)
        {
            var model = GetInstanceIndexModel();
            model.CurrentInstance = instanceName;

            await _semaphore.WaitAsync();
            try
            {
                using FileStream fileStream = _pathProvider.InstanceIndex.Create();
                await _jsonConfigSaveService.SaveAsync(model, fileStream);
            }
            finally
            {
                _semaphore.Release();
            }

            string? oldCurrent = _instanceIndex?.CurrentInstance;
            _instanceIndex = MinecraftInstanceIndex.FromDataModel(model);

            if (_logger.IsEnabled(LogLevel.Debug) && oldCurrent != model.CurrentInstance)
                _logger.LogDebug("当前Minecraft实例已变更: \"{OldInstance}\" -> \"{NewInstance}\"", oldCurrent, model.CurrentInstance);
            InstanceIndexChanged?.Invoke(this, EventArgs.Empty);
        }

        public void AddInstance(MinecraftInstanceConfig.Model instance)
        {
            var model = GetInstanceIndexModel();
            string instanceName = instance.InstanceName;
            if (model.InstanceList.Contains(instanceName))
                throw new InvalidOperationException($"Instance with name '{instanceName}' already exists.");

            _pathProvider.InstanceList.CreateIfNotExists();
            using (FileStream configStream = _pathProvider.GetInstanceConfig(instanceName).Create())
            {
                _tomlConfigSaveService.Save(instance, configStream);
            }

            model.InstanceList.Add(instanceName);
            model.CurrentInstance = instanceName;

            _semaphore.Wait();
            try
            {
                using FileStream indexStream = _pathProvider.InstanceIndex.Create();
                _jsonConfigSaveService.Save(model, indexStream);
            }
            finally
            {
                _semaphore.Release();
            }

            _instanceIndex = MinecraftInstanceIndex.FromDataModel(model);

            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug("添加Minecraft实例: \"{InstanceName}\"", instanceName);
            InstanceIndexChanged?.Invoke(this, EventArgs.Empty);
        }

        public async Task AddInstanceAsync(MinecraftInstanceConfig.Model instance)
        {
            var model = GetInstanceIndexModel();
            string instanceName = instance.InstanceName;
            if (model.InstanceList.Contains(instanceName))
                throw new InvalidOperationException($"Instance with name '{instanceName}' already exists.");

            _pathProvider.InstanceList.CreateIfNotExists();
            using (FileStream configStream = _pathProvider.GetInstanceConfig(instanceName).Create())
            {
                await _tomlConfigSaveService.SaveAsync(instance, configStream);
            }

            model.InstanceList.Add(instanceName);
            model.CurrentInstance = instanceName;

            await _semaphore.WaitAsync();
            try
            {
                using FileStream indexStream = _pathProvider.InstanceIndex.Create();
                await _jsonConfigSaveService.SaveAsync(model, indexStream);
            }
            finally
            {
                _semaphore.Release();
            }

            _instanceIndex = MinecraftInstanceIndex.FromDataModel(model);

            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug("添加Minecraft实例: \"{InstanceName}\"", instanceName);
            InstanceIndexChanged?.Invoke(this, EventArgs.Empty);
        }

        public bool RemoveInstance(string instanceName)
        {
            var model = GetInstanceIndexModel();
            int index = model.InstanceList.IndexOf(instanceName);
            if (index < 0)
                return false;

            FileInfo configFile = _pathProvider.GetInstanceConfig(instanceName);
            try
            {
                configFile.Delete();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "无法删除实例配置文件: \"{InstanceName}\"", instanceName);
                return false;
            }

            model.InstanceList.RemoveAt(index);
            if (model.CurrentInstance == instanceName)
            {
                model.CurrentInstance =
                    model.InstanceList.Count == 0 ? string.Empty :
                    model.InstanceList[Math.Min(index, model.InstanceList.Count - 1)];
            }

            _semaphore.Wait();
            try
            {
                using FileStream indexStream = _pathProvider.InstanceIndex.Create();
                _jsonConfigSaveService.Save(model, indexStream);
            }
            finally
            {
                _semaphore.Release();
            }

            string? oldCurrent = _instanceIndex?.CurrentInstance;
            _instanceIndex = MinecraftInstanceIndex.FromDataModel(model);

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("移除Minecraft实例: \"{InstanceName}\"", instanceName);
                if (oldCurrent != model.CurrentInstance)
                    _logger.LogDebug("当前Minecraft实例已变更: \"{OldInstance}\" -> \"{NewInstance}\"", oldCurrent, model.CurrentInstance);
            }

            _instanceCache.TryRemove(instanceName, out _);
            InstanceIndexChanged?.Invoke(this, EventArgs.Empty);
            return true;
        }

        public async Task<bool> RemoveInstanceAsync(string instanceName)
        {
            var model = GetInstanceIndexModel();
            int index = model.InstanceList.IndexOf(instanceName);
            if (index < 0)
                return false;

            FileInfo configFile = _pathProvider.GetInstanceConfig(instanceName);
            try
            {
                configFile.Delete();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "无法删除实例配置文件: \"{InstanceName}\"", instanceName);
                return false;
            }

            model.InstanceList.RemoveAt(index);
            if (model.CurrentInstance == instanceName)
            {
                model.CurrentInstance =
                    model.InstanceList.Count == 0 ? string.Empty :
                    model.InstanceList[Math.Min(index, model.InstanceList.Count - 1)];
            }

            await _semaphore.WaitAsync();
            try
            {
                using FileStream indexStream = _pathProvider.InstanceIndex.Create();
                await _jsonConfigSaveService.SaveAsync(model, indexStream);
            }
            finally
            {
                _semaphore.Release();
            }

            string? oldCurrent = _instanceIndex?.CurrentInstance;
            _instanceIndex = MinecraftInstanceIndex.FromDataModel(model);

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("移除Minecraft实例: \"{InstanceName}\"", instanceName);
                if (oldCurrent != model.CurrentInstance)
                    _logger.LogDebug("当前Minecraft实例已变更: \"{OldInstance}\" -> \"{NewInstance}\"", oldCurrent, model.CurrentInstance);
            }

            _instanceCache.TryRemove(instanceName, out _);
            InstanceIndexChanged?.Invoke(this, EventArgs.Empty);
            return true;
        }

        public bool RenameInstance(string oldName, string newName)
        {
            var model = GetInstanceIndexModel();
            int index = model.InstanceList.IndexOf(oldName);
            if (index < 0 || model.InstanceList.Contains(newName))
                return false;

            FileInfo oldConfig = _pathProvider.GetInstanceConfig(oldName);
            FileInfo newConfig = _pathProvider.GetInstanceConfig(newName);

            try
            {
                if (!string.IsNullOrEmpty(newConfig.DirectoryName))
                    Directory.CreateDirectory(newConfig.DirectoryName);
                oldConfig.MoveTo(newConfig.FullName);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "无法重命名实例配置文件: \"{Old}\" -> \"{New}\"", oldName, newName);
                return false;
            }

            model.InstanceList[index] = newName;
            if (model.CurrentInstance == oldName)
                model.CurrentInstance = newName;

            _semaphore.Wait();
            try
            {
                using FileStream indexStream = _pathProvider.InstanceIndex.Create();
                _jsonConfigSaveService.Save(model, indexStream);
            }
            finally
            {
                _semaphore.Release();
            }

            string? oldCurrent = _instanceIndex?.CurrentInstance;
            _instanceIndex = MinecraftInstanceIndex.FromDataModel(model);

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("重命名Minecraft实例: \"{OldName}\" -> \"{NewName}\"", oldName, newName);
                if (oldCurrent != model.CurrentInstance)
                    _logger.LogDebug("当前Minecraft实例已变更: \"{OldInstance}\" -> \"{NewInstance}\"", oldCurrent, model.CurrentInstance);
            }

            _instanceCache.TryRemove(oldName, out _);
            InstanceIndexChanged?.Invoke(this, EventArgs.Empty);
            return true;
        }

        public async Task<bool> RenameInstanceAsync(string oldName, string newName)
        {
            var model = GetInstanceIndexModel();
            int index = model.InstanceList.IndexOf(oldName);
            if (index < 0 || model.InstanceList.Contains(newName))
                return false;

            FileInfo oldConfig = _pathProvider.GetInstanceConfig(oldName);
            FileInfo newConfig = _pathProvider.GetInstanceConfig(newName);

            try
            {
                if (!string.IsNullOrEmpty(newConfig.DirectoryName))
                    Directory.CreateDirectory(newConfig.DirectoryName);
                oldConfig.MoveTo(newConfig.FullName);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "无法重命名实例配置文件: \"{Old}\" -> \"{New}\"", oldName, newName);
                return false;
            }

            model.InstanceList[index] = newName;
            if (model.CurrentInstance == oldName)
                model.CurrentInstance = newName;

            await _semaphore.WaitAsync();
            try
            {
                using FileStream indexStream = _pathProvider.InstanceIndex.Create();
                await _jsonConfigSaveService.SaveAsync(model, indexStream);
            }
            finally
            {
                _semaphore.Release();
            }

            string? oldCurrent = _instanceIndex?.CurrentInstance;
            _instanceIndex = MinecraftInstanceIndex.FromDataModel(model);

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("重命名Minecraft实例: \"{OldName}\" -> \"{NewName}\"", oldName, newName);
                if (oldCurrent != model.CurrentInstance)
                    _logger.LogDebug("当前Minecraft实例已变更: \"{OldInstance}\" -> \"{NewInstance}\"", oldCurrent, model.CurrentInstance);
            }

            _instanceCache.TryRemove(oldName, out _);
            InstanceIndexChanged?.Invoke(this, EventArgs.Empty);
            return true;
        }

        private MinecraftInstanceIndex.Model GetInstanceIndexModel()
        {
            MinecraftInstanceIndex? instanceIndex = _instanceIndex ?? GetInstanceIndex();
            return (MinecraftInstanceIndex.Model)instanceIndex.ToDataModel();
        }
    }
}
