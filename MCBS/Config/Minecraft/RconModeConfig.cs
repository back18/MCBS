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
    public class RconModeConfig : IDataModelOwner<RconModeConfig, RconModeConfig.Model>
    {
        private RconModeConfig(Model model)
        {
            NullValidator.ValidateObject(model, nameof(model));

            Port = (ushort)model.Port;
            Password = model.Password;
        }

        public ushort Port { get; }

        public string Password { get; }

        public static RconModeConfig FromDataModel(Model model)
        {
            return new(model);
        }

        public Model ToDataModel()
        {
            return new()
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

            [Display(Name = "RCON端口")]
            [Range(ushort.MinValue, ushort.MaxValue, ErrorMessage = ErrorMessageHelper.RangeAttribute)]
            public int Port { get; set; }

            [Display(Name = "RCON密码")]
            [Required(ErrorMessage = ErrorMessageHelper.RequiredAttribute)]
            public string Password { get; set; }

            public static Model CreateDefault()
            {
                return new();
            }

            public static void Validate(Model model, string name)
            {
                ValidationHelper.Validate(model, name);
            }
        }
    }
}
