using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.WpfApp.Config.Items
{
    public class ListConfigItem : ConfigItem
    {
        public required List<object?> Items;

        public required Type ItemType;
    }
}
