﻿using MCBS.Events;
using MCBS.Frame;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.UI
{
    public interface IControlRendering
    {
        public bool Visible { get; set; }

        public Point ClientLocation { get; set; }

        public Size ClientSize { get; set; }

        public Point OffsetPosition { get; set; }

        public int BorderWidth { get; set; }

        public AnchorPosition ContentAnchor { get; set; }

        public ISkin Skin { get; }

        public bool NeedRendering { get; }

        public IFrame RenderingFrame();

        public ArrayFrame? GetFrameCache();

        public void HandleRenderingCompleted(ArrayFrameEventArgs e);
    }
}
