using MCBS.BlockForms;
using MCBS.Events;
using QuanLib.Minecraft.Blocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.SystemApplications.ScreenController
{
    public class ControllerPanel : Panel<Button>
    {
        public ControllerPanel()
        {
            BorderWidth = 0;

            Up_Button = new();
            Down_Button = new();
            Left_Button = new();
            Right_Button = new();
            Center_Button = new();
        }

        internal readonly Button Up_Button;

        internal readonly Button Down_Button;

        internal readonly Button Left_Button;

        internal readonly Button Right_Button;

        internal readonly Button Center_Button;

        public override void Initialize()
        {
            base.Initialize();

            ChildControls.Add(Up_Button);
            Up_Button.Text = "Up";
            Up_Button.Skin.SetAllBackgroundColor(BlockManager.Concrete.Pink);

            ChildControls.Add(Down_Button);
            Down_Button.Text = "Down";
            Down_Button.Skin.SetAllBackgroundColor(BlockManager.Concrete.LightBlue);

            ChildControls.Add(Left_Button);
            Left_Button.Text = "Left";
            Left_Button.Skin.SetAllBackgroundColor(BlockManager.Concrete.Yellow);

            ChildControls.Add(Right_Button);
            Right_Button.Text = "Right";
            Right_Button.Skin.SetAllBackgroundColor(BlockManager.Concrete.Lime);

            ChildControls.Add(Center_Button);
            Center_Button.Text = "Center";
            Center_Button.Skin.SetAllBackgroundColor(BlockManager.Concrete.Orange);

            ActiveLayoutAll();
        }

        protected override void OnResize(Control sender, SizeChangedEventArgs e)
        {
            base.OnResize(sender, e);
        }

        protected override void OnLayoutAll(AbstractControlContainer<Control> sender, SizeChangedEventArgs e)
        {
            base.OnLayoutAll(sender, e);

            ActiveLayoutAll();
        }

        public override void ActiveLayoutAll()
        {
            int edgeWidth = ClientSize.Width / 3;
            int edgeHeight = ClientSize.Height / 3;
            int centerWidth = ClientSize.Width - edgeWidth * 2;
            int centerHeight = ClientSize.Height - edgeHeight * 2;

            int x1 = 1;
            int x2 = x1 + edgeWidth;
            int x3 = x2 + centerWidth;

            int y1 = 1;
            int y2 = y1 + edgeHeight;
            int y3 = y2 + centerHeight;

            edgeWidth -= 2;
            edgeHeight -= 2;
            centerWidth -= 2;
            centerHeight -= 2;

            Up_Button.ClientLocation = new(x2, y1);
            Up_Button.Size = new(centerWidth, edgeHeight);

            Down_Button.ClientLocation = new(x2, y3);
            Down_Button.Size = new(centerWidth, edgeHeight);

            Left_Button.ClientLocation = new(x1, y2);
            Left_Button.Size = new(edgeWidth, centerHeight);

            Right_Button.ClientLocation = new(x3, y2);
            Right_Button.Size = new(edgeWidth, centerHeight);

            Center_Button.ClientLocation = new(x2, y2);
            Center_Button.Size = new(centerWidth, centerHeight);
        }
    }
}
