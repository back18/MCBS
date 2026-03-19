using QuanLib.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MCBS.WpfApp.Services.Implementations
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
            if (config is null)
                return false;

            ValidationContext validationContext = new(config);
            if (config is TModel model)
                return !model.GetValidator().Validate(validationContext).Any();

            return Validator.TryValidateObject(config, validationContext, null, true);
        }
    }
}
