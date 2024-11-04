using MCBS.BlockForms.Utility;
using MCBS.Events;
using MCBS.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.BlockForms.DialogBox
{
    public class TextInputBoxForm : DialogBoxForm<string>
    {
        public TextInputBoxForm(IForm initiator, string title) : base(initiator, title)
        {
            DefaultResult = string.Empty;
            DialogResult = DefaultResult;

            Text_MultilineTextBox = new();
            OK_Button = new();
            Cancel_Button = new();
        }

        private readonly MultilineTextBox Text_MultilineTextBox;

        private readonly Button OK_Button;

        private readonly Button Cancel_Button;

        public override string DefaultResult { get; }

        public override string DialogResult { get; protected set; }

        public override void Initialize()
        {
            base.Initialize();

            ClientSize = new(102, 74 + TitleBar_Control.Height);
            CenterOnInitiatorForm();

            Home_PagePanel.ChildControls.Add(Text_MultilineTextBox);
            Text_MultilineTextBox.ClientLocation = new(2, 2);
            Text_MultilineTextBox.ClientSize = new(96, 48);

            Home_PagePanel.ChildControls.Add(Cancel_Button);
            Cancel_Button.Text = "取消";
            Cancel_Button.ClientSize = new(32, 16);
            Cancel_Button.LayoutLeft(this, Text_MultilineTextBox.BottomLocation + 3, 2);
            Cancel_Button.RightClick += Cancel_Button_RightClick;

            Home_PagePanel.ChildControls.Add(OK_Button);
            OK_Button.Text = "确认";
            OK_Button.ClientSize = new(32, 16);
            OK_Button.LayoutLeft(this, Cancel_Button, 2);
            OK_Button.RightClick += OK_Button_RightClick;
        }

        private void OK_Button_RightClick(Control sender, CursorEventArgs e)
        {
            DialogResult = Text_MultilineTextBox.Text;
            CloseForm();
        }

        private void Cancel_Button_RightClick(Control sender, CursorEventArgs e)
        {
            CloseForm();
        }
    }
}
