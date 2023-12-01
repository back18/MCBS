using MCBS.Application;
using MCBS.BlockForms;
using MCBS.BlockForms.Utility;
using MCBS.Events;
using QuanLib.Minecraft.Blocks;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.BlockForms.DialogBox
{
    public class ApplicationItem : IconTextBox<Rgba32>
    {
        public ApplicationItem(ApplicationManifest applicationManifest)
        {
            ArgumentNullException.ThrowIfNull(applicationManifest, nameof(applicationManifest));

            ApplicationManifest = applicationManifest;

            Skin.SetBackgroundColor(BlockManager.Concrete.White, ControlState.None, ControlState.Hover);
            Skin.SetBackgroundColor(BlockManager.Concrete.LightGray, ControlState.Selected, ControlState.Hover | ControlState.Selected);
        }

        public ApplicationManifest ApplicationManifest { get; }

        public override void Initialize()
        {
            base.Initialize();

            Icon_PictureBox.SetImage(ApplicationManifest.GetIcon());
            Text_Label.Text = ApplicationManifest.Name;
            Text_Label.AutoSetSize();
        }

        protected override void OnRightClick(Control sender, CursorEventArgs e)
        {
            base.OnRightClick(sender, e);

            IsSelected = !IsSelected;
        }
    }
}
