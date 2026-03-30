using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MCBS.WpfApp.Commands;
using MCBS.WpfApp.Messages;
using MCBS.WpfApp.Models;
using MCBS.WpfApp.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows;

namespace MCBS.WpfApp.ViewModels.Home
{
    public partial class LaunchViewModel :
        ObservableObject,
        IRecipient<MinecraftInstanceChangedMessage>,
        IRecipient<MinecraftInstanceModifiedMessage>,
        IRecipient<MainWindowClosingMessage>,
        IRecipient<PageNavigatingFromMessage>,
        IRecipient<PageNavigatedToMessage>
    {
        public LaunchViewModel(PageNavigateCommand pageNavigateCommand, IInstanceListStorage instanceListStorage, ILogger<LaunchViewModel> logger)
        {
            ArgumentNullException.ThrowIfNull(pageNavigateCommand, nameof(pageNavigateCommand));
            ArgumentNullException.ThrowIfNull(instanceListStorage, nameof(instanceListStorage));
            ArgumentNullException.ThrowIfNull(logger, nameof(logger));

            PageNavigateCommand = pageNavigateCommand;
            InstanceListStorage = instanceListStorage;
            _logger = logger;
            InstanceList = [];
            CurrentInstance = null;

            WeakReferenceMessenger.Default.Register<MinecraftInstanceChangedMessage>(this);
            WeakReferenceMessenger.Default.Register<MinecraftInstanceModifiedMessage>(this);
            WeakReferenceMessenger.Default.Register<MainWindowClosingMessage>(this);
            WeakReferenceMessenger.Default.Register<PageNavigatingFromMessage, string>(this, PageToken);
            WeakReferenceMessenger.Default.Register<PageNavigatedToMessage, string>(this, PageToken);
        }

        private bool _instanceChanged = true;
        private bool _instanceModified = false;

        protected readonly IInstanceListStorage InstanceListStorage;
        private readonly ILogger<LaunchViewModel> _logger;

        public PageNavigateCommand PageNavigateCommand { get; }

        protected virtual string PageToken => nameof(Pages.Home.LaunchPage);

        [ObservableProperty]
        public partial ObservableCollection<MinecraftInstanceConfig.Model> InstanceList { get; set; }

        [ObservableProperty]
        public partial MinecraftInstanceConfig.Model? CurrentInstance { get; set; }

        partial void OnCurrentInstanceChanged(MinecraftInstanceConfig.Model? oldValue, MinecraftInstanceConfig.Model? newValue)
        {
            string oldInstanceName = oldValue?.InstanceName ?? string.Empty;
            string newInstanceName = newValue?.InstanceName ?? string.Empty;

            if (oldInstanceName != newInstanceName)
                WeakReferenceMessenger.Default.Send(new MinecraftInstanceChangedMessage(oldInstanceName, newInstanceName));
        }

        void IRecipient<MinecraftInstanceChangedMessage>.Receive(MinecraftInstanceChangedMessage message)
        {
            _instanceChanged = true;
        }

        void IRecipient<MinecraftInstanceModifiedMessage>.Receive(MinecraftInstanceModifiedMessage message)
        {
            _instanceModified = true;
        }

        async void IRecipient<MainWindowClosingMessage>.Receive(MainWindowClosingMessage message)
        {
            await SaveCurrentInstance();
        }

        async void IRecipient<PageNavigatingFromMessage>.Receive(PageNavigatingFromMessage message)
        {
            if (!message.EventArgs.Cancel)
                await SaveCurrentInstance();
        }

        async void IRecipient<PageNavigatedToMessage>.Receive(PageNavigatedToMessage message)
        {
            if (_instanceChanged || _instanceModified)
                await Reload();
        }

        [RelayCommand]
        public async Task Reload()
        {
            MinecraftInstanceIndex instanceIndex;
            try
            {
                instanceIndex = await InstanceListStorage.GetInstanceIndexAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "实例列表重载失败，无法获取实例索引");
                return;
            }

            if (instanceIndex.InstanceList.Count == 0)
            {
                InstanceList = [];
                CurrentInstance = null;
            }
            else
            {
                if (_instanceModified || !InstanceList.Select(s => s.InstanceName).SequenceEqual(instanceIndex.InstanceList))
                {
                    List<MinecraftInstanceConfig.Model> instanceList = [];
                    foreach (string instanceName in instanceIndex.InstanceList)
                    {
                        try
                        {
                            IConfigStorage configStorage = InstanceListStorage.GetInstanceStorage(instanceName);
                            IConfigService configService = await configStorage.LoadConfigAsync();
                            var instance = (MinecraftInstanceConfig.Model)configService.GetCurrentConfig();
                            instanceList.Add(instance);
                        }
                        catch (FileNotFoundException fnfex)
                        {
                            _logger.LogWarning(fnfex, "实例列表重载时，实例“{InstanceName}”的配置文件未找到，已跳过", instanceName);
                            try
                            {
                                await InstanceListStorage.RemoveInstanceAsync(instanceName);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "实例列表重载时，实例“{InstanceName}”的配置文件未找到，尝试从索引中移除失败", instanceName);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "实例列表重载失败，实例“{InstanceName}”无法加载", instanceName);
                            return;
                        }
                    }
                    InstanceList = new ObservableCollection<MinecraftInstanceConfig.Model>(instanceList);
                }

                if (_instanceModified || !instanceIndex.CurrentInstance.Equals(GetInstanceName()))
                    CurrentInstance = InstanceList.FirstOrDefault(i => i.InstanceName == instanceIndex.CurrentInstance);
            }

            _instanceModified = false;
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("实例列表重载成功，当前选中实例: \"{InstanceName}\"", GetInstanceName());
        }

        [RelayCommand]
        public async Task SaveCurrentInstance()
        {
            if (!_instanceChanged)
                return;

            try
            {
                await InstanceListStorage.SetCurrentInstanceAsync(GetInstanceName());
                _instanceChanged = false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "当前选中实例“{InstanceName}”无法写入索引", GetInstanceName());
            }
        }

        private string GetInstanceName()
        {
            return CurrentInstance?.InstanceName ?? string.Empty;
        }
    }
}
