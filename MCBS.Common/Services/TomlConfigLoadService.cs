using Nett;
using QuanLib.Core;
using QuanLib.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.Common.Services
{
    public class TomlConfigLoadService(TomlSettings? defaultSettings = null) : ITomlConfigLoadService
    {
        private static readonly TomlSettings DefaultSettings = TomlSettings.Create();
        private readonly TomlSettings _defaultSettings = defaultSettings ?? DefaultSettings;

        public T Load<T>(string filePath) where T : IDataModel<T>
        {
            return Load<T>(filePath, _defaultSettings);
        }

        public T Load<T>(string filePath, TomlSettings settings) where T : IDataModel<T>
        {
            ArgumentNullException.ThrowIfNull(settings, nameof(settings));
            ThrowHelper.FileNotFound(filePath);

            TomlTable tomlTable = Toml.ReadFile(filePath, settings);
            return Load<T>(tomlTable, Path.GetFullPath(filePath));
        }

        public T Load<T>(Stream inputStream) where T : IDataModel<T>
        {
            return Load<T>(inputStream, _defaultSettings);
        }

        public T Load<T>(Stream inputStream, TomlSettings settings) where T : IDataModel<T>
        {
            ArgumentNullException.ThrowIfNull(settings, nameof(settings));
            ArgumentNullException.ThrowIfNull(inputStream, nameof(inputStream));
            ThrowHelper.StreamNotSupportRead(inputStream);

            TomlTable tomlTable = Toml.ReadStream(inputStream, settings);
            return Load<T>(tomlTable);
        }

        public T LoadOrCreate<T>(string filePath) where T : IDataModel<T>
        {
            return LoadOrCreate<T>(filePath, _defaultSettings);
        }

        public T LoadOrCreate<T>(string filePath, TomlSettings settings) where T : IDataModel<T>
        {
            if (File.Exists(filePath))
                return Load<T>(filePath, settings);
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
    }
}
