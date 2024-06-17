using MCBS.BlockForms.Utility;
using MCBS.Events;
using QuanLib.Minecraft.Blocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.BlockForms
{
    public class Switch : TextControl
    {
        public Switch()
        {
            _OnText = string.Empty;
            _OffText = string.Empty;

            Skin.SetBackgroundColor(BlockManager.Concrete.Red, ControlState.None, ControlState.Hover);
            Skin.SetBackgroundColor(BlockManager.Concrete.Lime, ControlState.Selected, ControlState.Hover | ControlState.Selected);
            ContentAnchor = AnchorPosition.Centered;
        }

        public override void Initialize()
        {
            base.Initialize();

            if (IsSelected)
            {
                if (!string.IsNullOrEmpty(OnText))
                    Text = OnText;
            }
            else
            {
                if (!string.IsNullOrEmpty(OffText))
                    Text = OffText;
            }
        }

        protected override void OnRightClick(Control sender, CursorEventArgs e)
        {
            base.OnRightClick(sender, e);

            IsSelected = !IsSelected;
        }

        protected override void OnControlSelected(Control sender, EventArgs e)
        {
            base.OnControlSelected(sender, e);

            if (!string.IsNullOrEmpty(OnText))
                Text = OnText;
        }

        protected override void OnControlDeselected(Control sender, EventArgs e)
        {
            base.OnControlDeselected(sender, e);

            if (!string.IsNullOrEmpty(OffText))
                Text = OffText;
        }

        public string OnText
        {
            get => _OnText;
            set
            {
                if (_OnText != value)
                {
                    _OnText = value;
                    if (!string.IsNullOrEmpty(_OnText) && IsSelected)
                        Text = OnText;
                }
            }
        }
        private string _OnText;

        public string OffText
        {
            get => _OffText;
            set
            {
                if (_OffText != value)
                {
                    _OffText = value;
                    if (!string.IsNullOrEmpty(_OffText) && !IsSelected)
                        RequestRendering();
                    Text = OffText;
                }
            }
        }
        private string _OffText;
    }
}
