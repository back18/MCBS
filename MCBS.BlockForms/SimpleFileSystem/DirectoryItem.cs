using QuanLib.Minecraft.Blocks;
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
            Skin.BackgroundBlockID = BlockManager.Concrete.Yellow;
            Skin.BackgroundBlockID_Hover = BlockManager.Concrete.Yellow;
            Skin.BackgroundBlockID_Selected = BlockManager.Concrete.Orange;
            Skin.BackgroundBlockID_Hover_Selected = BlockManager.Concrete.Orange;
        }

        public override FileSystemInfo FileSystemInfo => DirectoryInfo;

        public DirectoryInfo DirectoryInfo { get; }
    }
}
