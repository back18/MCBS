using QuanLib.Minecraft.Blocks;
using MCBS.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.BlockForms.SimpleFileSystem
{
    public class DriveItem : FileSystemItem
    {
        public DriveItem(DriveInfo driveInfo) : base(QuanLib.Core.IO.PathType.Drive)
        {
            DriveInfo = driveInfo ?? throw new ArgumentNullException(nameof(driveInfo));

            Text = driveInfo.Name;
            Skin.SetBackgroundColor(BlockManager.Concrete.LightBlue, ControlState.None, ControlState.Hover);
            Skin.SetBackgroundColor(BlockManager.Concrete.Blue, ControlState.Selected, ControlState.Hover | ControlState.Selected);
        }

        public override FileSystemInfo FileSystemInfo => DriveInfo.RootDirectory;

        public DriveInfo DriveInfo { get; }
    }
}
