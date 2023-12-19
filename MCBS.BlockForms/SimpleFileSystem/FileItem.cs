using QuanLib.Minecraft.Blocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MCBS.BlockForms.Utility;

namespace MCBS.BlockForms.SimpleFileSystem
{
    public class FileItem : FileSystemItem
    {
        public FileItem(FileInfo fileInfo) : base(QuanLib.Core.PathType.File)
        {
            FileInfo = fileInfo;

            Text = fileInfo.Name;
            Skin.SetBackgroundColor(BlockManager.Concrete.White, ControlState.None, ControlState.Hover);
            Skin.SetBackgroundColor(BlockManager.Concrete.LightGray, ControlState.Selected, ControlState.Hover | ControlState.Selected);
        }

        public override FileSystemInfo FileSystemInfo => FileInfo;

        public FileInfo FileInfo { get; }
    }
}
