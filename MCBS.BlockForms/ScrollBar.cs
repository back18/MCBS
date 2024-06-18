using QuanLib.Minecraft.Blocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MCBS.BlockForms.Utility;

namespace MCBS.BlockForms
{
    public abstract class ScrollBar : Control
    {
        protected ScrollBar()
        {
            DisplayPriority = int.MaxValue;
            MaxDisplayPriority = int.MaxValue;
            _SliderSize = 1;
            _SliderPosition = 0;

            Skin.SetForegroundColor(BlockManager.Concrete.LightGray, ControlState.None, ControlState.Selected);
            Skin.SetForegroundColor(BlockManager.Concrete.LightBlue, ControlState.Hover, ControlState.Hover | ControlState.Selected);
        }

        public double SliderSize
        {
            get => _SliderSize;
            set
            {
                if (value < 0)
                    value = 0;
                else if (value > 1)
                    value = 1;

                if (_SliderSize != value)
                {
                    _SliderSize = value;
                    RequestRedraw();
                }
            }
        }
        private double _SliderSize;

        public double SliderPosition
        {
            get => _SliderPosition;
            set
            {
                if (value < 0)
                    value = 0;
                else if (value > 1)
                    value = 1;

                if (_SliderPosition != value)
                {
                    _SliderPosition = value;
                    RequestRedraw();
                }
            }
        }
        private double _SliderPosition;
    }
}
