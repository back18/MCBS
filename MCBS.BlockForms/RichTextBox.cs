using MCBS.Cursor;
using MCBS.Events;
using MCBS.Rendering;
using MCBS.UI;
using QuanLib.BDF;
using QuanLib.Core.Events;
using QuanLib.Minecraft.Blocks;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.BlockForms
{
    public class RichTextBox : ScrollablePanel
    {
        public RichTextBox()
        {
            Lines = new(Array.Empty<string>());
            IsReadOnly = true;
            Skin.SetBackgroundColor(BlockManager.Concrete.LightBlue, ControlState.Selected, ControlState.Hover | ControlState.Selected);

            _text = new();
            _WordWrap = true;
        }

        private readonly StringBuilder _text;

        public ReadOnlyCollection<string> Lines { get; private set; }

        public bool IsReadOnly { get; set; }

        public override string Text
        {
            get => _text.ToString();
            set
            {
                string temp = _text.ToString();
                if (temp != value)
                {
                    _text.Clear();
                    _text.Append(value);
                    HandleTextChanged(new(temp, value));
                    RequestRendering();
                }
            }
        }

        public bool WordWrap
        {
            get => _WordWrap;
            set
            {
                if (_WordWrap != value)
                {
                    _WordWrap = value;
                    RequestRendering();
                }
            }
        }
        private bool _WordWrap;

        protected override BlockFrame Rendering()
        {
            BlockFrame baseFrame = base.Rendering();
            if (Lines.Count == 0)
                return baseFrame;

            Point start = OffsetPosition;
            Point end = new(OffsetPosition.X + ClientSize.Width, OffsetPosition.Y + ClientSize.Height);
            Point position = new(0, start.Y / SR.DefaultFont.Height * SR.DefaultFont.Height);
            for (int i = start.Y / SR.DefaultFont.Height; i < Lines.Count; i++)
            {
                if (position.Y > end.Y)
                    break;

                foreach (char c in Lines[i])
                {
                    FontData fontData = SR.DefaultFont[c];
                    if (position.X > end.X)
                        break;
                    if (position.X + fontData.Width < start.X)
                    {
                        position.X += fontData.Width;
                        break;
                    }

                    baseFrame.DrawBinary(fontData.GetBinary(), GetForegroundColor().ToBlockId(), position);
                    position.X += fontData.Width;
                }

                position.X = 0;
                position.Y += SR.DefaultFont.Height;
            }

            return baseFrame;
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

            string[] lines = e.NewText.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            if (WordWrap)
            {
                if (ClientSize.Width < SR.DefaultFont.FullWidth)
                {
                    Lines = new(Array.Empty<string>());
                    PageSize = ClientSize;
                }

                List<string> words = new();
                foreach (string line in lines)
                {
                    int start = 0;
                    int width = 0;
                    for (int i = 0; i < line.Length; i++)
                    {
                        var data = SR.DefaultFont[line[i]];
                        width += data.Width;
                        if (width > ClientSize.Width)
                        {
                            words.Add(line[start..i]);
                            start = i;
                            width = data.Width;
                        }
                    }
                    words.Add(line[start..line.Length]);
                }

                Lines = new(words);
                PageSize = new(ClientSize.Width, Lines.Count * SR.DefaultFont.Height);
            }
            else
            {
                int maxWidth = 0;
                foreach (string line in lines)
                {
                    int width = SR.DefaultFont.GetTotalSize(line).Width;
                    if (width > maxWidth)
                        maxWidth = width;
                }

                Lines = new(lines);
                PageSize = new(maxWidth, Lines.Count * SR.DefaultFont.Height);
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
