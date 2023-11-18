using MCBS.BlockForms.Utility;
using MCBS.Cursor;
using MCBS.Events;
using QuanLib.Core.Events;
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
            ClientSize = new(64, 16);
            Skin.SetBackgroundColor(BlockManager.Concrete.LightBlue, ControlState.Selected, ControlState.Hover | ControlState.Selected);
        }

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

            UpdateSelected();
        }

        protected override void OnTextChanged(Control sender, TextChangedEventArgs e)
        {
            base.OnTextChanged(sender, e);

            if (!IsReadOnly)
            {
                foreach (CursorContext cursorContext in GetHoverTextEditorCursors())
                    cursorContext.TextEditor.SetInitialText(Text);
            }
        }

        protected override void OnTextEditorUpdate(Control sender, CursorEventArgs e)
        {
            base.OnTextEditorUpdate(sender, e);

            if (!IsReadOnly)
                Text = e.NewData.TextEditor;
        }

        public CursorContext[] GetHoverTextEditorCursors()
        {
            CursorContext[] cursorContexts = GetHoverCursors();
            List<CursorContext> result = new();
            foreach (CursorContext cursorContext in cursorContexts)
            {
                if (cursorContext.NewInputData.CursorMode == CursorMode.TextEditor)
                    result.Add(cursorContext);
            }

            return result.ToArray();
        }

        private void HandleInput(CursorEventArgs e)
        {
            if (IsReadOnly)
                return;

            if (e.CursorContext.NewInputData.CursorMode == CursorMode.TextEditor)
            {
                IsSelected = true;
                e.CursorContext.TextEditor.SetInitialText(Text);
            }
            else
            {
                UpdateSelected();
            }
        }

        private void UpdateSelected()
        {
            if (!IsReadOnly && GetHoverTextEditorCursors().Length > 0)
                IsSelected = true;
            else
                IsSelected = false;
        }
    }
}
