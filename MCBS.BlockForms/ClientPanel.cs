using MCBS.Frame;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFrame = MCBS.Frame.ImageFrame;

namespace MCBS.BlockForms
{
    public class ClientPanel : ScrollablePanel
    {
        public ClientPanel()
        {
            BorderWidth = 0;
        }

        public override IFrame RenderingFrame()
        {
            ImageFrame? image = Skin.GetBackgroundImage();
            if (image is null)
                return base.RenderingFrame();

            Size size = GetRenderingSize();
            if (image.FrameSize != size)
            {
                image.ResizeOptions.Size = size;
                image.Update();
            }

            return image.GetFrameClone();
        }
    }
}
