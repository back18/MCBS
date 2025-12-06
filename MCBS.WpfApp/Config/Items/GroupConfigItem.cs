using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.WpfApp.Config.Items
{
    public class GroupConfigItem : ConfigItem
    {
        public required ConfigItem[] ConfigItems { get; init; }
    }
}
