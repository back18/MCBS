using Newtonsoft.Json;
using QuanLib.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.Common.Services
{
    public interface IJsonConfigLoadService : IConfigLoadService
    {
        public T Load<T>(string filePath, JsonSerializerSettings? settings) where T : IDataModel<T>;

        public T Load<T>(Stream inputStream, JsonSerializerSettings? settings) where T : IDataModel<T>;

        public T LoadOrCreate<T>(string filePath, JsonSerializerSettings? settings) where T : IDataModel<T>;
    }
}
