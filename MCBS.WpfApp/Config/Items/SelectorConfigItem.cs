using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace MCBS.WpfApp.Config.Items
{
    public class SelectorConfigItem : ConfigItem
    {
        public required ReadOnlyCollection<object?> AllowedValues { get; init; }
    }
}
