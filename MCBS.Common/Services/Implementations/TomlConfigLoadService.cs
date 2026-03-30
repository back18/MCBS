using Nett;
using QuanLib.Core;
using QuanLib.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.Common.Services.Implementations
{
    public class TomlConfigLoadService(TomlSettings? defaultSettings = null) : ITomlConfigLoadService
    {
        private static readonly TomlSettings DefaultSettings = TomlSettings.Create();
        private readonly TomlSettings _defaultSettings = defaultSettings ?? DefaultSettings;

        public T Load<T>(string filePath) where T : IDataModel<T>
        {
            return Load<T>(filePath, _defaultSettings);
        }

        public Task<T> LoadAsync<T>(string filePath) where T : IDataModel<T>
        {
            return LoadAsync<T>(filePath, _defaultSettings);
        }

        public T Load<T>(string filePath, TomlSettings settings) where T : IDataModel<T>
        {
            ArgumentNullException.ThrowIfNull(settings, nameof(settings));
            ThrowHelper.FileNotFound(filePath);

            TomlTable tomlTable = Toml.ReadFile(filePath, settings);
            return Load<T>(tomlTable, Path.GetFullPath(filePath));
        }

        public async Task<T> LoadAsync<T>(string filePath, TomlSettings settings) where T : IDataModel<T>
        {
            ArgumentNullException.ThrowIfNull(settings, nameof(settings));
            ThrowHelper.FileNotFound(filePath);

            string toml = await File.ReadAllTextAsync(filePath);
            return LoadFromToml<T>(toml, Path.GetFullPath(filePath), settings);
        }

        public T Load<T>(Stream inputStream) where T : IDataModel<T>
        {
            return Load<T>(inputStream, _defaultSettings);
        }

        public Task<T> LoadAsync<T>(Stream inputStream) where T : IDataModel<T>
        {
            return LoadAsync<T>(inputStream, _defaultSettings);
        }

        public T Load<T>(Stream inputStream, TomlSettings settings) where T : IDataModel<T>
        {
            ArgumentNullException.ThrowIfNull(settings, nameof(settings));
            ArgumentNullException.ThrowIfNull(inputStream, nameof(inputStream));
            ThrowHelper.StreamNotSupportRead(inputStream);

            TomlTable tomlTable = Toml.ReadStream(inputStream, settings);
            return Load<T>(tomlTable);
        }

        public async Task<T> LoadAsync<T>(Stream inputStream, TomlSettings settings) where T : IDataModel<T>
        {
            ArgumentNullException.ThrowIfNull(settings, nameof(settings));
            ArgumentNullException.ThrowIfNull(inputStream, nameof(inputStream));
            ThrowHelper.StreamNotSupportRead(inputStream);

            using StreamReader reader = new(inputStream, Encoding.UTF8, true, -1, true);
            string toml = await reader.ReadToEndAsync();
            return LoadFromToml<T>(toml, null, settings);
        }

        public T LoadOrCreate<T>(string filePath) where T : IDataModel<T>
        {
            return LoadOrCreate<T>(filePath, _defaultSettings);
        }

        public Task<T> LoadOrCreateAsync<T>(string filePath) where T : IDataModel<T>
        {
            return LoadOrCreateAsync<T>(filePath, _defaultSettings);
        }

        public T LoadOrCreate<T>(string filePath, TomlSettings settings) where T : IDataModel<T>
        {
            if (File.Exists(filePath))
                return Load<T>(filePath, settings);
            else
                return T.CreateDefault();
        }

        public async Task<T> LoadOrCreateAsync<T>(string filePath, TomlSettings settings) where T : IDataModel<T>
        {
            if (File.Exists(filePath))
                return await LoadAsync<T>(filePath, settings);
            else
                return T.CreateDefault();
        }

        public static T Load<T>(TomlTable tomlTable, string? displayName = null) where T : IDataModel<T>
        {
            ArgumentNullException.ThrowIfNull(tomlTable, nameof(tomlTable));

            var model = tomlTable.Get<T>();
            model.ThrowIfFailed(displayName);
            return model;
        }

        private static T LoadFromToml<T>(string toml, string? displayName, TomlSettings settings) where T : IDataModel<T>
        {
            using MemoryStream memoryStream = new(Encoding.UTF8.GetBytes(toml));
            TomlTable tomlTable = Toml.ReadStream(memoryStream, settings);
            return Load<T>(tomlTable, displayName);
        }
    }
}
