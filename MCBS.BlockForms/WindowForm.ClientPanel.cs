using MCBS.Rendering;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.BlockForms
{
    public abstract partial class WindowForm
    {
        public class ClientPanel : ScrollablePanel
        {
            public ClientPanel()
            {
                BorderWidth = 0;
            }
        }
    }
}
