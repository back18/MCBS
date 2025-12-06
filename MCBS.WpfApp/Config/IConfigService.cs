using QuanLib.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace MCBS.WpfApp.Config
{
    public interface IConfigService : INotifyPropertyChanged
    {
        public bool IsModified { get; }

        public IConfigStorage GetConfigStorage();

        public object GetCurrentConfig();

        public object GetDefaultConfig();

        public void NotifyModified(string? propertyName);

        public object? GetPropertyValue(string propertyName);

        public void SetPropertyValue(string propertyName, object? value);
    }
}
