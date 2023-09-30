using MCBS.Application;
using MCBS.BlockForms;
using MCBS.Events;
using QuanLib.Minecraft.Block;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.BlockForms.DialogBox
{
    public class ApplicationItem : IconTextBox
    {
        public ApplicationItem(ApplicationInfo appInfo)
        {
            ApplicationInfo = appInfo ?? throw new ArgumentNullException(nameof(appInfo));

            Skin.BackgroundBlockID = Skin.BackgroundBlockID_Hover = BlockManager.Concrete.White;
            Skin.BackgroundBlockID_Selected = Skin.BackgroundBlockID_Hover_Selected = BlockManager.Concrete.LightGray;
        }

        public ApplicationInfo ApplicationInfo { get; }

        public override void Initialize()
        {
            base.Initialize();

            Icon_PictureBox.SetImage(ApplicationInfo.GetIcon());
            Text_Label.Text = ApplicationInfo.Name;
            Text_Label.AutoSetSize();
        }

        protected override void OnRightClick(Control sender, CursorEventArgs e)
        {
            base.OnRightClick(sender, e);

            IsSelected = !IsSelected;
        }
    }
}
