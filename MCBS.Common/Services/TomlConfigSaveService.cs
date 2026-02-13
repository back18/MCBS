using Nett;
using QuanLib.Core;
using QuanLib.TomlConfig;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.Common.Services
{
    public class TomlConfigSaveService(TomlSettings? defaultSettings = null) : ITomlConfigSaveService
    {
        private static readonly TomlSettings DefaultSettings = TomlSettings.Create();
        private readonly TomlSettings _defaultSettings = defaultSettings ?? DefaultSettings;

        public void Save<T>(T dataModel, string filePath) where T : IDataModel<T>
        {
            Save(dataModel, filePath, _defaultSettings);
        }

        public void Save<T>(T dataModel, string filePath, TomlSettings settings) where T : IDataModel<T>
        {
            ArgumentNullException.ThrowIfNull(dataModel, nameof(dataModel));
            ArgumentNullException.ThrowIfNull(settings, nameof(settings));
            ArgumentException.ThrowIfNullOrEmpty(filePath, nameof(filePath));

            TomlTable tomlTable = TomlConfigBuilder.Build(dataModel);
            Toml.WriteFile(tomlTable, filePath, settings);
        }

        public void Save<T>(T dataModel, Stream outputStream) where T : IDataModel<T>
        {
            Save(dataModel, outputStream, _defaultSettings);
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

        public void CreateIfNotExists<T>(string filePath) where T : IDataModel<T>
        {
            CreateIfNotExists<T>(filePath, _defaultSettings);
        }

        public void CreateIfNotExists<T>(string filePath, TomlSettings settings) where T : IDataModel<T>
        {
            if (!File.Exists(filePath))
            {
                T def = T.CreateDefault();
                Save(def, filePath, settings);
            }
        }
    }
}
