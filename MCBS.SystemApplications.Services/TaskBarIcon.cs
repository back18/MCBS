using MCBS.BlockForms;
using MCBS.BlockForms.Utility;
using MCBS.Events;
using MCBS.UI;
using QuanLib.Minecraft.Blocks;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.SystemApplications.Services
{
    public class TaskBarIcon : IconTextBox<Rgba32>
    {
        public TaskBarIcon(IForm form)
        {
            ArgumentNullException.ThrowIfNull(form, nameof(form));

            Form = form;

            Icon_PictureBox.SetImage(form.GetIcon());
            Text_Label.Text = form.Text;
            AutoSetSize();
            Skin.SetBorderColor(BlockManager.Concrete.Orange, ControlState.Selected);
            Skin.SetBorderColor(BlockManager.Concrete.Pink, ControlState.Hover, ControlState.Hover | ControlState.Selected);
        }

        public IForm Form { get; }

        protected override void OnRightClick(Control sender, CursorEventArgs e)
        {
            base.OnRightClick(sender, e);

            if (IsSelected)
            {
                Form.MinimizeForm();
            }
            else
            {
                IRootForm? rootForm = MinecraftBlockScreen.Instance?.FormContextOf(Form)?.RootForm;
                if (rootForm is null)
                    return;
                if (rootForm.ContainsForm(Form))
                    rootForm.TrySwitchSelectedForm(Form);
                else
                    Form.UnminimizeForm();
            }
        }
    }
}
