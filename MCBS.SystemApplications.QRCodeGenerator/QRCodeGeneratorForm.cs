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

            Home_PagePanel.Resize += ClientPanel_Control_Resize;

            Home_PagePanel.ChildControls.Add(Input_TextBox);
            Input_TextBox.Size = new(Home_PagePanel.ClientSize.Width - 40, 18);
            Input_TextBox.ClientLocation = new(2, Home_PagePanel.ClientSize.Height - Input_TextBox.Height - 2);
            Input_TextBox.Stretch = Direction.Right;
            Input_TextBox.Anchor = Direction.Bottom;

            Home_PagePanel.ChildControls.Add(Generate_Button);
            Generate_Button.Text = "生成";
            Generate_Button.ClientSize = new(32, 16);
            Generate_Button.LayoutRight(Home_PagePanel, Input_TextBox, 2);
            Generate_Button.Anchor = Direction.Bottom | Direction.Right;
            Generate_Button.RightClick += Generate_Button_RightClick;

            Home_PagePanel.ChildControls.Add(QRCodeBox_Control);
            QRCodeBox_Control.Size = new(Home_PagePanel.ClientSize.Height - 24, Home_PagePanel.ClientSize.Height - 24);
            QRCodeBox_Control.LayoutHorizontalCentered(Home_PagePanel, 2);
        }

        private void ClientPanel_Control_Resize(Control sender, SizeChangedEventArgs e)
        {
            QRCodeBox_Control.Size = new(Home_PagePanel.ClientSize.Height - 24, Home_PagePanel.ClientSize.Height - 24);
            QRCodeBox_Control.LayoutHorizontalCentered(Home_PagePanel, 2);
        }

        private void Generate_Button_RightClick(Control sender, CursorEventArgs e)
        {
            if (string.IsNullOrEmpty(Input_TextBox.Text))
                return;

            QRCodeBox_Control.Text = Input_TextBox.Text;
        }
    }
}
