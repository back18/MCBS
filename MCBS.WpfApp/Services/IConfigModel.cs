using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.WpfApp.Services
{
    public interface IConfigModel
    {
        public Type Type { get; }

        public object CreateDefault();

        public bool Validate(object? model);
    }
}
