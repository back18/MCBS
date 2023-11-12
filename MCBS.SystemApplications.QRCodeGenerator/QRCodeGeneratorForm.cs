using MCBS.BlockForms;
using MCBS.BlockForms.Utility;
using MCBS.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.SystemApplications.QRCodeGenerator
{
    public class QRCodeGeneratorForm : WindowForm
    {
        public QRCodeGeneratorForm()
        {
            QRCodeBox_Control = new();
            Input_TextBox = new();
            Generate_Button = new();
        }

        private readonly QRCodeBox QRCodeBox_Control;

        private readonly TextBox Input_TextBox;

        private readonly Button Generate_Button;

        public override void Initialize()
        {
            base.Initialize();

            ClientPanel_Control.Resize += ClientPanel_Control_Resize;

            ClientPanel_Control.ChildControls.Add(Input_TextBox);
            Input_TextBox.Size = new(ClientPanel_Control.ClientSize.Width - 40, 18);
            Input_TextBox.ClientLocation = new(2, ClientPanel_Control.ClientSize.Height - Input_TextBox.Height - 2);
            Input_TextBox.Stretch = Direction.Right;
            Input_TextBox.Anchor = Direction.Bottom;

            ClientPanel_Control.ChildControls.Add(Generate_Button);
            Generate_Button.Text = "生成";
            Generate_Button.ClientSize = new(32, 16);
            Generate_Button.LayoutRight(ClientPanel_Control, Input_TextBox, 2);
            Generate_Button.Anchor = Direction.Bottom | Direction.Right;
            Generate_Button.RightClick += Generate_Button_RightClick;

            ClientPanel_Control.ChildControls.Add(QRCodeBox_Control);
            QRCodeBox_Control.Size = new(ClientPanel_Control.ClientSize.Height - 24, ClientPanel_Control.ClientSize.Height - 24);
            QRCodeBox_Control.LayoutHorizontalCentered(ClientPanel_Control, 2);
        }

        private void ClientPanel_Control_Resize(Control sender, SizeChangedEventArgs e)
        {
            QRCodeBox_Control.Size = new(ClientPanel_Control.ClientSize.Height - 24, ClientPanel_Control.ClientSize.Height - 24);
            QRCodeBox_Control.LayoutHorizontalCentered(ClientPanel_Control, 2);
        }

        private void Generate_Button_RightClick(Control sender, CursorEventArgs e)
        {
            if (string.IsNullOrEmpty(Input_TextBox.Text))
                return;

            QRCodeBox_Control.Text = Input_TextBox.Text;
        }
    }
}
