using Newtonsoft.Json;
using QuanLib.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.Common.Services.Implementations
{
    public class JsonConfigSaveService : IJsonConfigSaveService
    {
        public void Save<T>(T dataModel, string filePath) where T : IDataModel<T>
        {
            Save(dataModel, filePath, Formatting.None, null);
        }

        public Task SaveAsync<T>(T dataModel, string filePath) where T : IDataModel<T>
        {
            return SaveAsync(dataModel, filePath, Formatting.None, null);
        }

        public void Save<T>(T dataModel, string filePath, Formatting formatting) where T : IDataModel<T>
        {
            Save(dataModel, filePath, formatting, null);
        }

        public Task SaveAsync<T>(T dataModel, string filePath, Formatting formatting) where T : IDataModel<T>
        {
            return SaveAsync(dataModel, filePath, formatting, null);
        }

        public void Save<T>(T dataModel, string filePath, JsonSerializerSettings? settings) where T : IDataModel<T>
        {
            Save(dataModel, filePath, Formatting.None, settings);
        }

        public Task SaveAsync<T>(T dataModel, string filePath, JsonSerializerSettings? settings) where T : IDataModel<T>
        {
            return SaveAsync(dataModel, filePath, Formatting.None, settings);
        }

        public void Save<T>(T dataModel, string filePath, Formatting formatting, JsonSerializerSettings? settings) where T : IDataModel<T>
        {
            ArgumentNullException.ThrowIfNull(dataModel, nameof(dataModel));
            ArgumentException.ThrowIfNullOrEmpty(filePath, nameof(filePath));

            string json = JsonConvert.SerializeObject(dataModel, formatting, settings);
            File.WriteAllText(filePath, json);
        }

        public async Task SaveAsync<T>(T dataModel, string filePath, Formatting formatting, JsonSerializerSettings? settings) where T : IDataModel<T>
        {
            ArgumentNullException.ThrowIfNull(dataModel, nameof(dataModel));
            ArgumentException.ThrowIfNullOrEmpty(filePath, nameof(filePath));

            string json = JsonConvert.SerializeObject(dataModel, formatting, settings);
            await File.WriteAllTextAsync(filePath, json);
        }

        public void Save<T>(T dataModel, Stream outputStream) where T : IDataModel<T>
        {
            Save(dataModel, outputStream, Formatting.None, null);
        }

        public Task SaveAsync<T>(T dataModel, Stream outputStream) where T : IDataModel<T>
        {
            return SaveAsync(dataModel, outputStream, Formatting.None, null);
        }

        public void Save<T>(T dataModel, Stream outputStream, Formatting formatting) where T : IDataModel<T>
        {
            Save(dataModel, outputStream, formatting, null);
        }

        public Task SaveAsync<T>(T dataModel, Stream outputStream, Formatting formatting) where T : IDataModel<T>
        {
            return SaveAsync(dataModel, outputStream, formatting, null);
        }

        public void Save<T>(T dataModel, Stream outputStream, JsonSerializerSettings? settings) where T : IDataModel<T>
        {
            Save(dataModel, outputStream, Formatting.None, settings);
        }

        public Task SaveAsync<T>(T dataModel, Stream outputStream, JsonSerializerSettings? settings) where T : IDataModel<T>
        {
            return SaveAsync(dataModel, outputStream, Formatting.None, settings);
        }

        public void Save<T>(T dataModel, Stream outputStream, Formatting formatting, JsonSerializerSettings? settings) where T : IDataModel<T>
        {
            ArgumentNullException.ThrowIfNull(dataModel, nameof(dataModel));
            ArgumentNullException.ThrowIfNull(outputStream, nameof(outputStream));
            ThrowHelper.StreamNotSupportWrite(outputStream);

            using StreamWriter streamWriter = new(outputStream);
            string json = JsonConvert.SerializeObject(dataModel, formatting, settings);
            streamWriter.Write(json);
            streamWriter.Flush();
        }

        public async Task SaveAsync<T>(T dataModel, Stream outputStream, Formatting formatting, JsonSerializerSettings? settings) where T : IDataModel<T>
        {
            ArgumentNullException.ThrowIfNull(dataModel, nameof(dataModel));
            ArgumentNullException.ThrowIfNull(outputStream, nameof(outputStream));
            ThrowHelper.StreamNotSupportWrite(outputStream);

            using StreamWriter streamWriter = new(outputStream);
            string json = JsonConvert.SerializeObject(dataModel, formatting, settings);
            await streamWriter.WriteAsync(json);
            await streamWriter.FlushAsync();
        }

        public void CreateIfNotExists<T>(string filePath) where T : IDataModel<T>
        {
            CreateIfNotExists<T>(filePath, Formatting.None, null);
        }

        public Task CreateIfNotExistsAsync<T>(string filePath) where T : IDataModel<T>
        {
            return CreateIfNotExistsAsync<T>(filePath, Formatting.None, null);
        }

        public void CreateIfNotExists<T>(string filePath, Formatting formatting) where T : IDataModel<T>
        {
            CreateIfNotExists<T>(filePath, formatting, null);
        }

        public Task CreateIfNotExistsAsync<T>(string filePath, Formatting formatting) where T : IDataModel<T>
        {
            return CreateIfNotExistsAsync<T>(filePath, formatting, null);
        }

        public void CreateIfNotExists<T>(string filePath, JsonSerializerSettings? settings) where T : IDataModel<T>
        {
            CreateIfNotExists<T>(filePath, Formatting.None, settings);
        }

        public Task CreateIfNotExistsAsync<T>(string filePath, JsonSerializerSettings? settings) where T : IDataModel<T>
        {
            return CreateIfNotExistsAsync<T>(filePath, Formatting.None, settings);
        }

        public void CreateIfNotExists<T>(string filePath, Formatting formatting, JsonSerializerSettings? settings) where T : IDataModel<T>
        {
            if (!File.Exists(filePath))
            {
                T def = T.CreateDefault();
                Save(def, filePath, formatting, settings);
            }
        }

        public async Task CreateIfNotExistsAsync<T>(string filePath, Formatting formatting, JsonSerializerSettings? settings) where T : IDataModel<T>
        {
            if (!File.Exists(filePath))
            {
                T def = T.CreateDefault();
                await SaveAsync(def, filePath, formatting, settings);
            }
        }
    }
}
