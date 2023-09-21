using MCBS.Frame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Event
{
    public class VideoFrameChangedEventArgs : EventArgs
    {
        public VideoFrameChangedEventArgs(VideoFrame? oldVideoFrame, VideoFrame? newVideoFrame)
        {
            OldVideoFrame = oldVideoFrame;
            NewVideoFrame = newVideoFrame;
        }

        public VideoFrame? OldVideoFrame { get; }

        public VideoFrame? NewVideoFrame { get; }
    }
}
