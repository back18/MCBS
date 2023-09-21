using MCBS.BlockForms;
using MCBS.Event;
using QuanLib.Core.IO;
using QuanLib.Minecraft.Block;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
            Skin.BorderBlockID__Hover = BlockManager.Concrete.Pink;
            Skin.BorderBlockID_Hover_Selected = BlockManager.Concrete.Pink;
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
