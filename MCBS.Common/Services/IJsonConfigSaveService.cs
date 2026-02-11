using Newtonsoft.Json;
using QuanLib.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.Common.Services
{
    public interface IJsonConfigSaveService : IConfigSaveService
    {
        public void Save<T>(T dataModel, string filePath, Formatting formatting) where T : IDataModel<T>;

        public void Save<T>(T dataModel, Stream outputStream, Formatting formatting) where T : IDataModel<T>;

        public void CreateIfNotExists<T>(string filePath, Formatting formatting) where T : IDataModel<T>;

        public void Save<T>(T dataModel, string filePath, JsonSerializerSettings? settings) where T : IDataModel<T>;

        public void Save<T>(T dataModel, Stream outputStream, JsonSerializerSettings? settings) where T : IDataModel<T>;

        public void CreateIfNotExists<T>(string filePath, JsonSerializerSettings? settings) where T : IDataModel<T>;

        public void Save<T>(T dataModel, string filePath, Formatting formatting, JsonSerializerSettings? settings) where T : IDataModel<T>;

        public void Save<T>(T dataModel, Stream outputStream, Formatting formatting, JsonSerializerSettings? settings) where T : IDataModel<T>;

        public void CreateIfNotExists<T>(string filePath, Formatting formatting, JsonSerializerSettings? settings) where T : IDataModel<T>;
    }
}
