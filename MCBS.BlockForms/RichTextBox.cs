﻿using MCBS.Cursor;
using MCBS.Events;
using MCBS.Frame;
using QuanLib.BDF;
using QuanLib.Minecraft.Blocks;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.BlockForms
{
    public class RichTextBox : ScrollablePanel
    {
        public RichTextBox()
        {
            IsReadOnly = true;
            Skin.BackgroundBlockID_Selected = BlockManager.Concrete.LightBlue;
            Skin.BackgroundBlockID_Hover_Selected = BlockManager.Concrete.LightBlue;

            _frame = ArrayFrame.BuildFrame(PageSize, Skin.GetBackgroundBlockID());
            _text = new();
            _lines = new();
            _WordWrap = true;
        }

        private ArrayFrame _frame;

        private readonly StringBuilder _text;

        private readonly List<string> _lines;

        public IReadOnlyList<string> Lines => _lines;

        public int LineCount => _lines.Count;

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
                    RequestUpdateFrame();
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
                    RequestUpdateFrame();
                }
            }
        }
        private bool _WordWrap;

        public override IFrame RenderingFrame()
        {
            ActiveLayoutAll();
            return _frame;
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

        protected override void OnTextEditorUpdate(Control sender, CursorEventArgs e)
        {
            base.OnTextEditorUpdate(sender, e);

            if (!IsReadOnly)
                Text = e.NewData.TextEditor;
        }

        public override void ActiveLayoutAll()
        {
            _lines.Clear();
            BdfFont font = SR.DefaultFont;
            string[] lines = _text.ToString().Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            if (WordWrap)
            {
                if (ClientSize.Width < font.FullWidth)
                {
                    PageSize = ClientSize;
                    _frame = ArrayFrame.BuildFrame(PageSize, Skin.GetBackgroundBlockID());
                    return;
                }

                foreach (string line in lines)
                {
                    int start = 0;
                    int width = 0;
                    for (int i = 0; i < line.Length; i++)
                    {
                        var data = font[line[i]];
                        width += data.Width;
                        if (width > ClientSize.Width)
                        {
                            _lines.Add(line[start..i]);
                            start = i;
                            width = data.Width;
                        }
                    }

                    _lines.Add(line[start..line.Length]);
                }
                PageSize = new(ClientSize.Width, _lines.Count * font.Height);
            }
            else
            {
                _lines.AddRange(lines);
                int height = _lines.Count * font.Height;
                int width = 0;
                foreach (var line in _lines)
                {
                    int lineWidth = font.GetTotalSize(line).Width;
                    if (lineWidth > width)
                        width = lineWidth;
                }
                PageSize = new(width, height);
            }

            _frame = ArrayFrame.BuildFrame(PageSize, Skin.GetBackgroundBlockID());
            Point position = new(0, 0);
            foreach (var line in _lines)
            {
                foreach (var c in line)
                {
                    var data = font[c];
                    _frame.Overwrite(ArrayFrame.BuildFrame(data.GetBinary(), Skin.GetForegroundBlockID(), Skin.GetBackgroundBlockID()), position);
                    position.X += data.Width;
                }
                position.X = 0;
                position.Y += font.Height;
            }
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
