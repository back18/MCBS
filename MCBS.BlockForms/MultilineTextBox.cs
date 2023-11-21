using MCBS.Cursor;
using MCBS.Events;
using MCBS.BlockForms.Utility;
using QuanLib.Minecraft.Blocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuanLib.Core.Events;

namespace MCBS.BlockForms
{
    public class MultilineTextBox : MultilineTextControl
    {
        public MultilineTextBox()
        {
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
