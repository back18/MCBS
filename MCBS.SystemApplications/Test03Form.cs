using MCBS.BlockForms;
using MCBS.Event;
using QuanLib.Minecraft.Block;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.SystemApplications
{
    public class Test03Form : WindowForm
    {
        public override void Initialize()
        {
            base.Initialize();

            ClientPanel.Skin.SetAllBackgroundBlockID(BlockManager.Concrete.Lime);
        }

        protected override void OnCursorSlotChanged(Control sender, CursorSlotEventArgs e)
        {
            base.OnCursorSlotChanged(sender, e);

            Console.WriteLine(e.Delta);
        }
    }
}
