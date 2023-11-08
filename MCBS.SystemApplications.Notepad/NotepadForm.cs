using MCBS.BlockForms;
using MCBS.BlockForms.DialogBox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.SystemApplications.Notepad
{
    public class NotepadForm : WindowForm
    {
        public NotepadForm(string? open = null)
        {
            RichTextBox = new();

            _open = open;
        }

        private string? _open;

        private readonly RichTextBox RichTextBox;

        public override void Initialize()
        {
            base.Initialize();

            ClientPanel.ChildControls.Add(RichTextBox);
            RichTextBox.ClientLocation = new(2, 2);
            RichTextBox.Size = new(ClientPanel.ClientSize.Width - 4, ClientPanel.ClientSize.Height - 4);
            RichTextBox.Stretch = Direction.Bottom | Direction.Right;
        }

        public override void AfterInitialize()
        {
            base.AfterInitialize();

            if (_open is not null)
            {
                try
                {
                    RichTextBox.Text = File.ReadAllText(_open);
                }
                catch
                {
                    _ = DialogBoxHelper.OpenMessageBoxAsync(this, "警告", $"无法打开文本文件：“{_open}”", MessageBoxButtons.OK);
                }
            }
        }
    }
}
