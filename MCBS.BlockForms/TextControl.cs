using MCBS.Frame;
using MCBS.UI;
using QuanLib.Core.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.BlockForms
{
    public abstract class TextControl : Control
    {
        protected override void OnTextChanged(Control sender, TextChangedEventArgs e)
        {
            base.OnTextChanged(sender, e);

            if (AutoSize)
                AutoSetSize();
        }

        public override IFrame RenderingFrame()
        {
            ArrayFrame? image = Skin.GetBackgroundImage()?.GetFrame();
            if (image is not null && Skin.IsRenderedImageBackground)
            {
                ArrayFrame frame = ArrayFrame.BuildFrame(image!.Width, image.Width, Skin.GetBackgroundBlockID());
                frame.Overwrite(image, new(0, 0));
                image = frame;
            }

            switch (ControlContent)
            {
                case ControlContent.None:
                    return base.RenderingFrame();
                case ControlContent.Text:
                    LinkedFrame fb = RenderingText(Skin.GetForegroundBlockID(), Skin.GetBackgroundBlockID());
                    return fb;
                case ControlContent.Image:
                    return image!;
                case ControlContent.Text | ControlContent.Image:
                    LinkedFrame text = RenderingText(Skin.GetForegroundBlockID(), string.Empty);
                    image!.Overwrite(text.ToArrayFrame(), new(0, 0));
                    return image!;
                default:
                    throw new InvalidOperationException();
            }

            LinkedFrame RenderingText(string foreground, string background)
            {
                LinkedFrame result = new();
                switch (ContentAnchor)
                {
                    case AnchorPosition.UpperLeft:
                    case AnchorPosition.LowerLeft:
                    case AnchorPosition.Centered:
                        foreach (var c in Text)
                        {
                            result.AddRight(ArrayFrame.BuildFrame(SR.DefaultFont[c].GetBitMap(), foreground, background));
                            if (result.Width >= Width)
                                break;
                        }
                        break;
                    case AnchorPosition.UpperRight:
                    case AnchorPosition.LowerRight:
                        for (int i = Text.Length - 1; i >= 0; i--)
                        {
                            result.AddLeft(ArrayFrame.BuildFrame(SR.DefaultFont[Text[i]].GetBitMap(), foreground, background));
                            if (result.Width >= Width)
                                break;
                        }
                        break;
                    default:
                        throw new InvalidOperationException();
                }
                return result;
            }
        }

        public override void AutoSetSize()
        {
            ClientSize = SR.DefaultFont.GetTotalSize(Text);
        }
    }
}
