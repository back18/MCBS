using Nett;
using QuanLib.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.Common.Services
{
    public interface ITomlConfigLoadService : IConfigLoadService
    {
        public T Load<T>(string filePath, TomlSettings settings) where T : IDataModel<T>;

        public T Load<T>(Stream inputStream, TomlSettings settings) where T : IDataModel<T>;

        public T LoadOrCreate<T>(string filePath, TomlSettings settings) where T : IDataModel<T>;
    }
}
