using QuanLib.Game;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.UI
{
    public interface IForm : IContainerControl, IFormEventHandling
    {
        public bool AllowSelected { get; set; }

        public bool AllowDeselected { get; set; }

        public bool AllowDrag { get; set; }

        public bool AllowStretch { get; set; }

        public object? ReturnValue { get; }

        public Image<Rgba32> GetIcon();

        public Direction GetStretchingBorders(Point position);

        public void MinimizeForm();

        public void UnminimizeForm();

        public void CloseForm();
    }
}
