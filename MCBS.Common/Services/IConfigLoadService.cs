using QuanLib.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.Common.Services
{
    public interface IConfigLoadService
    {
        public T Load<T>(string filePath) where T : IDataModel<T>;

        public T Load<T>(Stream inputStream) where T : IDataModel<T>;

        public T LoadOrCreate<T>(string filePath) where T : IDataModel<T>;
    }
}
