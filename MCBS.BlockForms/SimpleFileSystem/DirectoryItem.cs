using QuanLib.Minecraft.Blocks;
using MCBS.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.BlockForms.SimpleFileSystem
{
    public class DirectoryItem : FileSystemItem
    {
        public DirectoryItem(DirectoryInfo directoryInfo) : base(QuanLib.Core.IO.PathType.Directory)
        {
            DirectoryInfo = directoryInfo ?? throw new ArgumentNullException(nameof(directoryInfo));

            Text = directoryInfo.Name;
            Skin.SetBackgroundColor(BlockManager.Concrete.Yellow, ControlState.None, ControlState.Hover);
            Skin.SetBackgroundColor(BlockManager.Concrete.Orange, ControlState.Selected, ControlState.Hover | ControlState.Selected);
        }

        public override FileSystemInfo FileSystemInfo => DirectoryInfo;

        public DirectoryInfo DirectoryInfo { get; }
    }
}
