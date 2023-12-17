using MCBS.BlockForms;
using MCBS.Events;
using MCBS.Screens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.SystemApplications.ScreenAdjuster
{
    public class ScreenAdjusterForm : WindowForm
    {
        public ScreenAdjusterForm()
        {
            AdjusterPanel_Control = new();
        }

        private readonly AdjusterPanel AdjusterPanel_Control;

        public override void Initialize()
        {
            base.Initialize();

            Home_PagePanel.ChildControls.Add(AdjusterPanel_Control);
            AdjusterPanel_Control.Size = Home_PagePanel.ClientSize;
            AdjusterPanel_Control.Stretch = Direction.Bottom | Direction.Right;
            AdjusterPanel_Control.Up_Button.RightClick += Up_Button_RightClick;
            AdjusterPanel_Control.Down_Button.RightClick += Down_Button_RightClick;
            AdjusterPanel_Control.Left_Button.RightClick += Left_Button_RightClick;
            AdjusterPanel_Control.Right_Button.RightClick += Right_Button_RightClick;
            AdjusterPanel_Control.Center_Button.RightClick += Center_Button_RightClick;
        }

        private void Up_Button_RightClick(Control sender, CursorEventArgs e)
        {
            ScreenContext? screenContext = MCOS.Instance.ScreenContextOf(this);
            if (screenContext is null)
                return;

            screenContext.ScreenOutputHandler.FillAirBlock();
            screenContext.Screen.UpRotate();
            screenContext.ScreenOutputHandler.ResetBuffer();
        }

        private void Down_Button_RightClick(Control sender, CursorEventArgs e)
        {
            ScreenContext? screenContext = MCOS.Instance.ScreenContextOf(this);
            if (screenContext is null)
                return;

            screenContext.ScreenOutputHandler.FillAirBlock();
            screenContext.Screen.DownRotate();
            screenContext.ScreenOutputHandler.ResetBuffer();
        }

        private void Left_Button_RightClick(Control sender, CursorEventArgs e)
        {
            ScreenContext? screenContext = MCOS.Instance.ScreenContextOf(this);
            if (screenContext is null)
                return;

            screenContext.ScreenOutputHandler.FillAirBlock();
            screenContext.Screen.LeftRotate();
            screenContext.ScreenOutputHandler.ResetBuffer();
        }

        private void Right_Button_RightClick(Control sender, CursorEventArgs e)
        {
            ScreenContext? screenContext = MCOS.Instance.ScreenContextOf(this);
            if (screenContext is null)
                return;

            screenContext.ScreenOutputHandler.FillAirBlock();
            screenContext.Screen.RightRotate();
            screenContext.ScreenOutputHandler.ResetBuffer();
        }

        private void Center_Button_RightClick(Control sender, CursorEventArgs e)
        {
            ScreenContext? screenContext = MCOS.Instance.ScreenContextOf(this);
            if (screenContext is null)
                return;

            screenContext.ScreenOutputHandler.FillAirBlock();
            screenContext.Screen.ClockwiseRotate();
            screenContext.ScreenOutputHandler.ResetBuffer();
        }
    }
}
