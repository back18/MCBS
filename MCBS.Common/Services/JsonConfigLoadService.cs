using Newtonsoft.Json;
using QuanLib.Core;
using QuanLib.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace MCBS.Common.Services
{
    public class JsonConfigLoadService : IJsonConfigLoadService
    {
        public T Load<T>(string filePath) where T : IDataModel<T>
        {
            return Load<T>(filePath, null);
        }

        public T Load<T>(string filePath, JsonSerializerSettings? settings) where T : IDataModel<T>
        {
            ThrowHelper.FileNotFound(filePath);

            string json = File.ReadAllText(filePath);
            return LoadFromJson<T>(json, Path.GetFullPath(filePath), settings);
        }

        public T Load<T>(Stream inputStream) where T : IDataModel<T>
        {
            return Load<T>(inputStream, null);
        }

        public T Load<T>(Stream inputStream, JsonSerializerSettings? settings) where T : IDataModel<T>
        {
            ArgumentNullException.ThrowIfNull(inputStream, nameof(inputStream));
            ThrowHelper.StreamNotSupportRead(inputStream);

            using StreamReader reader = new(inputStream);
            string json = reader.ReadToEnd();
            return LoadFromJson<T>(json, null, settings);
        }

        public T LoadOrCreate<T>(string filePath) where T : IDataModel<T>
        {
            return LoadOrCreate<T>(filePath, null);
        }

        public T LoadOrCreate<T>(string filePath, JsonSerializerSettings? settings) where T : IDataModel<T>
        {
            if (File.Exists(filePath))
                return Load<T>(filePath, settings);
            else
                return T.CreateDefault();
        }

        private static T LoadFromJson<T>(string json, string? displayName, JsonSerializerSettings? settings) where T : IDataModel<T>
        {
            var model = JsonConvert.DeserializeObject<T>(json, settings) ?? T.CreateDefault();
            model.ThrowIfFailed(displayName);
            return model;
        }
    }
}
