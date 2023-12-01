using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.BlockForms
{
    public class PagePanel : ScrollablePanel
    {
        public PagePanel(string pageKey)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(pageKey, nameof(pageKey));

            PageKey = pageKey;
            BorderWidth = 0;
        }

        public string PageKey { get; }
    }
}
