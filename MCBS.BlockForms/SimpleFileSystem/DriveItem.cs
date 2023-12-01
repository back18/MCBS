using QuanLib.Minecraft.Blocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MCBS.BlockForms.Utility;

namespace MCBS.BlockForms.SimpleFileSystem
{
    public class DriveItem : FileSystemItem
    {
        public DriveItem(DriveInfo driveInfo) : base(QuanLib.Core.IO.PathType.Drive)
        {
            ArgumentNullException.ThrowIfNull(driveInfo, nameof(driveInfo));

            DriveInfo = driveInfo;

            Text = driveInfo.Name;
            Skin.SetBackgroundColor(BlockManager.Concrete.LightBlue, ControlState.None, ControlState.Hover);
            Skin.SetBackgroundColor(BlockManager.Concrete.Blue, ControlState.Selected, ControlState.Hover | ControlState.Selected);
        }

        public override FileSystemInfo FileSystemInfo => DriveInfo.RootDirectory;

        public DriveInfo DriveInfo { get; }
    }
}
