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
    public class McapiModeConfig : IDataModelOwner<McapiModeConfig, McapiModeConfig.Model>
    {
        private McapiModeConfig(Model model)
        {
            NullValidator.ValidateObject(model, nameof(model));

            Address = model.Address;
            Port = (ushort)model.Port;
            Password = model.Password;
        }

        public string Address { get; }

        public ushort Port { get; }

        public string Password { get; }

        public static McapiModeConfig FromDataModel(Model model)
        {
            return new(model);
        }

        public Model ToDataModel()
        {
            return new()
            {
                Address = Address,
                Port = Port,
                Password = Password
            };
        }

        public class Model : IDataModel<Model>
        {
            public Model()
            {
                Address = "localhost";
                Port = 25585;
                Password = string.Empty;
            }

            [Display(Order = 0, GroupName = nameof(MinecraftConfig), Name = "MCAPI地址")]
            [Required(ErrorMessage = ErrorMessageHelper.RequiredAttribute)]
            public string Address { get; set; }

            [Display(Order = 1, GroupName = nameof(MinecraftConfig), Name = "MCAPI端口")]
            [Range(ushort.MinValue, ushort.MaxValue, ErrorMessage = ErrorMessageHelper.RangeAttribute)]
            public int Port { get; set; }

            [Display(Order = 2, GroupName = nameof(MinecraftConfig), Name = "MCAPI密码")]
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
