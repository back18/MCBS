using Nett;
using QuanLib.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.Common.Services
{
    public interface ITomlConfigSaveService : IConfigSaveService
    {
        public void Save<T>(T dataModel, string filePath, TomlSettings settings) where T : IDataModel<T>;

        public void Save<T>(T dataModel, Stream outputStream, TomlSettings settings) where T : IDataModel<T>;

        public void CreateIfNotExists<T>(string filePath, TomlSettings settings) where T : IDataModel<T>;
    }
}
