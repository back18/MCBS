using MCBS.Cursor;
using MCBS.Events;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.BlockForms
{
    public class LatticeMultilineTextBox : LatticeMultilineTextControl
    {
        public LatticeMultilineTextBox()
        {
            _indexs = [];
        }

        private readonly Dictionary<string, int> _indexs;

        protected override void OnCursorMove(Control sender, CursorEventArgs e)
        {
            base.OnCursorMove(sender, e);

            if (IsReadOnly || e.CursorContext.NewInputData.CursorMode != CursorMode.TextEditor)
            {
                e.CursorContext.Visible = true;
                return;
            }

            e.CursorContext.Visible = false;
            int newIndex = PagePosToTextIndex(e.Position);
            int oldIndex;
            if (_indexs.TryGetValue(e.CursorContext.PlayerName, out var index))
                oldIndex = index;
            else
                oldIndex = -1;

            if (newIndex == oldIndex)
                return;

            string text = GetSubText(newIndex);
            e.CursorContext.TextEditor.SetInitialText(text);

            HighlightedCharacters.Remove(oldIndex);
            if (!HighlightedCharacters.Contains(newIndex))
                HighlightedCharacters.Add(newIndex);

            _indexs[e.CursorContext.PlayerName] = newIndex;
            RequestRedraw();
        }

        protected override void OnCursorEnter(Control sender, CursorEventArgs e)
        {
            base.OnCursorEnter(sender, e);

            if (IsReadOnly || e.CursorContext.NewInputData.CursorMode != CursorMode.TextEditor || _indexs.ContainsKey(e.CursorContext.PlayerName))
                return;

            e.CursorContext.Visible = false;
            int index = PagePosToTextIndex(e.Position);
            HighlightedCharacters.Add(index);
            _indexs.Add(e.CursorContext.PlayerName, index);
            RequestRedraw();
        }

        protected override void OnCursorLeave(Control sender, CursorEventArgs e)
        {
            base.OnCursorLeave(sender, e);

            if (IsReadOnly || !_indexs.TryGetValue(e.CursorContext.PlayerName, out var index))
                return;

            e.CursorContext.Visible = true;
            HighlightedCharacters.Remove(index);
            _indexs.Remove(e.CursorContext.PlayerName);
            RequestRedraw();
        }

        protected override void OnTextEditorUpdate(Control sender, CursorEventArgs e)
        {
            base.OnTextEditorUpdate(sender, e);

            if (IsReadOnly || !_indexs.TryGetValue(e.CursorContext.PlayerName, out var index))
                return;

            string newText = e.NewData.TextEditor;
            string oldText = GetSubText(index);
            if (newText == oldText)
                return;

            TextBuffer.Remove(index, oldText.Length);
            TextBuffer.Insert(index, newText);
            TextBufferUpdated();
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

        private int PagePosToTextIndex(Point pagePosition)
        {
            return BufferPosToTextIndex(PagePosBufferPos(pagePosition));
        }

        private int BufferPosToTextIndex(Point bufferPosition)
        {
            var character = GetCharacter(bufferPosition);
            return LineBuffer.Lines[character.LineNumber].TextIndex + character.ColumnNumber;
        }

        private string GetSubText(int index)
        {
            int start = index;
            int end = Math.Min(index + 64, Text.Length);
            return Text[start..end];
        }
    }
}
