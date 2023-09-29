using MCBS.UI;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Frame
{
    public interface IFrame
    {
        public int Width { get; }

        public int Height { get; }

        public ArrayFrame ToArrayFrame();

        public LinkedFrame ToLinkedFrame();

        public void CorrectSize(Size size, Point offset, AnchorPosition anchor, string background);
    }
}
