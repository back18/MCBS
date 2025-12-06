using QuanLib.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MCBS.WpfApp.Config
{
    public class ConfigDataModel<TModel> : IConfigModel where TModel : IDataModel<TModel>
    {
        public Type Type => typeof(TModel);

        public object CreateDefault()
        {
            return TModel.CreateDefault();
        }

        public bool Validate(object? config)
        {
            return config is not null && Validator.TryValidateObject(config, new(config), [], true);
        }
    }
}
