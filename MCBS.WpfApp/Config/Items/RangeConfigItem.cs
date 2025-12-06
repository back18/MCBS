using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.WpfApp.Config.Items
{
    public class RangeConfigItem : ConfigItem
    {
        public required object Minimum { get; set; }

        public required object Maximum { get; set; }
    }
}
