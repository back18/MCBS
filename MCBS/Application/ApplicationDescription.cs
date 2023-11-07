using QuanLib.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Application
{
    public class ApplicationDescription
    {
        public ApplicationDescription(Model model)
        {
            NullValidator.ValidateObject(model, nameof(model));

            ID = model.ID;
            Version = Version.Parse(model.Version);
            Mandatory = model.Mandatory;
        }

        public string ID { get; }

        public Version Version { get; }

        public bool Mandatory { get; }

        public class Model
        {
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
            public string ID { get; set; }

            public string Version { get; set; }

            public bool Mandatory { get; }
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        }
    }
}
