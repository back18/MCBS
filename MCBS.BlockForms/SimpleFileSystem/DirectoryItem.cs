using QuanLib.Minecraft.Blocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MCBS.BlockForms.Utility;

namespace MCBS.BlockForms.SimpleFileSystem
{
    public class DirectoryItem : FileSystemItem
    {
        public DirectoryItem(DirectoryInfo directoryInfo) : base(QuanLib.Core.IO.PathType.Directory)
        {
            ArgumentNullException.ThrowIfNull(directoryInfo, nameof(directoryInfo));

            DirectoryInfo = directoryInfo;

            Text = directoryInfo.Name;
            Skin.SetBackgroundColor(BlockManager.Concrete.Yellow, ControlState.None, ControlState.Hover);
            Skin.SetBackgroundColor(BlockManager.Concrete.Orange, ControlState.Selected, ControlState.Hover | ControlState.Selected);
        }

        public override FileSystemInfo FileSystemInfo => DirectoryInfo;

        public DirectoryInfo DirectoryInfo { get; }
    }
}
