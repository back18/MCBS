using MCBS.Event;
using MCBS.UI;
using QuanLib.Minecraft.Block;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.BlockForms
{
    public class TaskBarIcon : IconTextBox
    {
        public TaskBarIcon(IForm form)
        {
            Form = form ?? throw new ArgumentNullException(nameof(form));

            Icon_PictureBox.SetImage(form.Icon);
            Text_Label.Text = form.Text;
            AutoSetSize();
            Skin.BorderBlockID = BlockManager.Concrete.Gray;
            Skin.BorderBlockID_Selected = BlockManager.Concrete.Orange;
            Skin.BorderBlockID__Hover = BlockManager.Concrete.Pink;
            Skin.BorderBlockID_Hover_Selected = BlockManager.Concrete.Pink;

            Form.FormMinimize += Form_FormMinimize;
            Form.FormUnminimize += Form_FormUnminimize;
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
                IRootForm? rootForm = MCOS.Instance?.FormContextOf(Form)?.RootForm;
                if (rootForm is null)
                    return;
                if (rootForm.ContainsForm(Form))
                    rootForm.TrySwitchSelectedForm(Form);
                else
                    Form.UnminimizeForm();
            }
        }

        private void Form_FormMinimize(IForm sender, EventArgs e)
        {
            IsSelected = Form.IsSelected;
        }

        private void Form_FormUnminimize(IForm sender, EventArgs e)
        {
            IsSelected = Form.IsSelected;
        }
    }
}
