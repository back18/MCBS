using MCBS.Cursor;
using MCBS.Events;
using QuanLib.Minecraft.Blocks;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.BlockForms
{
    public class TextBox : TextControl
    {
        public TextBox()
        {
            IsReadOnly = false;

            ClientSize = new(64, 16);
            Skin.BackgroundBlockID_Selected = BlockManager.Concrete.LightBlue;
            Skin.BackgroundBlockID_Hover_Selected = BlockManager.Concrete.LightBlue;
        }

        public bool IsReadOnly { get; set; }

        protected override void OnCursorMove(Control sender, CursorEventArgs e)
        {
            base.OnCursorMove(sender, e);

            HandleInput(e);
        }

        protected override void OnCursorEnter(Control sender, CursorEventArgs e)
        {
            base.OnCursorEnter(sender, e);

            HandleInput(e);
        }

        protected override void OnCursorLeave(Control sender, CursorEventArgs e)
        {
            base.OnCursorLeave(sender, e);

            IsSelected = false;
        }

        protected override void OnTextEditorUpdate(Control sender, CursorEventArgs e)
        {
            base.OnTextEditorUpdate(sender, e);

            if (!IsReadOnly)
                Text = e.CursorContext.TextEditor.CurrentText;
        }

        private void HandleInput(CursorEventArgs e)
        {
            if (!IsReadOnly && e.CursorContext.InputData.CursorMode == CursorMode.TextEditor)
            {
                IsSelected = true;
                e.CursorContext.TextEditor.SetInitialText(Text);
            }
            else
            {
                IsSelected = false;
            }
        }
    }
}
