using QuanLib.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Application
{
    public class ApplicationDependencie
    {
        public ApplicationDependencie(Model model)
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
            public required string ID { get; set; }

            public required string Version { get; set; }

            public required bool Mandatory { get; set; }
        }
    }
}
