using QuanLib.Core;
using QuanLib.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Config.Minecraft
{
    public class RconModeConfig : IDataViewModel<RconModeConfig>
    {
        private RconModeConfig(Model model)
        {
            NullValidator.ValidateObject(model, nameof(model));

            Port = (ushort)model.Port;
            Password = model.Password;
        }

        public ushort Port { get; }

        public string Password { get; }

        public static RconModeConfig FromDataModel(object model)
        {
            return new RconModeConfig((Model)model);
        }

        public object ToDataModel()
        {
            return new Model()
            {
                Port = Port,
                Password = Password
            };
        }

        public class Model : IDataModel<Model>
        {
            public Model()
            {
                Port = 25575;
                Password = string.Empty;
            }

            [Display(Order = 0, GroupName = nameof(MinecraftConfig), Name = "RCON端口")]
            [Range(ushort.MinValue, ushort.MaxValue, ErrorMessage = ErrorMessageHelper.RangeAttribute)]
            public int Port { get; set; }

            [Display(Order = 1, GroupName = nameof(MinecraftConfig), Name = "RCON密码")]
            [Required(ErrorMessage = ErrorMessageHelper.RequiredAttribute)]
            public string Password { get; set; }

            public static Model CreateDefault()
            {
                return new Model();
            }

            public IValidatableObject GetValidator()
            {
                return new ValidatableObject(this);
            }

            public IEnumerable<IValidatable> GetValidatableProperties()
            {
                return Array.Empty<IValidatable>();
            }
        }
    }
}
