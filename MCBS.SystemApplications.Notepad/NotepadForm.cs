using MCBS.BlockForms;
using MCBS.BlockForms.DialogBox;
using QuanLib.Game;
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
            Text_MultilineTextBox = new();

            _open = open;
        }

        private string? _open;

        private readonly MultilineTextBox Text_MultilineTextBox;

        public override void Initialize()
        {
            base.Initialize();

            Home_PagePanel.ChildControls.Add(Text_MultilineTextBox);
            Text_MultilineTextBox.ClientLocation = new(2, 2);
            Text_MultilineTextBox.Size = new(Home_PagePanel.ClientSize.Width - 4, Home_PagePanel.ClientSize.Height - 4);
            Text_MultilineTextBox.Stretch = Direction.Bottom | Direction.Right;
        }

        public override void AfterInitialize()
        {
            base.AfterInitialize();

            if (_open is not null)
            {
                try
                {
                    Text_MultilineTextBox.Text = File.ReadAllText(_open);
                }
                catch
                {
                    _ = DialogBoxHelper.OpenMessageBoxAsync(this, "警告", $"无法打开文本文件：“{_open}”", MessageBoxButtons.OK);
                }
            }
        }
    }
}
