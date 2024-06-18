using MCBS.BlockForms.Utility;
using MCBS.Drawing;
using MCBS.Drawing.Extensions;
using QuanLib.Core.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.BlockForms
{
    public abstract class TextControl : Control
    {
        protected override void OnTextChanged(Control sender, ValueChangedEventArgs<string> e)
        {
            base.OnTextChanged(sender, e);

            if (AutoSize)
                AutoSetSize();
        }

        protected override BlockFrame Drawing()
        {
            BlockFrame baseFrame = base.Drawing();
            if (string.IsNullOrEmpty(Text))
                return baseFrame;

            HashBlockFrame fontFrame = new(SR.DefaultFont.GetTotalSize(Text));
            int x = 0;
            foreach (char c in Text)
            {
                OverwriteContext overwriteContext = fontFrame.DrawBinary(SR.DefaultFont[c].GetBitArray(), GetForegroundColor().ToBlockId(), new(x, 0));
                x = overwriteContext.BaseEndPosition.X + 1;
            }

            baseFrame.Overwrite(fontFrame, ContentAnchor);
            return baseFrame;
        }

        public override void AutoSetSize()
        {
            ClientSize = SR.DefaultFont.GetTotalSize(Text);
        }
    }
}
