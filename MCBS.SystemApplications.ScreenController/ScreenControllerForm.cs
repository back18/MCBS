using MCBS.BlockForms;
using MCBS.Events;
using MCBS.Screens;
using QuanLib.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.SystemApplications.ScreenController
{
    public class ScreenControllerForm : WindowForm
    {
        public ScreenControllerForm()
        {
            ControllerPanel_Control = new();
        }

        private readonly ControllerPanel ControllerPanel_Control;

        public override void Initialize()
        {
            base.Initialize();

            Home_PagePanel.ChildControls.Add(ControllerPanel_Control);
            ControllerPanel_Control.Size = Home_PagePanel.ClientSize;
            ControllerPanel_Control.Stretch = Direction.Bottom | Direction.Right;
            ControllerPanel_Control.Up_Button.RightClick += Up_Button_RightClick;
            ControllerPanel_Control.Down_Button.RightClick += Down_Button_RightClick;
            ControllerPanel_Control.Left_Button.RightClick += Left_Button_RightClick;
            ControllerPanel_Control.Right_Button.RightClick += Right_Button_RightClick;
            ControllerPanel_Control.Center_Button.RightClick += Center_Button_RightClick;
        }

        private void Up_Button_RightClick(Control sender, CursorEventArgs e)
        {
            ScreenContext? screenContext = MinecraftBlockScreen.Instance.ScreenContextOf(this);
            if (screenContext is null)
                return;

            screenContext.ScreenController.UpRotate();
        }

        private void Down_Button_RightClick(Control sender, CursorEventArgs e)
        {
            ScreenContext? screenContext = MinecraftBlockScreen.Instance.ScreenContextOf(this);
            if (screenContext is null)
                return;

            screenContext.ScreenController.DownRotate();
        }

        private void Left_Button_RightClick(Control sender, CursorEventArgs e)
        {
            ScreenContext? screenContext = MinecraftBlockScreen.Instance.ScreenContextOf(this);
            if (screenContext is null)
                return;

            screenContext.ScreenController.LeftRotate();
        }

        private void Right_Button_RightClick(Control sender, CursorEventArgs e)
        {
            ScreenContext? screenContext = MinecraftBlockScreen.Instance.ScreenContextOf(this);
            if (screenContext is null)
                return;

            screenContext.ScreenController.RightRotate();
        }

        private void Center_Button_RightClick(Control sender, CursorEventArgs e)
        {
            ScreenContext? screenContext = MinecraftBlockScreen.Instance.ScreenContextOf(this);
            if (screenContext is null)
                return;

            screenContext.ScreenController.ClockwiseRotate();
        }
    }
}
