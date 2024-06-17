using MCBS.BlockForms.Utility;
using MCBS.Events;
using QuanLib.Core;
using QuanLib.Minecraft.Blocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.BlockForms.SimpleFileSystem
{
    public abstract class FileSystemItem : Switch
    {
        protected FileSystemItem(PathType pathType)
        {
            PathType = pathType;

            AutoSize = true;
            Skin.SetBorderColor(BlockManager.Concrete.Pink, ControlState.Hover, ControlState.Hover | ControlState.Selected);
        }

        public PathType PathType { get; }

        public abstract FileSystemInfo FileSystemInfo { get; }

        protected override void OnCursorEnter(Control sender, CursorEventArgs e)
        {
            base.OnCursorEnter(sender, e);

            BorderWidth = 2;
            Location = ClientLocation;
        }

        protected override void OnCursorLeave(Control sender, CursorEventArgs e)
        {
            base.OnCursorLeave(sender, e);

            BorderWidth = 1;
            ClientLocation = Location;
        }
    }
}
