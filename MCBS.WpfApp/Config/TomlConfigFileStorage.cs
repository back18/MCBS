using Nett;
using QuanLib.TomlConfig;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Text;

namespace MCBS.WpfApp.Config
{
    public class TomlConfigFileStorage : IConfigStorage
    {
        public TomlConfigFileStorage(IConfigModel configModel, string filePath, Encoding encoding)
        {
            ArgumentNullException.ThrowIfNull(configModel, nameof(configModel));
            ArgumentException.ThrowIfNullOrEmpty(filePath, nameof(filePath));
            ArgumentNullException.ThrowIfNull(encoding, nameof(encoding));

            _configModel = configModel;

            FilePath = filePath;
            Encoding = encoding;
        }

        private object? _config;

        private ConfigService? _configService;

        private readonly IConfigModel _configModel;

        public string FilePath { get; }

        public Encoding Encoding { get; }

        public bool IsExists => File.Exists(FilePath);

        public bool IsLoaded => _configService is not null;

        public event EventHandler? Saved;

        public IConfigModel GetModel()
        {
            return _configModel;
        }

        public IConfigService GetConfig()
        {
            return _configService ?? throw new InvalidOperationException("Configuration not loaded");
        }

        public IConfigService CreateConfig(bool save)
        {
            if (_configService is not null)
                throw new InvalidOperationException("Cannot create configuration repeatedly");

            object config = _configModel.CreateDefault();
            _configService = new ConfigService(config, _configModel, this);

            if (save)
                SaveConfig();

            return _configService;
        }

        public async Task<IConfigService> CreateConfigAsync(bool save)
        {
            if (_configService is not null)
                throw new InvalidOperationException("Cannot create configuration repeatedly");

            _config = _configModel.CreateDefault();
            _configService = new ConfigService(_config, _configModel, this);

            if (save)
                await SaveConfigAsync();

            return _configService;
        }

        public IConfigService LoadConfig()
        {
            if (_configService is not null)
                return _configService;

            string toml = File.ReadAllText(FilePath, Encoding);
            TomlTable table = Toml.ReadString(toml);

            _config = table.Get(_configModel.Type);
            _configService = new ConfigService(_config, _configModel, this);
            return _configService;
        }

        public async Task<IConfigService> LoadConfigAsync()
        {
            if (_configService is not null)
                return _configService;

            string toml = await File.ReadAllTextAsync(FilePath, Encoding);
            TomlTable table = Toml.ReadString(toml);

            _config = table.Get(_configModel.Type);
            _configService = new ConfigService(_config, _configModel, this);
            return _configService;
        }

        public void SaveConfig()
        {
            if (_configService is null)
                throw new InvalidOperationException("Configuration not loaded");

            TomlTable tomlTable = TomlConfigBuilder.Build(_configService.GetCurrentConfig());
            string toml = tomlTable.ToString();

            File.WriteAllText(FilePath, toml, Encoding);
            Saved?.Invoke(this, EventArgs.Empty);
        }

        public async Task SaveConfigAsync()
        {
            if (_configService is null)
                throw new InvalidOperationException("Configuration not loaded");

            TomlTable tomlTable = TomlConfigBuilder.Build(_configService.GetCurrentConfig());
            string toml = tomlTable.ToString();

            await File.WriteAllTextAsync(FilePath, toml, Encoding);
            Saved?.Invoke(this, EventArgs.Empty);
        }
    }
}
