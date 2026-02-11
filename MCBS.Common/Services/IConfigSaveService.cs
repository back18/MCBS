using QuanLib.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.Common.Services
{
    public interface IConfigSaveService
    {
        public void Save<T>(T dataModel, string filePath) where T : IDataModel<T>;

        public void Save<T>(T dataModel, Stream outputStream) where T : IDataModel<T>;

        public void CreateIfNotExists<T>(string filePath) where T : IDataModel<T>;
    }
}
