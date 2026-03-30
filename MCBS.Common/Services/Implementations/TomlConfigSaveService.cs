using Nett;
using QuanLib.Core;
using QuanLib.TomlConfig;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.Common.Services.Implementations
{
    public class TomlConfigSaveService(TomlSettings? defaultSettings = null) : ITomlConfigSaveService
    {
        private static readonly TomlSettings DefaultSettings = TomlSettings.Create();
        private readonly TomlSettings _defaultSettings = defaultSettings ?? DefaultSettings;

        public void Save<T>(T dataModel, string filePath) where T : IDataModel<T>
        {
            Save(dataModel, filePath, _defaultSettings);
        }

        public Task SaveAsync<T>(T dataModel, string filePath) where T : IDataModel<T>
        {
            return SaveAsync(dataModel, filePath, _defaultSettings);
        }

        public void Save<T>(T dataModel, string filePath, TomlSettings settings) where T : IDataModel<T>
        {
            ArgumentNullException.ThrowIfNull(dataModel, nameof(dataModel));
            ArgumentNullException.ThrowIfNull(settings, nameof(settings));
            ArgumentException.ThrowIfNullOrEmpty(filePath, nameof(filePath));

            TomlTable tomlTable = TomlConfigBuilder.Build(dataModel);
            Toml.WriteFile(tomlTable, filePath, settings);
        }

        public async Task SaveAsync<T>(T dataModel, string filePath, TomlSettings settings) where T : IDataModel<T>
        {
            ArgumentNullException.ThrowIfNull(dataModel, nameof(dataModel));
            ArgumentNullException.ThrowIfNull(settings, nameof(settings));
            ArgumentException.ThrowIfNullOrEmpty(filePath, nameof(filePath));

            using FileStream fileStream = File.Create(filePath);
            using MemoryStream memoryStream = CreateTomlStream(dataModel, settings);
            await memoryStream.CopyToAsync(fileStream);
            await fileStream.FlushAsync();
        }

        public void Save<T>(T dataModel, Stream outputStream) where T : IDataModel<T>
        {
            Save(dataModel, outputStream, _defaultSettings);
        }

        public Task SaveAsync<T>(T dataModel, Stream outputStream) where T : IDataModel<T>
        {
            return SaveAsync(dataModel, outputStream, _defaultSettings);
        }

        public void Save<T>(T dataModel, Stream outputStream, TomlSettings settings) where T : IDataModel<T>
        {
            ArgumentNullException.ThrowIfNull(dataModel, nameof(dataModel));
            ArgumentNullException.ThrowIfNull(settings, nameof(settings));
            ArgumentNullException.ThrowIfNull(outputStream, nameof(outputStream));
            ThrowHelper.StreamNotSupportWrite(outputStream);

            TomlTable tomlTable = TomlConfigBuilder.Build(dataModel);
            Toml.WriteStream(tomlTable, outputStream, settings);
        }

        public async Task SaveAsync<T>(T dataModel, Stream outputStream, TomlSettings settings) where T : IDataModel<T>
        {
            ArgumentNullException.ThrowIfNull(dataModel, nameof(dataModel));
            ArgumentNullException.ThrowIfNull(settings, nameof(settings));
            ArgumentNullException.ThrowIfNull(outputStream, nameof(outputStream));
            ThrowHelper.StreamNotSupportWrite(outputStream);

            using MemoryStream memoryStream = CreateTomlStream(dataModel, settings);
            await memoryStream.CopyToAsync(outputStream);
            await outputStream.FlushAsync();
        }

        public void CreateIfNotExists<T>(string filePath) where T : IDataModel<T>
        {
            CreateIfNotExists<T>(filePath, _defaultSettings);
        }

        public Task CreateIfNotExistsAsync<T>(string filePath) where T : IDataModel<T>
        {
            return CreateIfNotExistsAsync<T>(filePath, _defaultSettings);
        }

        public void CreateIfNotExists<T>(string filePath, TomlSettings settings) where T : IDataModel<T>
        {
            if (!File.Exists(filePath))
            {
                T def = T.CreateDefault();
                Save(def, filePath, settings);
            }
        }

        public async Task CreateIfNotExistsAsync<T>(string filePath, TomlSettings settings) where T : IDataModel<T>
        {
            if (!File.Exists(filePath))
            {
                T def = T.CreateDefault();
                await SaveAsync(def, filePath, settings);
            }
        }

        private static MemoryStream CreateTomlStream<T>(T dataModel, TomlSettings settings) where T : IDataModel<T>
        {
            TomlTable tomlTable = TomlConfigBuilder.Build(dataModel);
            MemoryStream memoryStream = new();
            Toml.WriteStream(tomlTable, memoryStream, settings);
            memoryStream.Position = 0;
            return memoryStream;
        }
    }
}
